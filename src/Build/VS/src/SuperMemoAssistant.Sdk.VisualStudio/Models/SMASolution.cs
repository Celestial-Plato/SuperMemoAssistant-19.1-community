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
// Created On:   2020/03/31 17:56
// Modified On:  2020/04/01 19:00
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE100;
using SuperMemoAssistant.Sdk.VisualStudio.Extensions;
using SuperMemoAssistant.Sdk.VisualStudio.Properties;

namespace SuperMemoAssistant.Sdk.VisualStudio.Models
{
  using Utils.Templates;

  /// <summary>Defines the components and files of a SMA solution</summary>
  public class SMASolution
  {
    #region Constructors

    /// <summary>Initializes an existing SMA collection</summary>
    /// <param name="solution"></param>
    public SMASolution(Solution4 solution)
    {
      if (solution.IsOpen == false)
        throw new InvalidOperationException(Resources.Error_InvalidSolution);

      Projects = solution.GetProjects();

      if (Projects == null || Projects.Count == 0)
        throw new InvalidOperationException(Resources.Error_InvalidSolution);

      SMAProjects  = Projects.DetermineSMAProjects();
      SrcDirectory = solution.DetermineSrcFolder(SMAProjects);

      if (SrcDirectory == null || SrcDirectory.Exists == false)
        throw new InvalidOperationException(Resources.Error_NoSrcFolder);

      GlobalJsonFilePath = SrcDirectory.DetermineGlobalJsonFile();
      GlobalJsonFilePath.UpdateOrWriteNetCoreGlobal();

      VSSolution = solution;
    }

    /// <summary>Initializes a new SMA solution</summary>
    /// <param name="solution"></param>
    /// <param name="templateRoot">The SuperMemoAssistant.Sdk.Templates.Plugin template's root dir</param>
    /// <param name="replacementDict"></param>
    public SMASolution(Solution4 solution, DirectoryInfo templateRoot, Dictionary<string, string> replacementDict)
    {
      var templateSolutionDir = new DirectoryInfo(Path.Combine(templateRoot.FullName, "..", "Solution", "Content"));

      var solutionRootPath = new DirectoryInfo(replacementDict.SolutionDirectory());
      SrcDirectory = new DirectoryInfo(Path.Combine(replacementDict.SolutionDirectory(), "src"));

      solutionRootPath.EnsureExists();

      templateSolutionDir.CopyFolderRecursive(
        solutionRootPath,
        replacementDict.ReplaceVariables,
        fi => fi.Name.EndsWith("Plugin.sln.DotSettings")
          ? replacementDict.SafeProjectName() + ".sln.DotSettings"
          : fi.Name
      );

      SrcDirectory.EnsureExists();
      VSSolution = solution;
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>
    /// The VS automation object
    /// </summary>
    public Solution4 VSSolution { get; }

    /// <summary>The "src" directory</summary>
    public DirectoryInfo SrcDirectory { get; }

    /// <summary>The "global.json" file. See <see cref="NetCoreGlobal" />.</summary>
    public FileInfo GlobalJsonFilePath { get; }

    /// <summary>All existing projects in the solution</summary>
    public IList<Project> Projects { get; set; }

    /// <summary>All projects that are using the SuperMemoAssistant.Sdk(.*) SDK</summary>
    public IList<Project> SMAProjects { get; set; }

    #endregion
  }
}
