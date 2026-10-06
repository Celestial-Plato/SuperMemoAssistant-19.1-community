using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Sdk.VisualStudio.Extensions
{
  public static class ExceptionEx
  {
    public static void ThrowIfArgumentNull(this object obj, string errMsg)
    {
      if (obj == null)
        throw new ArgumentNullException(errMsg);
    }

    public static void ThrowIfNull(this object obj, string errMsg)
    {
      if (obj == null)
        throw new NullReferenceException(errMsg);
    }

    public static void ThrowIfNotNull(this object obj, string errMsg)
    {
      if (obj != null)
        throw new InvalidOperationException(errMsg);
    }

    public static void ThrowIfMissing(this DirectoryInfo dir, string errMsg)
    {
      if (dir.Exists == false)
        throw new InvalidOperationException(errMsg);
    }

    public static void ThrowIfMissing(this FileInfo file, string errMsg)
    {
      if (file.Exists == false)
        throw new InvalidOperationException(errMsg);
    }
  }
}
