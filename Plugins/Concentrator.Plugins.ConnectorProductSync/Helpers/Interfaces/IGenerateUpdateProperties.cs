using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{
  public interface IGenerateUpdateProperties
  {
    List<string> GenerateIgnoreProperties<T>(T source, params Expression<Func<T, object>>[] ignoreProperties);
    List<string> GetPropertiesForUpdate<T>(T obj, T currentObj);
    List<string> GetPropertiesForUpdate<T>(T obj, T currentObj, IEnumerable<string> ignoreProperties);
  }
}
