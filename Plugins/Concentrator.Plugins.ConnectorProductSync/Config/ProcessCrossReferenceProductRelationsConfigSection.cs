using System.Configuration;

namespace Concentrator.Plugins.ConnectorProductSync.Config
{
  public class ProcessCrossReferenceProductRelationsConfigSection : ConfigurationSection
  {
    [ConfigurationProperty("CreateRelatedProductsWithVendorID", IsRequired = true)]
    public int CreateRelatedProductsWithVendorID
    {
      get { return (int)this["CreateRelatedProductsWithVendorID"]; }
      set { this["CreateRelatedProductsWithVendorID"] = value; }
    }
    [ConfigurationProperty("CreateRelatedProductsWithUserID", IsRequired = true)]
    public int CreateRelatedProductsWithUserID
    {
      get { return (int)this["CreateRelatedProductsWithUserID"]; }
      set { this["CreateRelatedProductsWithUserID"] = value; }
    }
  }
}