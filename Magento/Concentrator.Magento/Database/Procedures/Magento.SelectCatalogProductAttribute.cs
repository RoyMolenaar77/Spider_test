using System;

namespace Concentrator.Magento
{
  public partial class ConcentratorDatabase
  {
    public static partial class Magento
    {
      public partial class SelectCatalogProductAttributeParameters
      {
        public Int32? CatalogProductAttributeID
        {
          get;
          set;
        }

        public Int32? ConnectorID
        {
          get;
          set;
        }

        public Int32? AttributeID
        {
          get;
          set;
        }

        public Boolean? IsFilter
        {
          get;
          set;
        }
      }
    }
  }
}
