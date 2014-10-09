using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Microsoft.Practices.ServiceLocation;

namespace Concentrator.Tasks.Stores
{
  using Objects.DataAccess.Repository;
  using Objects.DataAccess.UnitOfWork;
  using Objects.Models.Attributes;

  public abstract class ProductAttributeStore : StoreBase
  {
    public ProductAttributeStore(TraceSource traceSource = null) : base(traceSource)
    {
    }

    public override Boolean Load()
    {
      var loadResult = true;

      var productAttributeLookup = ServiceLocator
        .Current
        .GetInstance<IUnitOfWork>()
        .Scope
        .Repository<ProductAttributeMetaData>()
        .GetAll()
        .ToLookup(item => item.AttributeCode, StringComparer.CurrentCultureIgnoreCase);
      
      foreach (var property in GetProperties(property => property.PropertyType == typeof(ProductAttributeMetaData)))
      {
        var attribute = property
          .GetCustomAttributes(false)
          .OfType<ProductAttributeAttribute>()
          .SingleOrDefault();

        if (attribute != null)
        {
          var productAttribute = productAttributeLookup[attribute.AttributeCode].FirstOrDefault(item => !attribute.IsConfigurable.HasValue || attribute.IsConfigurable.Value == item.IsConfigurable);

          if (productAttribute == null)
          {
            TraceSource.TraceEvent(TraceEventType.Error, 0, "Unable to find the {0}product attribute with the code '{1}'."
              , attribute.IsConfigurable.HasValue
                ? attribute.IsConfigurable.Value ? "configurable " : "simple "
                : String.Empty
              , attribute.AttributeCode);

            loadResult = false;

            continue;
          }

          property.SetValue(this, productAttribute, null);
        }
        else
        {
          TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Property '{0}' of type '{1}' does not have a '{2}' specified."
            , property.Name
            , property.DeclaringType.FullName
            , typeof(ProductAttributeAttribute).FullName);
        }
      }

      return loadResult;
    }
  }
}