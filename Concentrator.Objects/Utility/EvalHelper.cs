using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Concentrator.Objects.Utility
{
 public static class EvalHelper
  {
    public static object Eval(string sCSCode)
    {
      var compiler = new CSharpCodeProvider();
      var compilerParameters = new CompilerParameters
      {
        CompilerOptions = "/t:library",
        GenerateInMemory = true,
        ReferencedAssemblies =
        {
          "system.dll",
          "system.xml.dll",
          "system.data.dll",
          "system.windows.forms.dll",
          "system.drawing.dll"
        }
      };

      var stringBuilder = new StringBuilder();

      stringBuilder.AppendLine("using System;");
      stringBuilder.AppendLine("using System.Xml;");
      stringBuilder.AppendLine("using System.Data;");
      stringBuilder.AppendLine("using System.Data.SqlClient;");
      stringBuilder.AppendLine("using System.Drawing;");
      stringBuilder.AppendLine("using System.Windows.Forms;");

      stringBuilder.AppendLine("namespace CSCodeEvaler");
      stringBuilder.AppendLine("{");
      stringBuilder.AppendLine("\tpublic class CSCodeEvaler");
      stringBuilder.AppendLine("\t{");
      stringBuilder.AppendLine("\t\tpublic Object EvalCode()");
      stringBuilder.AppendLine("\t\t{");
      stringBuilder.AppendLine("\t\t\treturn " + sCSCode + ";");
      stringBuilder.AppendLine("\t\t}");
      stringBuilder.AppendLine("\t}");
      stringBuilder.AppendLine("}");
       
      var compilerResult = compiler.CompileAssemblyFromSource(compilerParameters, stringBuilder.ToString());

      if (compilerResult.Errors.Count > 0)
      {
        return null;
      }

      var assembly = compilerResult.CompiledAssembly;
      var codeEvaluator = assembly.CreateInstance("CSCodeEvaler.CSCodeEvaler");

      var codeEvaluatorType = codeEvaluator.GetType();
      var methodInfo = codeEvaluatorType.GetMethod("EvalCode");

      return methodInfo.Invoke(codeEvaluator, null);
    }
  }
}
