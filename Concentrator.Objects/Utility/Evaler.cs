using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.CSharp;

namespace Concentrator.Objects.Utility
{
  public class Evaler
  {
    private CodeDomProvider _codeProvider;
    private CompilerParameters _compilerParams;

    public Evaler()
    {
      InitCompiler();
    }

    private void InitCompiler()
    {
      _codeProvider = new CSharpCodeProvider();

      _compilerParams = new CompilerParameters
      {
        CompilerOptions = "/t:library",
        GenerateInMemory = true,
        ReferencedAssemblies =
        {
          "System.dll",
          "System.Xml.dll",
          "System.Data.dll",
          "System.Windows.Forms.dll",
          "System.Drawing.dll"
        }
      };

      //_compilerParams.ReferencedAssemblies.Add("system.dll");
      //_compilerParams.ReferencedAssemblies.Add("system.xml.dll");
      //_compilerParams.ReferencedAssemblies.Add("system.data.dll");
      //_compilerParams.ReferencedAssemblies.Add("system.windows.forms.dll");
      //_compilerParams.ReferencedAssemblies.Add("system.drawing.dll");

    }

    public object EvalExpression(string expression)
    {
      string com = ConstructSource(expression);

      CompilerResults cr = _codeProvider.CompileAssemblyFromSource(_compilerParams, com);
      if (cr.Errors.Count > 0)
      {

        return null;
      }

      System.Reflection.Assembly a = cr.CompiledAssembly;
      object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");

      Type t = o.GetType();
      MethodInfo mi = t.GetMethod("EvalCode");

      object s = mi.Invoke(o, null);
      return s;
    }

    private string ConstructSource(string expression)
    {
      StringBuilder sb = new StringBuilder("");
      sb.Append("using System;\n");
      sb.Append("using System.Xml;\n");
      sb.Append("using System.Data;\n");
      sb.Append("using System.Data.SqlClient;\n");
      sb.Append("using System.Windows.Forms;\n");
      sb.Append("using System.Drawing;\n");

      sb.Append("namespace CSCodeEvaler{ \n");
      sb.Append("public class CSCodeEvaler{ \n");
      sb.Append("public object EvalCode(){\n");
      sb.Append("return " + expression + "; \n");
      sb.Append("} \n");
      sb.Append("} \n");
      sb.Append("}\n");

      return sb.ToString();
    }
  }
}
