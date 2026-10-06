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
// Created On:   2020/04/04 02:03
// Modified On:  2020/04/04 02:06
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio.Utils.VS
{
  using Extensions;
  using Microsoft.Build.Framework;

  /// <summary>
  ///   The ReusableLogger wraps a logger and allows it to be used for both design-time and
  ///   build-time.  It internally swaps between the design-time and build-time event sources in
  ///   response to Initialize and Shutdown events.
  /// </summary>
  /// <remarks>
  ///   original
  ///   https://github.com/microsoft/msbuild/blob/master/src/Build/Definition/ProjectCollection.cs
  /// </remarks>
  internal class ReusableLogger : INodeLogger, IEventSource3
  {
    #region Properties & Fields - Non-Public

    /// <summary>The logger we are wrapping.</summary>
    private readonly ILogger _originalLogger;

    /// <summary>The Any event handler</summary>
    private AnyEventHandler _anyEventHandler;

    /// <summary>The Error event handler</summary>
    private BuildErrorEventHandler _buildErrorEventHandler;

    /// <summary>The BuildFinished event handler</summary>
    private BuildFinishedEventHandler _buildFinishedEventHandler;

    /// <summary>The Message event handler</summary>
    private BuildMessageEventHandler _buildMessageEventHandler;

    /// <summary>The BuildStarted event handler</summary>
    private BuildStartedEventHandler _buildStartedEventHandler;

    /// <summary>The Status event handler</summary>
    private BuildStatusEventHandler _buildStatusEventHandler;

    /// <summary>The build-time event source</summary>
    private IEventSource _buildTimeEventSource;

    /// <summary>The Warning event handler</summary>
    private BuildWarningEventHandler _buildWarningEventHandler;

    /// <summary>The Custom event handler</summary>
    private CustomBuildEventHandler _customBuildEventHandler;

    /// <summary>The design-time event source</summary>
    private IEventSource _designTimeEventSource;

    private bool _includeEvaluationMetaprojects;

    private bool _includeEvaluationProfiles;

    private bool _includeTaskInputs;

    /// <summary>The ProjectFinished event handler</summary>
    private ProjectFinishedEventHandler _projectFinishedEventHandler;

    /// <summary>The ProjectStarted event handler</summary>
    private ProjectStartedEventHandler _projectStartedEventHandler;

    /// <summary>The TargetFinished event handler</summary>
    private TargetFinishedEventHandler _targetFinishedEventHandler;

    /// <summary>The TargetStarted event handler</summary>
    private TargetStartedEventHandler _targetStartedEventHandler;

    /// <summary>The TaskFinished event handler</summary>
    private TaskFinishedEventHandler _taskFinishedEventHandler;

    /// <summary>The TaskStarted event handler</summary>
    private TaskStartedEventHandler _taskStartedEventHandler;

    /// <summary>The telemetry event handler.</summary>
    private TelemetryEventHandler _telemetryEventHandler;

    #endregion




    #region Constructors

    /// <summary>Constructor.</summary>
    public ReusableLogger(ILogger originalLogger)
    {
      originalLogger.ThrowIfArgumentNull(nameof(originalLogger));
      _originalLogger = originalLogger;
    }

    #endregion




    #region Methods

    /// <summary>Registers for all of the events on the specified event source.</summary>
    private void RegisterForEvents(IEventSource eventSource)
    {
      // Create the handlers.
      _anyEventHandler             = AnyEventRaisedHandler;
      _buildFinishedEventHandler   = BuildFinishedHandler;
      _buildStartedEventHandler    = BuildStartedHandler;
      _customBuildEventHandler     = CustomEventRaisedHandler;
      _buildErrorEventHandler      = ErrorRaisedHandler;
      _buildMessageEventHandler    = MessageRaisedHandler;
      _projectFinishedEventHandler = ProjectFinishedHandler;
      _projectStartedEventHandler  = ProjectStartedHandler;
      _buildStatusEventHandler     = StatusEventRaisedHandler;
      _targetFinishedEventHandler  = TargetFinishedHandler;
      _targetStartedEventHandler   = TargetStartedHandler;
      _taskFinishedEventHandler    = TaskFinishedHandler;
      _taskStartedEventHandler     = TaskStartedHandler;
      _buildWarningEventHandler    = WarningRaisedHandler;
      _telemetryEventHandler       = TelemetryLoggedHandler;

      // Register for the events.
      eventSource.AnyEventRaised    += _anyEventHandler;
      eventSource.BuildFinished     += _buildFinishedEventHandler;
      eventSource.BuildStarted      += _buildStartedEventHandler;
      eventSource.CustomEventRaised += _customBuildEventHandler;
      eventSource.ErrorRaised       += _buildErrorEventHandler;
      eventSource.MessageRaised     += _buildMessageEventHandler;
      eventSource.ProjectFinished   += _projectFinishedEventHandler;
      eventSource.ProjectStarted    += _projectStartedEventHandler;
      eventSource.StatusEventRaised += _buildStatusEventHandler;
      eventSource.TargetFinished    += _targetFinishedEventHandler;
      eventSource.TargetStarted     += _targetStartedEventHandler;
      eventSource.TaskFinished      += _taskFinishedEventHandler;
      eventSource.TaskStarted       += _taskStartedEventHandler;
      eventSource.WarningRaised     += _buildWarningEventHandler;

      if (eventSource is IEventSource2 eventSource2)
        eventSource2.TelemetryLogged += _telemetryEventHandler;

      if (eventSource is IEventSource3 eventSource3)
      {
        if (_includeEvaluationMetaprojects)
          eventSource3.IncludeEvaluationMetaprojects();
        if (_includeEvaluationProfiles)
          eventSource3.IncludeEvaluationProfiles();

        if (_includeTaskInputs)
          eventSource3.IncludeTaskInputs();
      }
    }

    /// <summary>Unregisters for all events on the specified event source.</summary>
    private void UnregisterForEvents(IEventSource eventSource)
    {
      // Unregister for the events.
      eventSource.AnyEventRaised    -= _anyEventHandler;
      eventSource.BuildFinished     -= _buildFinishedEventHandler;
      eventSource.BuildStarted      -= _buildStartedEventHandler;
      eventSource.CustomEventRaised -= _customBuildEventHandler;
      eventSource.ErrorRaised       -= _buildErrorEventHandler;
      eventSource.MessageRaised     -= _buildMessageEventHandler;
      eventSource.ProjectFinished   -= _projectFinishedEventHandler;
      eventSource.ProjectStarted    -= _projectStartedEventHandler;
      eventSource.StatusEventRaised -= _buildStatusEventHandler;
      eventSource.TargetFinished    -= _targetFinishedEventHandler;
      eventSource.TargetStarted     -= _targetStartedEventHandler;
      eventSource.TaskFinished      -= _taskFinishedEventHandler;
      eventSource.TaskStarted       -= _taskStartedEventHandler;
      eventSource.WarningRaised     -= _buildWarningEventHandler;

      if (eventSource is IEventSource2 eventSource2)
        eventSource2.TelemetryLogged -= _telemetryEventHandler;

      // Null out the handlers.
      _anyEventHandler             = null;
      _buildFinishedEventHandler   = null;
      _buildStartedEventHandler    = null;
      _customBuildEventHandler     = null;
      _buildErrorEventHandler      = null;
      _buildMessageEventHandler    = null;
      _projectFinishedEventHandler = null;
      _projectStartedEventHandler  = null;
      _buildStatusEventHandler     = null;
      _targetFinishedEventHandler  = null;
      _targetStartedEventHandler   = null;
      _taskFinishedEventHandler    = null;
      _taskStartedEventHandler     = null;
      _buildWarningEventHandler    = null;
      _telemetryEventHandler       = null;
    }

    /// <summary>Handler for Warning events.</summary>
    private void WarningRaisedHandler(object sender, BuildWarningEventArgs e)
    {
      WarningRaised?.Invoke(sender, e);
    }

    /// <summary>Handler for TaskStarted events.</summary>
    private void TaskStartedHandler(object sender, TaskStartedEventArgs e)
    {
      TaskStarted?.Invoke(sender, e);
    }

    /// <summary>Handler for TaskFinished events.</summary>
    private void TaskFinishedHandler(object sender, TaskFinishedEventArgs e)
    {
      TaskFinished?.Invoke(sender, e);
    }

    /// <summary>Handler for TargetStarted events.</summary>
    private void TargetStartedHandler(object sender, TargetStartedEventArgs e)
    {
      TargetStarted?.Invoke(sender, e);
    }

    /// <summary>Handler for TargetFinished events.</summary>
    private void TargetFinishedHandler(object sender, TargetFinishedEventArgs e)
    {
      TargetFinished?.Invoke(sender, e);
    }

    /// <summary>Handler for Status events.</summary>
    private void StatusEventRaisedHandler(object sender, BuildStatusEventArgs e)
    {
      StatusEventRaised?.Invoke(sender, e);
    }

    /// <summary>Handler for ProjectStarted events.</summary>
    private void ProjectStartedHandler(object sender, ProjectStartedEventArgs e)
    {
      ProjectStarted?.Invoke(sender, e);
    }

    /// <summary>Handler for ProjectFinished events.</summary>
    private void ProjectFinishedHandler(object sender, ProjectFinishedEventArgs e)
    {
      ProjectFinished?.Invoke(sender, e);
    }

    /// <summary>Handler for Message events.</summary>
    private void MessageRaisedHandler(object sender, BuildMessageEventArgs e)
    {
      MessageRaised?.Invoke(sender, e);
    }

    /// <summary>Handler for Error events.</summary>
    private void ErrorRaisedHandler(object sender, BuildErrorEventArgs e)
    {
      ErrorRaised?.Invoke(sender, e);
    }

    /// <summary>Handler for Custom events.</summary>
    private void CustomEventRaisedHandler(object sender, CustomBuildEventArgs e)
    {
      CustomEventRaised?.Invoke(sender, e);
    }

    /// <summary>Handler for BuildStarted events.</summary>
    private void BuildStartedHandler(object sender, BuildStartedEventArgs e)
    {
      BuildStarted?.Invoke(sender, e);
    }

    /// <summary>Handler for BuildFinished events.</summary>
    private void BuildFinishedHandler(object sender, BuildFinishedEventArgs e)
    {
      BuildFinished?.Invoke(sender, e);
    }

    /// <summary>Handler for Any events.</summary>
    private void AnyEventRaisedHandler(object sender, BuildEventArgs e)
    {
      AnyEventRaised?.Invoke(sender, e);
    }

    /// <summary>Handler for telemetry events.</summary>
    private void TelemetryLoggedHandler(object sender, TelemetryEventArgs e)
    {
      TelemetryLogged?.Invoke(sender, e);
    }

    #endregion




    #region IEventSource Members

    /// <summary>The Message logging event</summary>
    public event BuildMessageEventHandler MessageRaised;

    /// <summary>The Error logging event</summary>
    public event BuildErrorEventHandler ErrorRaised;

    /// <summary>The Warning logging event</summary>
    public event BuildWarningEventHandler WarningRaised;

    /// <summary>The BuildStarted logging event</summary>
    public event BuildStartedEventHandler BuildStarted;

    /// <summary>The BuildFinished logging event</summary>
    public event BuildFinishedEventHandler BuildFinished;

    /// <summary>The ProjectStarted logging event</summary>
    public event ProjectStartedEventHandler ProjectStarted;

    /// <summary>The ProjectFinished logging event</summary>
    public event ProjectFinishedEventHandler ProjectFinished;

    /// <summary>The TargetStarted logging event</summary>
    public event TargetStartedEventHandler TargetStarted;

    /// <summary>The TargetFinished logging event</summary>
    public event TargetFinishedEventHandler TargetFinished;

    /// <summary>The TashStarted logging event</summary>
    public event TaskStartedEventHandler TaskStarted;

    /// <summary>The TaskFinished logging event</summary>
    public event TaskFinishedEventHandler TaskFinished;

    /// <summary>The Custom logging event</summary>
    public event CustomBuildEventHandler CustomEventRaised;

    /// <summary>The Status logging event</summary>
    public event BuildStatusEventHandler StatusEventRaised;

    /// <summary>The Any logging event</summary>
    public event AnyEventHandler AnyEventRaised;

    /// <summary>The telemetry sent event.</summary>
    public event TelemetryEventHandler TelemetryLogged;

    /// <summary>Should evaluation events include generated metaprojects?</summary>
    public void IncludeEvaluationMetaprojects()
    {
      if (_buildTimeEventSource is IEventSource3 buildEventSource3)
        buildEventSource3.IncludeEvaluationMetaprojects();

      if (_designTimeEventSource is IEventSource3 designTimeEventSource3)
        designTimeEventSource3.IncludeEvaluationMetaprojects();

      _includeEvaluationMetaprojects = true;
    }

    /// <summary>Should evaluation events include profiling information?</summary>
    public void IncludeEvaluationProfiles()
    {
      if (_buildTimeEventSource is IEventSource3 buildEventSource3)
        buildEventSource3.IncludeEvaluationProfiles();

      if (_designTimeEventSource is IEventSource3 designTimeEventSource3)
        designTimeEventSource3.IncludeEvaluationProfiles();

      _includeEvaluationProfiles = true;
    }

    /// <summary>Should task events include task inputs?</summary>
    public void IncludeTaskInputs()
    {
      if (_buildTimeEventSource is IEventSource3 buildEventSource3)
        buildEventSource3.IncludeTaskInputs();

      if (_designTimeEventSource is IEventSource3 designTimeEventSource3)
        designTimeEventSource3.IncludeTaskInputs();

      _includeTaskInputs = true;
    }

    #endregion




    #region ILogger Members

    /// <summary>The logger verbosity</summary>
    public LoggerVerbosity Verbosity
    {
      get => _originalLogger.Verbosity;
      set => _originalLogger.Verbosity = value;
    }

    /// <summary>The logger parameters</summary>
    public string Parameters
    {
      get => _originalLogger.Parameters;

      set => _originalLogger.Parameters = value;
    }

    /// <summary>
    ///   If we haven't yet been initialized, we register for design time events and initialize
    ///   the logger we are holding. If we are in design-time mode
    /// </summary>
    public void Initialize(IEventSource eventSource, int nodeCount)
    {
      if (_designTimeEventSource == null)
      {
        _designTimeEventSource = eventSource;
        RegisterForEvents(_designTimeEventSource);

        if (_originalLogger is INodeLogger logger)
          logger.Initialize(this, nodeCount);
        else
          _originalLogger.Initialize(this);
      }
      else
      {
        _buildTimeEventSource.ThrowIfNotNull("Already registered for build-time.");
        _buildTimeEventSource = eventSource;
        UnregisterForEvents(_designTimeEventSource);
        RegisterForEvents(_buildTimeEventSource);
      }
    }

    /// <summary>
    ///   If we haven't yet been initialized, we register for design time events and initialize
    ///   the logger we are holding. If we are in design-time mode
    /// </summary>
    public void Initialize(IEventSource eventSource)
    {
      Initialize(eventSource, 1);
    }

    /// <summary>
    ///   If we are in build-time mode, we unregister for build-time events and re-register for
    ///   design-time events. If we are in design-time mode, we unregister for design-time events and
    ///   shut down the logger we are holding.
    /// </summary>
    public void Shutdown()
    {
      if (_buildTimeEventSource != null)
      {
        UnregisterForEvents(_buildTimeEventSource);
        RegisterForEvents(_designTimeEventSource);
        _buildTimeEventSource = null;
      }
      else
      {
        _designTimeEventSource.ThrowIfNull("Already unregistered for design-time.");
        UnregisterForEvents(_designTimeEventSource);
        _originalLogger.Shutdown();
      }
    }

    #endregion
  }
}
