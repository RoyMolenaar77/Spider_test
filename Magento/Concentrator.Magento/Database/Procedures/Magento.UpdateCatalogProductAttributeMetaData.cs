using System;

namespace Concentrator.Magento
{
  public partial class ConcentratorDatabase
  {
    public static partial class Magento
    {
      public partial class UpdateCatalogProductAttributeMetaDataParameters
      {
        public Int32? CatalogProductAttributeID
        {
          get;
          set;
        }

        public String Key
        {
          get;
          set;
        }

        public String Value
        {
          get;
          set;
        }

        public String Type
        {
          get;
          set;
        }
      }
    }
  }
}
