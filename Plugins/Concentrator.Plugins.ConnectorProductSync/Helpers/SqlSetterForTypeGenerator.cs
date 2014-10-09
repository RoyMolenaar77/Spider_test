using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{
  public class SqlSetterForTypeGenerator<T> : IGenerateSqlSetterForType
  {
    Func<string, T, string> _generator;

    public SqlSetterForTypeGenerator(Func<string, T, string> generator)
    {
      _generator = generator;
    }

    bool IGenerateSqlSetterForType.CanGenerateFor(Type type)
    {
      return typeof(T) == type;
    }

    string IGenerateSqlSetterForType.Generate(Type typeToGeneratorFor, string field, object value)
    {
      return _generator(field, (T)value);
    }
  }
}
