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
// Created On:   2020/04/03 17:11
// Modified On:  2020/04/03 17:12
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio.Extensions
{
  using System;
  using System.Linq;
  using EnvDTE;
  using EnvDTE80;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Shell.Interop;
  
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread

  public static class IVsHierarchyEx
  {
    #region Methods

    /// <summary>
    /// https://github.com/microsoft/PTVS/blob/1d04f01b7b902a9e1051b4080770b4a27e6e97e7/Common/Product/SharedProject/SolutionListenerForProjectOpen.cs
    /// </summary>
    /// <param name="hierarchy"></param>
    /// <returns></returns>
    internal static EnvDTE.Project GetProject(this IVsHierarchy hierarchy, DTE2 dte2)
    {
      if (hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, __VSHPROPID.VSHPROPID_ExtObject, out var project))
        return project;
      
      if (hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, __VSHPROPID.VSHPROPID_ProjectIDGuid, out var projectId) == false)
        return null;

      var solution = (_Solution)dte2.Solution;
      var projects = solution.Projects.Cast<Project>();

      return projects.FirstOrDefault(p => p.GetProjectId(out var id) && id == projectId);
    }

    private static bool GetProperty(this IVsHierarchy hierarchy, uint constant, __VSHPROPID propId, out Project project)
    {
      project = null;

      if (hierarchy == null || ErrorHandler.Failed(hierarchy.GetProperty(constant, (int)propId, out var projectObj)))
        return false;

      project = projectObj as Project;

      return project != null;
    }

    private static bool GetGuidProperty(this IVsHierarchy hierarchy, uint constant, __VSHPROPID propId, out Guid guid)
    {
      guid = default;

      if (hierarchy == null || ErrorHandler.Failed(hierarchy.GetGuidProperty(constant, (int)propId, out guid)))
        return false;

      return true;
    }

    public static bool GetProjectId(this Project project, out Guid guid)
    {
      return project.GetHierarchy().GetGuidProperty(
        VSConstants.VSITEMID_ROOT,
        __VSHPROPID.VSHPROPID_TypeGuid,
        out guid);
    }

    public static IVsHierarchy GetHierarchy(this Project project)
    {
      ErrorHandler.ThrowOnFailure((Package.GetGlobalService(typeof (SVsSolution)) as IVsSolution2).GetProjectOfUniqueName(project.UniqueName, out var vsHierarchy));

      return vsHierarchy;
    }

    #endregion
  }
}
