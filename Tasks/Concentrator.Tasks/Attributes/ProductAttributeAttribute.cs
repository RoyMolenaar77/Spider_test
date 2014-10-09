using System;

namespace Concentrator.Tasks
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public sealed class ProductAttributeAttribute : Attribute
  {
    public String AttributeCode
    {
      get;
      private set;
    }

    public Boolean? IsConfigurable
    {
      get;
      set;
    }

    public ProductAttributeAttribute(String attributeCode)
    {
      AttributeCode = attributeCode;
    }

    public ProductAttributeAttribute(String attributeCode, Boolean isConfigurable)
      : this(attributeCode)
    {
      IsConfigurable = isConfigurable;
    }
  }
}
