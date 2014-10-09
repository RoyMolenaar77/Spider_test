using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Concentrator.Objects.Models
{
  using Attributes;
  using Connectors;

  /// <summary>
  /// Represents a business logic layer object for a product attribute mapping between Concentrator and Magento.
  /// </summary>
  public class ConnectorProductAttributeMapping : IEquatable<ConnectorProductAttributeMapping>
  {
    /// <summary>
    /// Gets or sets the Connector this mapping belongs to.
    /// </summary>
    public Connector Connector
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the default value of the connector product attribute mapping.
    /// By default this property is set to <see cref="String.Empty"/>.
    /// </summary>
    public Object DefaultValue
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets whether the connector product attribute mapping is a filter.
    /// </summary>
    public Boolean IsFilter
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the Product Attribute this mapping is representing.
    /// </summary>
    public ProductAttributeMetaData ProductAttribute
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the Product Attribute this mapping is representing.
    /// </summary>
    public Type ProductAttributeType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets a dictionary of properties that apply to this mapping.
    /// </summary>
    public IDictionary<String, Object> Properties
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the <see cref="IEqualityComparer<String>"/> that is used to compare the keys of the properties.
    /// </summary>
    public static IEqualityComparer<String> PropertiesComparer
    {
      get
      {
        return StringComparer.InvariantCultureIgnoreCase;
      }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Concentrator.Objects.Models.ConnectorProductAttributeMapping" /> class.
    /// </summary>
    public ConnectorProductAttributeMapping()
      : this(null)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="Concentrator.Objects.Models.ConnectorProductAttributeMapping" /> class.
    /// </summary>
    public ConnectorProductAttributeMapping(IDictionary<String, Object> properties)
    {
      DefaultValue = String.Empty;
      ProductAttributeType = typeof(String);
      Properties = properties != null
        ? new Dictionary<String, Object>(properties, PropertiesComparer)
        : new Dictionary<String, Object>(PropertiesComparer);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Concentrator.Objects.Models.ConnectorProductAttributeMapping" /> class.
    /// </summary>
    public ConnectorProductAttributeMapping(Object objectWithProperties)
      : this(GetPropertiesFromObject(objectWithProperties))
    {
    }

    private static IDictionary<String, Object> GetPropertiesFromObject(Object objectWithProperties, BindingFlags bindingFlags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
    {
      return objectWithProperties
        .GetType()
        .GetProperties(bindingFlags)
        .ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(objectWithProperties, null), PropertiesComparer);
    }

    public override Int32 GetHashCode()
    {
      return (Connector != null ? Connector.GetHashCode() : 0) 
        ^ (ProductAttribute != null ? ProductAttribute.GetHashCode() : 0) 
        ^ IsFilter.GetHashCode();
    }

    public override String ToString()
    {
      return String.Format("Connector: '{0}' - '{1}'", Connector.Name, ProductAttribute.AttributeCode);
    }

    Boolean IEquatable<ConnectorProductAttributeMapping>.Equals(ConnectorProductAttributeMapping other)
    {
      return Connector == other.Connector && ProductAttribute == other.ProductAttribute && IsFilter == other.IsFilter;
    }
  }
}
