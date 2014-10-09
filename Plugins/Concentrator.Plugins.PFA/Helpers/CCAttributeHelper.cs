using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Helpers
{
  public static class CCAttributeHelper
  {
    public static List<AttributeVendorMetaData> Attributes = new List<AttributeVendorMetaData>()
    {
      new AttributeVendorMetaData(){
      Code = "Season",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID, 
      UsedOnConfigurableLevel = true,
      GetAttributeValue = (c => c.SeasonCode)
      },

      new AttributeVendorMetaData(){
      Code = "UserMoment",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      },
      new AttributeVendorMetaData(){
      Code = "Mentality",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      },
      new AttributeVendorMetaData(){
      Code = "Targetgroup",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      UsedOnConfigurableLevel = true,
      GetAttributeValue =(c => c.GroupCode1)
      },
      new AttributeVendorMetaData(){
      Code = "Productgroup",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID, 
      UsedOnConfigurableLevel = true,
      GetAttributeValue = (c => c.GroupCode2)
      },
      new AttributeVendorMetaData(){
      Code = "Subproductgroup",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      UsedOnConfigurableLevel = true,
      GetAttributeValue = (c => c.GroupCode3)
      },
      new AttributeVendorMetaData(){
      Code = "InputCode",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      },
     
      new AttributeVendorMetaData(){
      Code = "Module",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      },
      new AttributeVendorMetaData(){
      Code = "ShopWeek",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID,       
      },
      new AttributeVendorMetaData(){
      Code = "Style",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID      
      },
      new AttributeVendorMetaData(){
      Code = "Size",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID, 
      Configurable = true,
      Searchable = true
      },
      new AttributeVendorMetaData(){
      Code = "Color",
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID, 
      Configurable = true,
      Searchable = true
      },
      new AttributeVendorMetaData(){
      Code = "MaterialDescription",
      UsedOnConfigurableLevel = true,
      VendorID = PFAImport.CONCENTRATOR_VENDOR_ID, 
      GetAttributeValue = (info => info.Material)
     },
     new AttributeVendorMetaData(){
       Code = "Gender",
       VendorID = PFAImport.CONCENTRATOR_VENDOR_ID
     }
    };
  }
}
