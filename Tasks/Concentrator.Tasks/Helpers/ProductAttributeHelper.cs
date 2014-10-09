using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Microsoft.Practices.ServiceLocation;

using PetaPoco;

namespace Concentrator.Tasks
{
  using Objects.DataAccess.UnitOfWork;
  using Objects.Environments;
  using Objects.Models.Attributes;
  using Objects.Models.Vendors;
  using Objects.Sql;

  public static class ProductAttributeHelper
  {
    /// <summary>
    /// This method will use reflection to set the property or field decorated with the <see cref="Concentrator.Tasks.ProductAttributeAttribute"/>.
    /// The property or field can be of type <see cref="System.Int32"/> or <see cref="Concentrator.Objects.Models.Attributes.ProductAttributeMetaData"/>.
    /// </summary>
    public static Boolean Bind(Object instance, Vendor vendor = null, TraceSource traceSource = null)
    {
      if (instance == null)
      {
        throw new ArgumentNullException("instance");
      }

      using (var database = new Database(Environments.Current.Connection, Database.MsSqlClientProvider))
      {
        var query = new QueryBuilder()
          .From("[dbo].[ProductAttributeMetaData]")
          .Where("[VendorID] = @0 OR [VendorID] IS NULL AND @0 IS NULL")
          .Select();

        var productAttributes = database
          .Query<ProductAttributeMetaData>(query, vendor != null
            ? (Int32?)vendor.VendorID
            : null)
          .OrderBy(productAttribute => productAttribute.Index)
          .ToLookup(productAttribute => productAttribute.AttributeCode);
        
        var success = true;

        foreach (var memberInfo in instance.GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
          var memberAttribute = memberInfo.GetCustomAttributes(typeof(ProductAttributeAttribute), true).SingleOrDefault() as ProductAttributeAttribute;
          
          if (memberAttribute != null)
          {
            var productAttribute = productAttributes[memberAttribute.AttributeCode]
              .FirstOrDefault(item => !memberAttribute.IsConfigurable.HasValue || memberAttribute.IsConfigurable.Value == item.IsConfigurable);

            if (productAttribute != null)
            {
              switch (memberInfo.MemberType)
              {
                case MemberTypes.Field:
                  var fieldInfo = memberInfo as FieldInfo;

                  if (fieldInfo.FieldType == typeof(ProductAttributeMetaData))
                  {
                    fieldInfo.SetValue(instance, productAttribute);
                  }

                  if (fieldInfo.FieldType == typeof(Int32))
                  {
                    fieldInfo.SetValue(instance, productAttribute.AttributeID);
                  }
                  break;

                case MemberTypes.Property:
                  var propertyInfo = memberInfo as PropertyInfo;

                  if (propertyInfo.PropertyType == typeof(ProductAttributeMetaData))
                  {
                    propertyInfo.SetValue(instance, productAttribute, null);
                  }

                  if (propertyInfo.PropertyType == typeof(Int32))
                  {
                    propertyInfo.SetValue(instance, productAttribute.AttributeID, null);
                  }
                  break;
              }
            }
            else
            {
              if (traceSource != null)
              {
                traceSource.TraceError("The {0}product attribute with the attribute-code '{1}' does not exist."
                  , memberAttribute.IsConfigurable.HasValue
                    ? memberAttribute.IsConfigurable.Value
                      ? "configurable "
                      : "simple "
                    : String.Empty
                  , memberAttribute.AttributeCode);
              }

              success = false;
            }            
          }
        }

        return success;
      }
    }
  }
}