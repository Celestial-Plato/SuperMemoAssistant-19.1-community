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
// Created On:   2021/04/04 17:06
// Modified On:  2021/04/13 17:25
// Modified By:  Alexis

#endregion




namespace StubExecutable
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;

  internal class Program
  {
    #region Constants & Statics

    /// <summary>Note: if you change this regex also change the one below</summary>
    private static readonly Regex RE_AppDir = new Regex(@"^app-(?<Version>\d+(\.\d+){2,3})(?<Release>-[a-z][0-9a-z-\.]*)?$",
                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion




    #region Properties & Fields - Non-Public

    private readonly string _exeName;
    private readonly string _exeDirPath;
    private readonly string _cmdLine;

    #endregion




    #region Constructors

    public Program(string exeName, string exeDirPath, string cmdLine)
    {
      _exeName    = exeName;
      _exeDirPath = exeDirPath;
      _cmdLine    = cmdLine;
    }

    #endregion




    #region Methods

    private static void Main(string[] args)
    {
      string exeName = null;

      try
      {
        // Try to execute the latest installed version
        var exeFilePath = System.Reflection.Assembly.GetEntryAssembly().Location;
        var exeDirPath  = Path.GetDirectoryName(exeFilePath);
        exeName = Path.GetFileName(exeFilePath);

        var cmdLine = "\"" + string.Join("\" \"", args) + "\"";

        var program = new Program(exeName, exeDirPath, cmdLine);

        program.FindAndRunExe();
      }
      catch (Exception ex)
      {
        Log(ex.ToString(), exeName);

        throw;
      }
    }

    private void FindAndRunExe()
    {
      var exeFilePath = ScanReleaseFileForExe() ?? ScanDirectoriesForExe();

      var p = new Process
      {
        StartInfo =
        {
          FileName         = exeFilePath,
          Arguments        = _cmdLine,
          WorkingDirectory = _exeDirPath
        }
      };

      if (!p.Start())
        throw new InvalidOperationException($"Process {_exeName} failed to start. Command: '{exeFilePath} {_cmdLine}'");

      Natives.AllowSetForegroundWindow(p.Id);

      p.WaitForInputIdle(5000);
    }

    private string ScanReleaseFileForExe()
    {
      var releaseFilePath = Path.Combine(_exeDirPath, "packages", "RELEASES");
      var projectName     = Path.GetFileNameWithoutExtension(_exeName);

      if (File.Exists(releaseFilePath) == false)
        return null;

      var nuPkgRegex = new Regex("^" + projectName
                                 + @"-(?<Version>\d+(\.\d+){2,3})(?<Release>-[a-z][0-9a-z-\.]*)?\.nupkg$",
                                 RegexOptions.Compiled | RegexOptions.IgnoreCase);
      var versions = new List<NuGetVersion>();

      foreach (var line in File.ReadLines(releaseFilePath))
      {
        var splitLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (splitLine.Length != 3)
          continue;

        var match = nuPkgRegex.Match(splitLine[1]);

        if (match.Success == false)
          continue;

        var version = Version.Parse(match.Groups["Version"].Value);
        var release = match.Groups["Release"]?.Value;

        versions.Add(new NuGetVersion(version, release));
      }

      foreach (var v in versions.OrderByDescending(v => v, NuGetVersion.VersionReleaseComparer))
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        var exePath = Path.Combine(_exeDirPath, $"app-{v}", _exeName);

        if (File.Exists(exePath))
          return exePath;
      }

      return null;
    }

    private string ScanDirectoriesForExe()
    {
      var rootDir  = new DirectoryInfo(_exeDirPath);
      var versions = new Dictionary<NuGetVersion, string>();

      foreach (var subDir in rootDir.EnumerateDirectories())
      {
        var subDirName = subDir.Name;
        var match      = RE_AppDir.Match(subDir.Name);

        if (match.Success == false)
          continue;

        var exePath = Path.Combine(_exeDirPath, subDirName, _exeName);

        if (File.Exists(exePath) == false)
          continue;

        var version = Version.Parse(match.Groups["Version"].Value);
        var release = match.Groups["Release"]?.Value;

        var nuGetVersion = new NuGetVersion(version, release);

        versions[nuGetVersion] = exePath;
      }

      var maxVersion = versions.Keys
                               .OrderByDescending(v => v, NuGetVersion.VersionReleaseComparer)
                               .FirstOrDefault();

      return maxVersion != null
        ? versions[maxVersion]
        : null;
    }

    private static void Log(string msg, string logPrefix)
    {
      logPrefix ??= "Squirrel";

      var tmpFolder   = Path.GetTempPath();
      var logFilePath = Path.Combine(tmpFolder, logPrefix + "_StubExecutable.log");

      using (var fs = File.Open(logFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
      using (var sr = new StreamWriter(fs))
        sr.Write(msg);
    }

    #endregion




    private class NuGetVersion
    {
      #region Constants & Statics

      public static IComparer<NuGetVersion> VersionReleaseComparer { get; } = new VersionReleaseRelationalComparer();

      #endregion




      #region Constructors

      public NuGetVersion(Version version, string release)
      {
        Version = version;
        Release = release ?? string.Empty;
      }

      #endregion




      #region Properties & Fields - Public

      public Version Version { get; }
      public string  Release { get; }

      #endregion




      #region Methods Impl

      public override string ToString()
      {
        return Version + Release;
      }

      #endregion




      private sealed class VersionReleaseRelationalComparer : IComparer<NuGetVersion>
      {
        #region Methods Impl

        public int Compare(NuGetVersion x, NuGetVersion y)
        {
          if (ReferenceEquals(x, y))
            return 0;
          if (ReferenceEquals(null, y))
            return 1;
          if (ReferenceEquals(null, x))
            return -1;

          var versionComparison = Comparer<Version>.Default.Compare(x.Version, y.Version);

          return versionComparison != 0
            ? versionComparison
            : Natives.StrCmpLogicalW(x.Release, y.Release);
        }

        #endregion
      }
    }
  }
}
