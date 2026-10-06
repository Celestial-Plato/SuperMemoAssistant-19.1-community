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
// Created On:   2020/04/03 17:37
// Modified On:  2020/04/03 17:38
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sdk.VisualStudio
{
  using Extensions;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell.Interop;
  using Utils.VS;

  public sealed partial class SMASdkVSPackage
  {
    #region Methods Impl

    /// <inheritdoc />
    int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
    {
      this.WriteDebug($"[SMA] OnAfterOpenProject'.");
      var p = pHierarchy.GetProject(Dte2);

      if (p != null)
        SetProjectProperty(p);
      
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
    {
      this.WriteDebug($"[SMA] OnAfterLoadProject'.");
      var p = pRealHierarchy.GetProject(Dte2);
      
      if (p != null)
        SetProjectProperty(p);
      
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    #endregion
  }
}
