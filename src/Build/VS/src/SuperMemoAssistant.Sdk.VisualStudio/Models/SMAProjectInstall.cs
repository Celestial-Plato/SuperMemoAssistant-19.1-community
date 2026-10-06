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
// Created On:   2020/04/01 17:02
// Modified On:  2020/04/01 21:22
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.IO;
using SuperMemoAssistant.Sdk.VisualStudio.Extensions;

namespace SuperMemoAssistant.Sdk.VisualStudio.Models
{
  using Utils.Templates;

  public class SMAProjectInstall
  {
    #region Constructors

    public SMAProjectInstall(SMASolution smaSolution, DirectoryInfo templateRoot, Dictionary<string, string> replacementsDict)
    {
      ProjectName = ProjectUtils.EnforcePluginName(replacementsDict.ProjectName());
      PluginName  = ProjectUtils.GetPluginName(replacementsDict.ProjectName());

      TemplateRoot           = templateRoot;
      OriginalDestinationDir = new DirectoryInfo(replacementsDict.DestinationDir());

      FinalDestinationDir = new DirectoryInfo(Path.Combine(smaSolution.SrcDirectory.FullName, ProjectName));
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>The template dir from which to copy the project files</summary>
    public DirectoryInfo TemplateRoot { get; }

    /// <summary>The project name (e.g. SuperMemoAssistant.Plugins.MyPlugin)</summary>
    public string ProjectName { get; }

    /// <summary>The plugin name (e.g. MyPlugin)</summary>
    public string PluginName { get; }

    /// <summary>The real destination dir (under "src")</summary>
    public DirectoryInfo FinalDestinationDir { get; }

    /// <summary>The destination dir as decided by Visual Studio...</summary>
    public DirectoryInfo OriginalDestinationDir { get; }

    #endregion
  }
}
