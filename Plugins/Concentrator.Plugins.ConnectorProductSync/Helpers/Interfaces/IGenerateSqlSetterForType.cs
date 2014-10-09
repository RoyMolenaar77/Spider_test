using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{
  public interface IGenerateSqlSetterForType
  {
    bool CanGenerateFor(Type type);
    string Generate(Type typeToGeneratorFor, string field, object value);
  }

  public static class GenerateSqlSetterForTypeExtensions
  {
    public static string TryGenerate(this IGenerateSqlSetterForType generator, Type type, string field, object value)
    {
      return generator.CanGenerateFor(type) ? generator.Generate(type, field, value) : null;
    }
  }
}
