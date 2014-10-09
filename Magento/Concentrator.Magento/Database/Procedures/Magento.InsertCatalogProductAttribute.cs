using System;

namespace Concentrator.Magento
{
  public partial class ConcentratorDatabase
  {
    public static partial class Magento
    {
      public partial class InsertCatalogProductAttributeParameters
      {
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

        public Boolean? IsFilter
        {
          get;
          set;
        }
      }
    }
  }
}
