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
// Created On:   2020/03/31 02:23
// Modified On:  2020/04/02 01:23
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using SuperMemoAssistant.Sdk.VisualStudio.Extensions;
using SuperMemoAssistant.Sdk.VisualStudio.Models;
using SuperMemoAssistant.Sdk.VisualStudio.Properties;

namespace SuperMemoAssistant.Sdk.VisualStudio.Wizards
{
  using Utils.Templates;

  public class PluginTemplateWizard : IWizard
  {
    #region Constants & Statics

    private static SMASolution _smaSolution = null;

    #endregion




    #region Properties & Fields - Non-Public

    private SMAProjectInstall          _currentProject;
    private DTE2                       _dte;
    private Dictionary<string, string> _replacementsDict;
    private WizardRunKind              _runKind;
    private Solution4                  _solution = null;
    private DirectoryInfo              _templateRoot;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public void RunStarted(object                     automationObject,
                           Dictionary<string, string> replacementsDictionary,
                           WizardRunKind              runKind,
                           object[]                   customParams)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      customParams.ThrowIfArgumentNull(nameof(customParams));

      // Get current template
      var templateRootPath = (string)customParams[0];
      templateRootPath.ThrowIfNull(string.Format(Resources.Error_TemplateParamMissing, customParams[0]));

      var templateRoot = new DirectoryInfo(Path.GetDirectoryName(templateRootPath));
      templateRoot.ThrowIfMissing(string.Format(Resources.Error_TemplateMissing, templateRoot.FullName));

      // Get automation objects
      _dte = automationObject as DTE2;
      _dte.ThrowIfArgumentNull(Resources.Error_InvalidDTE);

      if (_solution == null)
      {
        // ReSharper disable once SuspiciousTypeConversion.Global
        _solution = _dte.Solution as Solution4;
        _solution.ThrowIfArgumentNull(Resources.Error_InvalidDTESolution);
      }

      _runKind = runKind;

      // If we are creating a new SMA solution
      if (runKind == WizardRunKind.AsMultiProject)
      {
        if (replacementsDictionary.NewSolution() == false)
          throw new InvalidOperationException(Resources.Error_SolutionAlreadyExists);

        _smaSolution      = new SMASolution(_solution, templateRoot, replacementsDictionary);
        _replacementsDict = replacementsDictionary;
        _templateRoot     = templateRoot;
      }

      // We are either creating a new project in an existing SMA solution, in which case this is a
      // single project install, or creating a project in a new SMA solution (see previous comment)
      else if (runKind == WizardRunKind.AsNewProject)
      {
        if (_smaSolution == null)
        {
          if (replacementsDictionary.NewSolution())
            throw new InvalidOperationException(Resources.Error_SmaSolutionNull);

          _smaSolution = new SMASolution(_solution);
        }

        _currentProject = new SMAProjectInstall(_smaSolution, templateRoot, replacementsDictionary);

        replacementsDictionary["$projectname$"]          = _currentProject.ProjectName;
        replacementsDictionary["$safeprojectname$"]      = _currentProject.ProjectName;
        replacementsDictionary["$finalProjectName$"]     = _currentProject.ProjectName;
        replacementsDictionary["$finalSafeProjectName$"] = _currentProject.ProjectName;
        replacementsDictionary["$finalPluginName$"]      = _currentProject.PluginName;
        replacementsDictionary["$targetDirMacro$"]       = "$(TargetDir)";
        replacementsDictionary["$projectNameMacro$"]     = "$(ProjectName)";
      }
    }

    /// <inheritdoc />
    public void ProjectFinishedGenerating(Project project) { }

    /// <inheritdoc />
    public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

    /// <inheritdoc />
    public bool ShouldAddProjectItem(string filePath)
    {
      return true;
    }

    /// <inheritdoc />
    public void BeforeOpeningFile(ProjectItem projectItem) { }

    /// <inheritdoc />
    public void RunFinished()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (_runKind == WizardRunKind.AsMultiProject)
      {
        _smaSolution.LoadProjects();

        // We trigger the template installation manually to get control over the destination folder
        _smaSolution.InstallProjects(
          _replacementsDict, _templateRoot,
          ("Plugin", "Plugins", string.Empty),
          ("PluginTest", "Tests", ".Tests"));

        var solCfgs = _smaSolution.VSSolution.SolutionBuild.SolutionConfigurations;
        solCfgs.Add("1. Alpha (Debug)", "Debug", false);
        solCfgs.Add("2. Beta (Release)", "Release", false);
        solCfgs.Add("3. Stable (Release)", "Release", false);
      }

      //if (_currentProject == null)
      //  return;

      //_currentProject.DeleteOriginalFolder();

      //_currentProject = null;
    }

    #endregion
  }
}
