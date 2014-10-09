using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.VisualStudio.TextTemplating
{
  public static class TextTransformationExtensions
  {
    public static IDisposable Indent(this TextTransformation textTransformation, String indent)
    {
      textTransformation.PushIndent(indent);

      return new ActionDisposable(delegate
      {
        textTransformation.PopIndent();
      });
    }

    public static IDisposable Scope(this TextTransformation textTransformation, String indent)
    {
      textTransformation.WriteLine("{");
      textTransformation.PushIndent(indent);

      return new ActionDisposable(delegate
      {
        textTransformation.PopIndent();
        textTransformation.WriteLine("}");
      });
    }

    public static void WriteLine(this TextTransformation textTransformation)
    {
      textTransformation.WriteLine(String.Empty);
    }
  }
}
