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
// Created On:   2020/03/31 18:33
// Modified On:  2020/03/31 18:45
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;

namespace SuperMemoAssistant.Sdk.VisualStudio.Extensions
{
  public static class DirectoryInfoEx
  {
    #region Methods

    public static void EnsureExists(this DirectoryInfo dir)
    {
      if (dir.Exists == false)
        dir.Create();
    }

    public static void CopyFolderRecursive(
      this DirectoryInfo srcDir,
      DirectoryInfo destDir,
      Func<FileInfo, string> processFile = null,
      Func<FileInfo, string> renameFile = null)
    {
      destDir.EnsureExists();

      foreach (var srcFile in srcDir.GetFiles())
      {
        string destFileName = srcFile.Name;

        if (renameFile != null)
          destFileName = renameFile.Invoke(srcFile);

        var destFile = Path.Combine(destDir.FullName, destFileName);

        if (processFile == null)
          srcFile.CopyTo(destFile, true);

        else
        {
          var content = processFile(srcFile);

          File.WriteAllText(destFile, content);
        }
      }

      foreach (var subSrcDir in srcDir.GetDirectories())
      {
        var subDestDir = new DirectoryInfo(Path.Combine(destDir.FullName, subSrcDir.Name));

        subSrcDir.CopyFolderRecursive(subDestDir, processFile);
      }
    }

    public static IEnumerable<FileInfo> FindFile(this DirectoryInfo rootDir, string fileName)
    {
      void FileFoundEvent(FileInfo fileInfo, List<FileInfo> localFiles)
      {
        if (fileName.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase))
          localFiles.Add(fileInfo);
      }

      var files = new List<FileInfo>();

      rootDir.RecursiveVisitor(fi => FileFoundEvent(fi, files), null);

      return files;
    }

    public static IEnumerable<DirectoryInfo> FindDir(this DirectoryInfo rootDir, string dirName)
    {
      bool DirectoryFoundEvent(DirectoryInfo di, List<DirectoryInfo> localDirs)
      {
        if (dirName.Equals(di.Name, StringComparison.OrdinalIgnoreCase))
          localDirs.Add(di);

        return true;
      }

      var dirs = new List<DirectoryInfo>();

      rootDir.RecursiveVisitor(null, di => DirectoryFoundEvent(di, dirs));

      return dirs;
    }

    private static void RecursiveVisitor(
      this DirectoryInfo        currentDir,
      Action<FileInfo>          fileFoundEvent,
      Func<DirectoryInfo, bool> directoryFoundEvent)
    {
      if (fileFoundEvent != null)
        foreach (var itFile in currentDir.EnumerateFiles())
          fileFoundEvent(itFile);

      foreach (var itDir in currentDir.EnumerateDirectories())
        if (directoryFoundEvent == null || directoryFoundEvent(itDir))
          itDir.RecursiveVisitor(fileFoundEvent, directoryFoundEvent);
    }

    #endregion
  }
}
