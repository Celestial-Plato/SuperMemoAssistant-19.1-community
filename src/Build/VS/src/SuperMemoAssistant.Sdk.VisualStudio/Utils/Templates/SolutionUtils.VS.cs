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
// Created On:   2020/03/31 17:19
// Modified On:  2020/03/31 20:51
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio.Utils.Templates
{
  using System.Collections.Generic;
  using EnvDTE;
  using EnvDTE100;
  using EnvDTE80;

  public static partial class SolutionUtils
  {
    #region Methods

    /// <summary>Gets the projects in a solution recursively.</summary>
    /// <returns></returns>
    public static IList<Project> GetProjects(this Solution solution)
    {
      // ReSharper disable once SuspiciousTypeConversion.Global
      return ((Solution4)solution).GetProjects();
    }

    /// <summary>Gets the projects in a solution recursively.</summary>
    /// <returns></returns>
    public static IList<Project> GetProjects(this Solution4 solution)
    {
      var projects = solution.Projects;
      var list     = new List<Project>();
      var item     = projects.GetEnumerator();

      while (item.MoveNext())
      {
        var project = item.Current as Project;

        if (project == null)
          continue;

        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
          list.AddRange(GetSolutionFolderProjects(project));
        else
          list.Add(project);
      }

      return list;
    }

    /// <summary>Gets the solution folder projects.</summary>
    /// <param name="solutionFolder">The solution folder.</param>
    /// <returns></returns>
    public static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
    {
      var list = new List<Project>();

      for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
      {
        var subProject = solutionFolder.ProjectItems.Item(i).SubProject;

        if (subProject == null)
          continue;

        // If this is another solution folder, do a recursive call, otherwise add
        if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
          list.AddRange(GetSolutionFolderProjects(subProject));
        else
          list.Add(subProject);
      }

      return list;
    }

    #endregion
  }
}
