using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using Concentrator.Plugins.Axapta.Helpers;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class VendorStockRepository : UnitOfWorkPlugin, IVendorStockRepository
  {
    public VendorStock GetVendorStock(int productID, int vendorID)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<VendorStock>()
          .GetSingle(x => x.ProductID == productID && x.VendorID == vendorID);
      }
    }

    public bool IsVendorStockExists(int productID, int vendorID)
    {
      using (var db = GetUnitOfWork())
      {
        var vendorStock = db
          .Scope
          .Repository<VendorStock>()
          .GetSingle(x => x.ProductID == productID && x.VendorID == vendorID);
        return vendorStock != null;
      }
    }

    public Boolean InsertVendorStock(VendorStock vendorStock)
    {
      using (var db = GetUnitOfWork())
      {
        try
        {
          db.Scope.Repository<VendorStock>().Add(vendorStock);
          db.Save();
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    public void UpdateVendorStockQuantity(int productID, int vendorID, int quantityOnHand)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        db.Execute(string.Format(@"
          UPDATE VendorStock
          SET QuantityOnHand = {0}
          WHERE ProductID = {1}
	          AND VendorID = {2}
        ", quantityOnHand, productID, vendorID));
      }
    }

    public IEnumerable<VendorStock> GetListOfVendorStockByVendorID(int vendorID)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        IEnumerable<VendorStock> vendorStocks = db.Query<VendorStock>(string.Format(@"
          SELECT ProductID
	          ,VendorID
	          ,QuantityOnHand
          FROM VendorStock
          WHERE VendorID = {0}
        ", vendorID));

        return vendorStocks;
      }
    }

    public bool InsertVendorStocks(IEnumerable<VendorStock> listOfVendorStocks)
    {
      using (var db = GetUnitOfWork())
      {
        try
        {
          foreach (var stock in listOfVendorStocks)
          {
            db.Scope.Repository<VendorStock>().Add(stock);
          }
          db.Save();
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    public bool UpdateVendorStocksQuantity(IEnumerable<VendorStock> listOfVendorStocks)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        try
        {
          foreach (var stock in listOfVendorStocks)
          {
            db.Execute(string.Format(@"
              UPDATE VendorStock
              SET QuantityOnHand = {0}
              WHERE ProductID = {1}
	              AND VendorID = {2}
            ", stock.QuantityOnHand, stock.ProductID, stock.VendorID));
          }
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }
  }
}
