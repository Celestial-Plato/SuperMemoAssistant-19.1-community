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
// Created On:   2020/04/04 16:51
// Modified On:  2020/04/04 16:51
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio
{
  using System.Linq;
  using EnvDTE80;
  using Microsoft.Build.Evaluation;
  using Utils.Templates;
  using Utils.VS;

  public sealed partial class SMASdkVSPackage
  {
    #region Methods

    private void SetPropertiesOnAllPlugins()
    {
      var loadedProjects     = ProjectCollection.GlobalProjectCollection.LoadedProjects;
      var loadedProjectPaths = loadedProjects.Select(p => p.FullPath).ToHashSet();

      var unloadedProjectsPaths = Dte2.Solution
                                      .GetProjects()
                                      .Where(p => p.Kind != ProjectKinds.vsProjectKindSolutionFolder)
                                      .Select(p => p.FullName);

      foreach (var loadedProj in loadedProjects)
        loadedProj.SetGlobalProperty("TestPropertyName123", "TestPropertyValue123");

      foreach (var unloadedProjPath in unloadedProjectsPaths.Where(p => loadedProjectPaths.Contains(p) == false))
      {
        // this.WriteDebug($"[SMA] Setting prop on '{projPath}'.");
        var proj = ProjectCollection.GlobalProjectCollection.LoadProject(unloadedProjPath);

        SetProjectProperty(proj);

        ProjectCollection.GlobalProjectCollection.UnloadProject(proj);
      }
    }

    private void SetProjectProperty(Project project)
    {
      project.SetGlobalProperty("TestPropertyName123", "TestPropertyValue123");
    }

    private void SetProjectProperty(EnvDTE.Project project)
    {
      //ThreadHelper.ThrowIfNotOnUIThread();

      //var props = project?.Properties;
      //var prop = props?.Item("TestPropertyName123");

      //if (prop != null)
      //  try
      //  {
      //    try
      //    {
      //      prop.Value = (object)"TestPropertyValue123";
      //    }
      //    catch (Exception ex)
      //    {
      //      this.WriteDebug($"[SMA] SetProjectProperty '{project?.Name}' exception 1:\n{ex}");
      //    }

      //    prop.let_Value("TestPropertyValue123");
      //  }
      //  catch (Exception ex)
      //  {
      //    this.WriteDebug($"[SMA] SetProjectProperty '{project?.Name}' exception 2:\n{ex}");
      //  }

      //project?.Save();

      this.WriteDebug($"[SMA] SetProjectProperty '{project?.Name}'.");
    }

    #endregion
  }
}
