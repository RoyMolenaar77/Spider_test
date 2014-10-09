using System;
using System.Linq;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.BAS.Vendors.BAS.WebService;
using Concentrator.Objects.Models.Vendors;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Bulk;
using System.Collections.Generic;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.BAS
{
  public class BarcodeImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "BAS Barcode Bulk Import Plugin"; }
    }

    protected override void Process()
    {
      foreach (Vendor vendor in Vendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.Barcodes) && x.IsActive))
      {
        try
        {
          if (vendor.VendorSettings.GetValueByKey<int>("AssortmentImportID", 0) < 1)
            continue;

          log.DebugFormat("Start Barcodes Import for Vendor '{0} ({1})'", vendor.Name, vendor.VendorID);

          using (var unit = GetUnitOfWork())
          {
            DataSet barcodes = new DataSet();

            using (JdeAssortmentSoapClient cl = new JdeAssortmentSoapClient())
            {
              barcodes = cl.GetBarcodes();

              var dataList = (from b in barcodes.Tables[0].AsEnumerable()
                              where VendorImportUtility.SetDataSetValue("ivxrt", b) != "DC"
                              select new Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode
                              {
                                Barcode = VendorImportUtility.SetDataSetValue("IVCITM", b),
                                CustomItemNumber = VendorImportUtility.SetDataSetValue("IVITM", b),
                                Type = 0,
                                VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID
                              }).AsEnumerable();

              using (var vendorAssortmentBulk = new VendorBarcodeBulk(dataList, vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID, 0))
              {
                vendorAssortmentBulk.Init(unit.Context);
                vendorAssortmentBulk.Sync(unit.Context);
              }

              var sapDataList = (from b in barcodes.Tables[0].AsEnumerable()
                              where VendorImportUtility.SetDataSetValue("ivxrt", b) == "DC"
                              select new Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode
                              {
                                Barcode = VendorImportUtility.SetDataSetValue("IVCITM", b),
                                CustomItemNumber = VendorImportUtility.SetDataSetValue("IVITM", b),
                                Type = 3,
                                VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID
                              }).AsEnumerable();

              using (var vendorAssortmentBulk = new VendorBarcodeBulk(sapDataList, vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID,3))
              {
                vendorAssortmentBulk.Init(unit.Context);
                vendorAssortmentBulk.Sync(unit.Context);
              }
            }
          }

          log.DebugFormat("Finished Barcode Import for vendor '{0} ({1})'", vendor.Name, vendor.VendorID);
        }
        catch (Exception ex)
        {
          log.Error("Error import BAS assortment", ex);
        }

      }
    }
  }
}
