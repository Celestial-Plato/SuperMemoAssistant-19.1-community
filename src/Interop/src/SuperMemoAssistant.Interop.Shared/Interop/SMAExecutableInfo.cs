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

#endregion




namespace SuperMemoAssistant.Interop
{
  using System.Reflection;
  using System.Text.RegularExpressions;
  using global::Extensions.System.IO;

  /// <summary>Contains information about the current executable's assembly</summary>
  public class SMAExecutableInfo
  {
    #region Constants & Statics

    private const string EntryAssemblyRegexPattern = @"/app-([\d.]+(?:-[\w\-\.]+)?|dev)/(SuperMemoAssistant(?:\.PluginHost)?.exe)";

    /// <summary>The <see cref="SMAExecutableInfo" /> singleton</summary>
    public static SMAExecutableInfo Instance { get; } = new SMAExecutableInfo();

    #endregion




    #region Constructors

    private SMAExecutableInfo()
    {
      var entryAssemblyFilePath = new FilePath(Assembly.GetEntryAssembly().Location);
      var regexPattern          = SMAFileSystem.AppRootDir.FullPath + EntryAssemblyRegexPattern;
      var regex                 = new Regex(regexPattern);
      var match                 = regex.Match(entryAssemblyFilePath.FullPath);

      DirectoryPath = entryAssemblyFilePath.Directory;

      if (match.Success)
      {
        IsPathLocalAppData = true;

        if (match.Groups.Count == 3)
        {
          IsDev = match.Groups[1].Value == "dev";

          switch (match.Groups[2].Value)
          {
            case SMAConst.Assembly.SuperMemoAssistantExe:
              ExecutableType = SMAExecutableType.SuperMemoAssistant;
              break;

            case SMAConst.Assembly.PluginHostExe:
              ExecutableType = SMAExecutableType.PluginHost;
              break;
          }
        }
      }
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>The directory which contains the executing assembly's exe</summary>
    public DirectoryPath DirectoryPath { get; }

    /// <summary>Which SMA executable is currently hosting this dll -- internal use only (PluginHost, SMA)</summary>
    public SMAExecutableType ExecutableType { get; } = SMAExecutableType.Unknown;

    /// <summary>Whether the current SMA executable is a development version</summary>
    public bool IsDev { get; } = false;

    /// <summary>Whether SMA is installed in %LocalAppData%</summary>
    public bool IsPathLocalAppData { get; } = false;

    #endregion
  }

  /// <summary>Defines the different type of executables for SMA (plugin, SMA core, ..)</summary>
  public enum SMAExecutableType
  {
    /// <summary>Other executable</summary>
    Unknown,
    /// <summary>SuperMemoAssistant.exe</summary>
    SuperMemoAssistant,
    /// <summary>PluginHost.exe</summary>
    PluginHost,
  }
}
