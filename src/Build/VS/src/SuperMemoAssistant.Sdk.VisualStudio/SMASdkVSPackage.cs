#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2020/03/31 00:04
// Modified On:  2020/04/04 16:54
// Modified By:  Alexis

#endregion




// ReSharper disable SuspiciousTypeConversion.Global
namespace SuperMemoAssistant.Sdk.VisualStudio
{
  using System;
  using System.Diagnostics;
  using System.Diagnostics.CodeAnalysis;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Threading.Tasks;
  using EnvDTE;
  using EnvDTE80;
  using Microsoft;
  using Microsoft.Build.Framework;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Shell.Interop;
  using Utils.VS;
  using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;
  using Task = System.Threading.Tasks.Task;

  /// <summary>This is the class that implements the package exposed by this assembly.</summary>
  /// <remarks>
  ///   <para>
  ///     The minimum requirement for a class to be considered a valid package for Visual Studio
  ///     is to implement the IVsPackage interface and register itself with the shell. This package
  ///     uses the helper classes defined inside the Managed Package Framework (MPF) to do it: it
  ///     derives from the Package class that provides the implementation of the IVsPackage
  ///     interface and uses the registration attributes defined in the framework to register itself
  ///     and its components with the shell. These attributes tell the pkgdef creation utility what
  ///     data to put into .pkgdef file.
  ///   </para>
  ///   <para>
  ///     To get loaded into VS, the package must be referred by &lt;Asset
  ///     Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
  ///   </para>
  /// </remarks>
  [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
  [PackageRegistration(UseManagedResourcesOnly                                                     = true, AllowsBackgroundLoading = true)]
  [Guid(PackageGuidString)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
  public sealed partial class SMASdkVSPackage
    : AsyncPackage, IVsSolutionEvents, IVsUpdateSolutionEvents2, IVsUpdateSolutionEvents, IVSOutputWindowWriter
  {
    #region Constants & Statics

    /// <summary>SuperMemoAssistant.Sdk.VisualStudioPackage GUID string.</summary>
    public const string PackageGuidString = "c09f5e26-a25e-4642-ab65-bfc4362794ac";

    #endregion




    #region Properties & Fields - Non-Public

    private BuildEvents   _buildEvents;
    private IEventSource3 _eventSrc;

    private IVsSolution              _solution;
    private IVsSolutionBuildManager2 _solutionBuildManager;
    private uint                     _solutionEventsCookie;
    private uint                     _updateSolutionEventsCookie;

    #endregion




    #region Constructors

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      UnregisterEventListeners();
    }

    #endregion




    #region Properties Impl - Public

    public DTE2 Dte2 { get; private set; }
    /// <inheritdoc />
    public IVsOutputWindowPane OutputWindowPane { get; set; }
    /// <inheritdoc />
    public OutputWindowPane OutputWindowPane2 { get; set; }
    /// <inheritdoc />
    public LoggerVerbosity CurrentBuildVerbosity { get; set; }

    #endregion




    #region Methods Impl

    /// <summary>
    ///   Initialization of the package; this method is called right after the package is
    ///   sited, so this is the place where you can put all the initialization code that rely on
    ///   services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">
    ///   A cancellation token to monitor for initialization
    ///   cancellation, which can occur when VS is shutting down.
    /// </param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>
    ///   A task representing the async work of package initialization, or an already completed
    ///   task if there is none. Do not return null from this method.
    /// </returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      // When initialized asynchronously, the current thread may be a background thread at this point.
      // Do any initialization that requires the UI thread after switching to the UI thread.
      await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

      Dte2 = GetGlobalService(typeof(DTE)) as DTE2;

      await this.WriteDebugAsync("Initializing VS extension SuperMemoAssistant.Sdk.VisualStudio...");

      // Since this package might not be initialized until after a solution has finished loading,
      // we need to check if a solution has already been loaded and then handle it.
      if (await IsSolutionLoadedAsync(cancellationToken))
        HandleOpenSolution();

      await InitializePackageAsync();
      RegisterEventListeners();

      await base.InitializeAsync(cancellationToken, progress);
      await this.WriteDebugAsync("Initializing VS extension SuperMemoAssistant.Sdk.VisualStudio... Done.");
    }

    public object GetServiceSync(Type serviceType)
    {
      return GetService(serviceType);
    }

    #endregion




    #region Methods

    private async Task InitializePackageAsync()
    {
      _solution             = (IVsSolution)await GetServiceAsync(typeof(SVsSolution));
      _solutionBuildManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
      _buildEvents          = Dte2.Events.BuildEvents;

      Assumes.Present(_solution);
      Assumes.Present(_solutionBuildManager);
    }

    [Conditional("SUSPENDED")]
    private void RegisterEventListeners()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      // Listen for subsequent solution events
      SolutionEvents.OnAfterBackgroundSolutionLoadComplete += HandleOpenSolution;

      // Build events
      _buildEvents.OnBuildBegin           += OnBuildBegin;
      _buildEvents.OnBuildProjConfigBegin += _buildEvents_OnBuildProjConfigBegin;

      // Solution events
      _solutionBuildManager.AdviseUpdateSolutionEvents(this, out _updateSolutionEventsCookie);
      _solution.AdviseSolutionEvents(this, out _solutionEventsCookie);
      //ProjectCollection.GlobalProjectCollection.ProjectAdded += ProjectAdded;
    }
    
    [Conditional("SUSPENDED")]
    private void UnregisterEventListeners()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (_solutionBuildManager != null && _updateSolutionEventsCookie != 0)
        _solutionBuildManager.UnadviseUpdateSolutionEvents(_updateSolutionEventsCookie);

      if (_solution != null && _solutionEventsCookie != 0)
        _solution.UnadviseSolutionEvents(_solutionEventsCookie);

      //_buildEvents.OnBuildBegin                              -= OnBuildBegin;
      //ProjectCollection.GlobalProjectCollection.ProjectAdded -= ProjectAdded;
    }

    private async Task<bool> IsSolutionLoadedAsync(CancellationToken cancellationToken)
    {
      await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

      var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

      Assumes.Present(solService);

      ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

      return value is bool isSolOpen && isSolOpen;
    }

    private void HandleOpenSolution(object sender = null, EventArgs e = null)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      this.WriteDebug("[SMA] HandleOpenSolution.");

      //SetPropertiesOnAllPlugins();

      //var projCol = ProjectCollection.GlobalProjectCollection;

      //projCol.SkipEvaluation = true;
      //projCol.SetGlobalProperty("TestPropertyName123", "TestPropertyValue123");
      //projCol.SkipEvaluation = false;

      //foreach (Project proj in _DTE2.Solution.Projects.Cast<Project>())
      //{
      //  proj.Globals["TestPropertyName123"] = "TestPropertyValue123";
      //}
    }

    #endregion
  }
}
