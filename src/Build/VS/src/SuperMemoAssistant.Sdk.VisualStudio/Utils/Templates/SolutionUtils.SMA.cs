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
// Created On:   2020/03/31 19:33
// Modified On:  2020/04/02 00:23
// Modified By:  Alexis

#endregion






// ReSharper disable SuspiciousTypeConversion.Global

namespace SuperMemoAssistant.Sdk.VisualStudio.Utils.Templates
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Linq;
  using System.Xml.Linq;
  using EnvDTE;
  using EnvDTE100;
  using EnvDTE80;
  using Extensions;
  using Models;
  using Newtonsoft.Json;
  using Properties;

  [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
  public static partial class SolutionUtils
  {
    #region Methods

    public static void InstallProjects(
      this SMASolution                         solution,
      Dictionary<string, string>               replacementsDict,
      DirectoryInfo                            templateRoot,
      params (string path, string solFolder, string projSuffix)[] projectTemplates)
    {
      // Get all project template directories
      var projectTemplateDirs =
        projectTemplates
          .Select(
            t => (
              path: new FileInfo(Path.Combine(templateRoot.FullName, "..", t.path, $"{t.path}.vstemplate")),
              t.solFolder,
              t.projSuffix
            )
          )
          .ToList();

      // Make sure the folders exist before going further
      projectTemplateDirs.ForEach(t => t.path.ThrowIfMissing(string.Format(Resources.Error_TemplateMissing, t.path.FullName, t.projSuffix)));
      
      // Install the templates
      foreach (var (templatePath, solFolderName, projSuffix) in projectTemplateDirs)
      {
        var projectName = ProjectUtils.EnforcePluginName(replacementsDict.ProjectName()) + projSuffix;

        var solFolder =
          (SolutionFolder)solution.Projects.FirstOrDefault(p => p.Kind == ProjectKinds.vsProjectKindSolutionFolder
                                                             && p.Name == solFolderName);

        if (solFolder == null)
        {
          var project = solution.VSSolution.AddSolutionFolder(solFolderName);
          solFolder = (SolutionFolder)project.Object;

          solution.Projects.Add(project);
        }

        solFolder.AddFromTemplate(
          templatePath.FullName,
          Path.Combine(solution.SrcDirectory.FullName, projectName),
          projectName);
      }
    }

    public static void LoadProjects(this SMASolution solution)
    {
      solution.Projects = solution.VSSolution.GetProjects();
      solution.SMAProjects = solution.Projects.DetermineSMAProjects();
    }

    public static void UpdateOrWriteNetCoreGlobal(this FileInfo fileInfo)
    {
      NetCoreGlobal global = null;

      if (fileInfo.Exists)
        global = fileInfo.DeserializeFromFile<NetCoreGlobal>();

      global                                                      ??= new NetCoreGlobal();
      global.MSBuildSdks["SuperMemoAssistant.Sdk.WindowsDesktop"] =   global.MSBuildSdks["SuperMemoAssistant.Sdk"] = "0.1";

      global.SerializeToFile(fileInfo, Formatting.Indented);
    }

    public static FileInfo DetermineGlobalJsonFile(this DirectoryInfo srcDir)
    {
      var globalJsonFiles = srcDir.FindFile("global.json");
      var globalJsonFile  = globalJsonFiles?.FirstOrDefault();

      if (globalJsonFile == null)
        throw new InvalidOperationException(string.Format(Resources.Error_NoGlobalJson, srcDir.FullName));

      return globalJsonFile;
    }

    public static IList<Project> DetermineSMAProjects(this IList<Project> projects)
    {
      var smaProjects = new List<Project>();

      foreach (var project in projects)
      {
        var projectFilePath = project.FullName;

        if (projectFilePath.EndsWith(".csproj") == false)
          continue;

        var doc         = XDocument.Load(projectFilePath);
        var projectNode = doc.Root;

        if (projectNode.Name.LocalName != "Project")
          continue;

        var sdkAttr = projectNode.Attribute("Sdk");

        if (sdkAttr == null || sdkAttr.Value.StartsWith("SuperMemoAssistant.Sdk") == false)
          continue;

        smaProjects.Add(project);
      }

      return smaProjects;
    }

    public static DirectoryInfo DetermineSrcFolder(this Solution4 solution, IList<Project> smaProjects)
    {
      var solutionDir = new DirectoryInfo(solution.FullName);

      if (solutionDir.Exists == false)
        throw new InvalidOperationException(Resources.Error_InvalidSolution);

      var smaProjectFolders = smaProjects.Select(p => p.FullName);
      var srcFolders        = solutionDir.FindDir("src");

      return srcFolders.Select(di => (di, smaProjectFolders.Count(pn => pn.StartsWith(di.FullName))))
                       .OrderByDescending(t => t.Item2)
                       .FirstOrDefault().di;
    }

    #endregion
  }
}
