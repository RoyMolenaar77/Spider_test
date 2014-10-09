using System;

namespace Concentrator.Magento
{
  public partial class ConcentratorDatabase
  {
    public static partial class Magento
    {
      public partial class UpdateCatalogProductAttributeParameters
      {
        public Int32? CatalogProductAttributeID
        {
          get;
          set;
        }

        public String AttributeType
        {
          get;
          set;
        }

        public String DefaultValue
        {
          get;
          set;
        }
      }
    }
  }
}
