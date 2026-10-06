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
// Created On:   2020/04/03 17:37
// Modified On:  2020/04/03 17:38
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio
{
  using Extensions;
  using Microsoft.Build.Evaluation;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Shell.Interop;
  using Utils.VS;

  public sealed partial class SMASdkVSPackage
  {
    #region Methods Impl

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //System.Diagnostics.Debugger.Launch();

      this.WriteDebug("[SMA] UpdateSolution_StartUpdate.");

      //var projCol = ProjectCollection.GlobalProjectCollection;

      //projCol.SkipEvaluation = true;
      //projCol.SetGlobalProperty("TestPropertyName123", "TestPropertyValue123");
      //projCol.SkipEvaluation = false;

      //var solDirPath = Path.GetDirectoryName(Dte2.Solution.FullName);

      //foreach (var loadedProject in ProjectCollection.GlobalProjectCollection.LoadedProjects)
      //{
      //  this.WriteDebug($"Listing LoadedProjects: '{loadedProject.FullPath}'");

      //  loadedProject.SetGlobalProperty("TestPropertyName123", "TestPropertyValue123");
      //  loadedProject.SetProperty("TestPropertyName123", "TestPropertyValue123");
      //}

      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_Cancel()
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj,
                                                        IVsCfg       pCfgProj,
                                                        IVsCfg       pCfgSln,
                                                        uint         dwAction,
                                                        ref int      pfCancel)
    {
      this.WriteDebug($"[SMA] UpdateProjectCfg_Begin'.");
      var p = pHierProj.GetProject(Dte2);

      if (p != null)
        SetProjectProperty(p);

      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj,
                                                       IVsCfg       pCfgProj,
                                                       IVsCfg       pCfgSln,
                                                       uint         dwAction,
                                                       int          fSuccess,
                                                       int          fCancel)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate)
    {
      return VSConstants.S_OK;
    }

    #endregion
  }
}
