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
// Created On:   2020/04/04 02:08
// Modified On:  2020/04/04 15:18
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio
{
  using System;
  using System.Reflection;
  using Microsoft.Build.Execution;
  using Microsoft.Build.Framework;
  using Utils.VS;

  public sealed partial class SMASdkVSPackage : ILogger
  {
    #region Properties Impl - Public

    /// <inheritdoc />
    LoggerVerbosity ILogger.Verbosity { get; set; }

    /// <inheritdoc />
    string ILogger.Parameters { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    void ILogger.Initialize(IEventSource eventSource)
    {
      _eventSrc = (IEventSource3)eventSource;

      //_eventSrc.TargetStarted += Build_TargetStarted;
      //_eventSrc.AnyEventRaised += Build_AnyEventRaised;
      _eventSrc.ProjectStarted += Build_ProjectStarted;
    }

    /// <inheritdoc />
    void ILogger.Shutdown()
    {
      //_eventSrc.TargetStarted -= EventSource_TargetStarted;

      _eventSrc = null;
    }

    #endregion




    #region Methods

    private bool RegisterILogger()
    {
      //var globalProjCol = ProjectCollection.GlobalProjectCollection;
      //globalProjCol.RegisterLogger(this);

      // Microsoft.Build.BackEnd.Logging.ILoggingService
      //var beLoggingSvc = globalProjCol.GetType().GetProperty("LoggingService", BindingFlags.NonPublic | BindingFlags.Instance)
      //                                .GetValue(globalProjCol);

      var buildMgr = BuildManager.DefaultBuildManager;

      var loggingSvc = buildMgr
                       .GetType()
                       .GetProperty("Microsoft.Build.BackEnd.IBuildComponentHost.LoggingService",
                                    BindingFlags.NonPublic | BindingFlags.Instance)
                       .GetValue(buildMgr);

      return (bool)loggingSvc
                   .GetType()
                   .GetMethod("RegisterLogger", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(ILogger) }, null)
                   .Invoke(loggingSvc, new object[] { new ReusableLogger(this) });
    }

    #endregion
  }
}
