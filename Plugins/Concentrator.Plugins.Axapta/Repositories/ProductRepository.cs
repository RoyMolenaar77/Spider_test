using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class ProductRepository : UnitOfWorkPlugin, IProductRepository
  {
    public Product GetProductByID(int productID)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<Product>()
          .GetSingle(x => x.ProductID == productID);
      }
    }

    public Product GetProductByVendorItemNumber(string vendorItemNumber)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<Product>()
          .GetSingle(x => x.VendorItemNumber == vendorItemNumber);
      }
    }

    public int GetProductIDByVendorItemNumber(string vendorItemNumber)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        Int32 productID = db.SingleOrDefault<Int32>(string.Format(@"
          SELECT ProductID
          FROM Product
          WHERE VendorItemNumber = '{0}'
        ", vendorItemNumber.Replace("@", "@@").Replace("'", "''")));

        return productID;
      }
    }

    public Product GetProductByVendorItemNumberAndBarcode(string vendorItemNumber, string barcode)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<Product>()
          .GetSingle(x => x.VendorItemNumber == vendorItemNumber && x.ProductBarcodes.Any(y => y.Barcode == barcode));
      }
    }

    public int GetProductIDByVendorItemNumberAndBarcode(string vendorItemNumber, string barcode)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        var product = db.SingleOrDefault<Int32>(string.Format(@"
          select p.ProductID 
          from product p
          inner join ProductBarcode pb on p.ProductID = pb.ProductID
          where VendorItemNumber = '{0}' and pb.Barcode = '{1}' 
        ", vendorItemNumber.Replace("@","@@").Replace("'", "''")
         , barcode.Replace("\"", "")));

        return product;
      }
    }

    #region Queries
    private const string GetSkusByVendorID = @";WITH  Attributes
                  AS ( SELECT pav.ProductID
                       ,      Value
                       ,      AttributeCode
                       FROM   dbo.Product p
                       INNER JOIN dbo.ProductAttributeValue pav ON p.ProductID = pav.ProductID
                       INNER JOIN dbo.ProductAttributeMetaData pamd ON pav.AttributeID = pamd.AttributeID
                       WHERE  (
                                AttributeCode = 'Size'
                                OR AttributeCode = 'Color'
                              )
                              AND p.SourceVendorID IN ({0})
                     ),
           Products AS (
            SELECT  ProductID
            ,       color.ColorValue
            ,       size.Value
            FROM    Attributes size
            CROSS APPLY ( SELECT TOP 1
                                  ColorValue = Value
                          FROM    Attributes color
                          WHERE   size.ProductID = color.ProductID
                                  AND color.AttributeCode = 'Color'
                        ) color
            WHERE   size.AttributeCode = 'Size'
	          )
	          SELECT  dbo.Product.ProductID
	          ,       VendorItemNumber
	          ,       ColorValue AS Color
	          ,       Value AS Size
	          FROM Product
	          INNER JOIN Products ON dbo.Product.ProductID = Products.ProductID
	          WHERE SourceVendorID IN ({0})";
    #endregion

    public IEnumerable<SkuModel> GetListOfSkusByVendorIDs(IEnumerable<int> vendorIDs)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        IEnumerable<SkuModel> listOfSkus = db.Query<SkuModel>(string.Format(GetSkusByVendorID, string.Join(", ", vendorIDs)));

        return listOfSkus;
      }
    }

    public IEnumerable<SkuModel> GetListOfSkusByVendorID(int vendorID)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        IEnumerable<SkuModel> listOfSkus = db.Query<SkuModel>(string.Format(GetSkusByVendorID, vendorID));

        return listOfSkus;
      }
    } 
  }
}
