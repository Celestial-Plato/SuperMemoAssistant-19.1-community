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
// Created On:   2020/04/04 16:47
// Modified On:  2020/04/04 16:51
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio
{
  using System;
  using System.Reflection;
  using EnvDTE;
  using Microsoft.Build.Execution;
  using Microsoft.Build.Framework;
  using Utils.VS;

  public sealed partial class SMASdkVSPackage
  {
    #region Methods

    private void _buildEvents_OnBuildProjConfigBegin(string Project, string ProjectConfig, string Platform, string SolutionConfig)
    {
      try
      {
        //if (!RegisterILogger())
        //  this.WriteDebug($"[SMA] OnBuildProjConfigBegin RegisterILogger failed :-(");
      }
      catch (Exception ex)
      {
        this.WriteDebug($"[SMA] OnBuildProjConfigBegin RegisterILogger exception {ex}");
      }
    }

    private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
    {
      try
      {
        //if (!RegisterILogger())
        //  this.WriteDebug($"[SMA] OnBuildBegin RegisterILogger failed :-(");

#if false
        var projCol = ProjectCollection.GlobalProjectCollection;

        projCol.SkipEvaluation = true;
        projCol.SetGlobalProperty("TestPropertyName123", "TestPropertyValue123");
        projCol.SkipEvaluation = false;

        this.WriteDebug($"[SMA] OnBuildBegin {action}.");
#endif
      }
      catch (Exception ex)
      {
        this.WriteDebug($"[SMA] OnBuildBegin RegisterILogger exception {ex}");
      }
    }

    private BuildParameters GetBuildParameters()
    {
      var buildMgr = BuildManager.DefaultBuildManager;

      return (BuildParameters)buildMgr
                              .GetType()
                              .GetField("_buildParameters", BindingFlags.NonPublic | BindingFlags.Instance)
                              .GetValue(buildMgr);
    }

    private void Build_TargetStarted(object sender, TargetStartedEventArgs e)
    {
      try
      {
        JoinableTaskFactory.Run(() => this.WriteDebugAsync($"[SMA] EventSource_TargetStarted '{e.ProjectFile}': {e.TargetName}"));
      }
      catch (Exception ex)
      {
        JoinableTaskFactory.Run(() => this.WriteDebugAsync($"[SMA] EventSource_TargetStarted exception {ex}"));
      }
    }

    private void Build_ProjectStarted(object sender, ProjectStartedEventArgs e)
    {
      try
      {
        JoinableTaskFactory.Run(() => this.WriteDebugAsync($"[SMA] ProjectStarted '{e.ProjectFile}': {e.TargetNames}"));
        e.GlobalProperties["TestPropertyName123"] = "TestPropertyValue123";
      }
      catch (Exception ex)
      {
        JoinableTaskFactory.Run(() => this.WriteDebugAsync($"[SMA] ProjectStarted exception {ex}"));
      }
    }

    private void Build_AnyEventRaised(object sender, BuildEventArgs e)
    {
      try
      {
        JoinableTaskFactory.Run(() => this.WriteDebugAsync($"[SMA] AnyEventRaised '{e.SenderName}': {e.Message}"));
      }
      catch (Exception ex)
      {
        JoinableTaskFactory.Run(() => this.WriteDebugAsync($"[SMA] AnyEventRaised exception {ex}"));
      }
    }

    #endregion
  }
}
