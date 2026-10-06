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
// Created On:   2020/04/02 21:47
// Modified On:  2020/04/03 17:23
// Modified By:  Alexis

#endregion




// ReSharper disable SuspiciousTypeConversion.Global
namespace SuperMemoAssistant.Sdk.VisualStudio.Utils.VS
{
  using System;
  using System.Threading.Tasks;
  using EnvDTE;
  using EnvDTE80;
  using Microsoft.Build.Framework;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Shell.Interop;

  public static class VSOutputWindow
  {
    #region Constants & Statics

    // {F84CD1E7-0B9E-4928-8B87-D473B960F25B}
    private static readonly Guid SMASdkPaneGuid = new Guid("92E55B59-ABF7-44DC-A3D7-5BF6AD6AAA3C");

    #endregion




    #region Methods

    /// <summary>
    ///   Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An array of objects to write using format. </param>
    public static bool WriteDebug(this IVSOutputWindowWriter writer,
                                  string                     format,
                                  params object[]            args)
    {
#if DEBUG
      return writer.WriteLine(LoggerVerbosity.Minimal, format, args);
#else
      return true;
#endif
    }

    /// <summary>
    ///   Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An array of objects to write using format. </param>
    public static bool WriteDetailed(this IVSOutputWindowWriter writer,
                                     string                     format,
                                     params object[]            args)
    {
      return writer.WriteLine(LoggerVerbosity.Detailed, format, args);
    }

    /// <summary>
    ///   Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="verbosity">The verbosity level.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An array of objects to write using format. </param>
    public static bool WriteLine(this IVSOutputWindowWriter writer,
                                 LoggerVerbosity            verbosity,
                                 string                     format,
                                 params object[]            args)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (writer.EnsureOutputWindow() == false)
        return false;

      if ((int)writer.CurrentBuildVerbosity < (int)verbosity)
        return false;

      writer.OutputWindowPane2.OutputString(string.Format(format + Environment.NewLine, args));

      return writer.OutputWindowPane.OutputString(string.Format(format + Environment.NewLine, args)) == VSConstants.S_OK;
    }

    /// <summary>
    ///   Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An array of objects to write using format. </param>
    public static Task<bool> WriteDebugAsync(this IVSOutputWindowWriter writer,
                                             string                     format,
                                             params object[]            args)
    {
#if DEBUG
      return writer.WriteLineAsync(LoggerVerbosity.Minimal, format, args);
#else
      return true;
#endif
    }

    /// <summary>
    ///   Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An array of objects to write using format. </param>
    public static Task<bool> WriteDetailedAsync(this IVSOutputWindowWriter writer,
                                                string                     format,
                                                params object[]            args)
    {
      return writer.WriteLineAsync(LoggerVerbosity.Detailed, format, args);
    }

    /// <summary>
    ///   Outputs a message to the debug output pane, if the VS MSBuildOutputVerbosity setting
    ///   value is greater than or equal to the given verbosity. So if verbosity is 0, it means the
    ///   message is always written to the output pane.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="verbosity">The verbosity level.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An array of objects to write using format. </param>
    public static async Task<bool> WriteLineAsync(this IVSOutputWindowWriter writer,
                                                  LoggerVerbosity            verbosity,
                                                  string                     format,
                                                  params object[]            args)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      if (await writer.EnsureOutputWindowAsync() == false)
        return false;

      if ((int)writer.CurrentBuildVerbosity < (int)verbosity)
        return false;

      writer.OutputWindowPane2.OutputString(string.Format(format + Environment.NewLine, args));

      return writer.OutputWindowPane.OutputString(string.Format(format + Environment.NewLine, args)) == VSConstants.S_OK;
    }

    /// <summary>Refreshes the value of the VisualStudio MSBuildOutputVerbosity setting.</summary>
    /// <remarks>0 is Quiet, while 4 is diagnostic.</remarks>
    private static void RefreshMSBuildOutputVerbositySetting(this IVSOutputWindowWriter writer)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      Properties properties = writer.Dte2.Properties["Environment", "ProjectsAndSolution"];

      writer.CurrentBuildVerbosity = (LoggerVerbosity)properties.Item("MSBuildOutputVerbosity").Value;
    }

    private static async Task<bool> EnsureOutputWindowAsync(this IVSOutputWindowWriter writer)
    {
      if (writer.OutputWindowPane != null)
        return true;

      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var outputWindow = writer.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;

      return writer.EnsureOutputWindow(outputWindow);
    }

    private static bool EnsureOutputWindow(this IVSOutputWindowWriter writer, IVsOutputWindow outputWindow = null)
    {
      if (writer.OutputWindowPane != null)
        return true;

      ThreadHelper.ThrowIfNotOnUIThread();

      var outputWindowPanes = writer.Dte2.ToolWindows.OutputWindow.OutputWindowPanes;
      outputWindow ??= writer.GetServiceSync(typeof(SVsOutputWindow)) as IVsOutputWindow;

      if (outputWindow == null)
        return false;

      Guid buildPaneGuid = VSConstants.GUID_BuildOutputWindowPane;
      int  hResult       = outputWindow.GetPane(ref buildPaneGuid, out var buildOutputWindowPane);

      writer.OutputWindowPane = buildOutputWindowPane;

      try
      {
        writer.OutputWindowPane2 = outputWindowPanes.Item("SuperMemoAssistant SDK");
      }
      catch (Exception)
      {
        writer.OutputWindowPane2 = outputWindowPanes.Add("SuperMemoAssistant SDK");
      }

      writer.RefreshMSBuildOutputVerbositySetting();

      return hResult == VSConstants.S_OK && buildOutputWindowPane != null;
    }

    #endregion
  }

  public interface IVSOutputWindowWriter
  {
    DTE2                Dte2                  { get; }
    IVsOutputWindowPane OutputWindowPane      { get; set; }
    OutputWindowPane    OutputWindowPane2     { get; set; }
    LoggerVerbosity     CurrentBuildVerbosity { get; set; }

    Task<object> GetServiceAsync(Type serviceType);
    object       GetServiceSync(Type      serviceType);
  }
}
