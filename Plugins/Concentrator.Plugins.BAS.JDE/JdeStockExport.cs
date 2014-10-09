using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Data.SqlClient;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors.Base;
using System.Configuration;
using Concentrator.Objects.DataAccess.EntityFramework;

namespace Concentrator.Plugins.BAS.JDE
{
  public class JdeStockExport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "JDE retail stock import"; }
    }

    protected override void Process()
    {
      try
      {
        string jdeConnectionString = ConfigurationManager.ConnectionStrings["JDE"].ConnectionString;


        using (var unit = GetUnitOfWork())
        {
          string jdeTime = DateTime.Now.ToString("HHmmss");

          var retailStockList = (from v in unit.Scope.Repository<VendorMapping>().GetAll()
                                 let setting = v.Vendor.VendorSettings.FirstOrDefault(x => x.SettingKey == "AssortmentImportID")
                                 where setting != null
                                 join vs in unit.Scope.Repository<VendorStock>().GetAll() on v.MapVendorID equals vs.VendorID
                                 join va in unit.Scope.Repository<VendorAssortment>().GetAll() on v.VendorID equals va.VendorID
                                 where va.ProductID == vs.ProductID && vs.QuantityOnHand > 0
                                 select new FB41021S
                                 {
                                   SS55RID = v.Vendor1.BackendVendorCode,
                                   SS55SLID = v.Vendor1.Name,
                                   SSITM = va.CustomItemNumber,
                                   SSLITM = va.CustomItemNumber,
                                   SSPQOH = vs.QuantityOnHand,
                                   SSSTKT = "S",
                                   SSUPMT = jdeTime
                                 }).AsEnumerable();

          using (JdeBulkRetail retailStock = new JdeBulkRetail(retailStockList, jdeConnectionString))
          {
            retailStock.Init(unit.Context);
            retailStock.Sync(unit.Context);
          }
        }
      }
      catch (Exception ex)
      {
        log.AuditFatal("JDE retailstock import failed", ex);
      }
    }
  }

}
