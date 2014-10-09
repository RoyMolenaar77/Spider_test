using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{

  public class GenerateUpdateProperties : IGenerateUpdateProperties
  {
    public List<string> GenerateIgnoreProperties<T>(T source, params Expression<Func<T, object>>[] ignoreProperties)
    {
      List<string> properties = new List<string>();

      foreach (var ignorePropery in ignoreProperties)
      {
        string property = ((ignorePropery.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
        properties.Add(property);
      }

      return properties;
    }

    public List<string> GetPropertiesForUpdate<T>(T obj, T currentObj, IEnumerable<string> ignoreProperties)
    {
      List<string> changes = new List<string>();

      IGenerateSqlSetterForType composedGenerator = new ComposedSqlSetterForTypeGenerator();

      if (obj == null) return changes;
      var objType = obj.GetType();

      var objProperties = objType.GetProperties().Where(x => !ignoreProperties.Contains(x.Name));
      foreach (var objProperty in objProperties)
      {
        var propValue = objProperty.GetValue(obj, null);
        var currentPropValue = objProperty.GetValue(currentObj, null);

        var isSourceNullButTargetNot = propValue == null && currentPropValue != null;
        var isTargetNullButSourceNot = propValue != null && currentPropValue == null;
        var areDifferent = isSourceNullButTargetNot
                        || isTargetNullButSourceNot
                        || (propValue != null && !propValue.Equals(currentPropValue))
                        || (currentPropValue != null && !currentPropValue.Equals(propValue));

        if (areDifferent)
        {
          var setter = composedGenerator.TryGenerate(objProperty.PropertyType, objProperty.Name, propValue);
          if (setter != null)
            changes.Add(setter);
        }            
      }
      return changes;
    }

    public List<string> GetPropertiesForUpdate<T>(T obj, T currentObj)
    {
      return GetPropertiesForUpdate(obj, currentObj, Enumerable.Empty<string>());
    }
  }
}
