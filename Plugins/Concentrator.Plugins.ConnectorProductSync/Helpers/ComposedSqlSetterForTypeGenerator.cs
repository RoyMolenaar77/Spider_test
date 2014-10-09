using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{
  public class ComposedSqlSetterForTypeGenerator : IGenerateSqlSetterForType
  {
    IList<IGenerateSqlSetterForType> generators = new List<IGenerateSqlSetterForType>();

    public ComposedSqlSetterForTypeGenerator()
    {
      Add<string>((x, y) => string.Format("{0}='{1}'", x, y != null ? y.Replace("'", "''").Replace("@", "@@") : "NULL"));
      Add<bool>((x, y) => string.Format("{0}={1}", x, y ? "1" : "0"));
      Add<bool?>((x, y) => string.Format("{0}={1}", x, y.HasValue ? (y.Value ? "1" : "0") : "NULL"));
      Add<int>((x, y) => string.Format("{0}={1}", x, y));
      Add<int?>((x, y) => string.Format("{0}={1}", x, y.HasValue ? y.Value.ToString() : "NULL"));
    }

    bool IGenerateSqlSetterForType.CanGenerateFor(Type type)
    {
      return generators.Any(x => x.CanGenerateFor(type));
    }

    string IGenerateSqlSetterForType.Generate(Type typeToGenerateFor, string field, object value)
    {
      var generator = generators.FirstOrDefault(x => x.CanGenerateFor(typeToGenerateFor));

      if (generator == null)
      {
        throw new Exception("No generator found for type {0}");
      }

      return generator.Generate(typeToGenerateFor, field, value);
    }

    private void Add<T>(Func<string, T, string> generator)
    {
      generators.Add(new SqlSetterForTypeGenerator<T>(generator));
    }
  }
}
