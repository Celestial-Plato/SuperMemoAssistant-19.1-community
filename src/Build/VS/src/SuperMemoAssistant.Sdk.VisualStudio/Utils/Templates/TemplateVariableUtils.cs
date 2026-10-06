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
// Created On:   2020/04/01 15:50
// Modified On:  2020/04/01 15:58
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio.Utils.Templates
{
  using System.Collections.Generic;
  using System.IO;
  using System.Text.RegularExpressions;

  public static class TemplateVariableUtils
  {
    #region Constants & Statics

    private static readonly Regex RE_Variable = new Regex(@"\$[\w-\.]+\$", RegexOptions.Compiled);

    #endregion




    #region Methods

    /// <summary>
    ///   Replace $variables$ in <paramref name="file" />'s content as defined by
    ///   <paramref name="replacementDic" />
    /// </summary>
    /// <param name="replacementDic">The available variables</param>
    /// <param name="file">The file from which to load the content to process and edit</param>
    /// <returns>The edited content with variables replaced</returns>
    public static string ReplaceVariables(this Dictionary<string, string> replacementDic, FileInfo file)
    {
      using (var fs = file.OpenRead())
      using (var sr = new StreamReader(fs))
        return replacementDic.ReplaceVariables(sr.ReadToEnd());
    }

    /// <summary>
    ///   Replace $variables$ in <paramref name="content" /> as defined by
    ///   <paramref name="replacementDic" />
    /// </summary>
    /// <param name="replacementDic">The available variables</param>
    /// <param name="content">Content to process and edit</param>
    /// <returns>The edited content with variables replaced</returns>
    public static string ReplaceVariables(this Dictionary<string, string> replacementDic, string content)
    {
      return RE_Variable.Replace(content, m => ReplaceVariable(m, replacementDic));
    }

    private static string ReplaceVariable(Match match, Dictionary<string, string> replacementDir)
    {
      var key = match.Value;

      replacementDir.TryGetValue(key, out var value);

      return value;
    }

    #endregion
  }
}
