using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Sql;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;

using PetaPoco;

namespace Concentrator.ui.Management.Controllers
{
  public class ContentPriceController : BaseController
  {
    public class ContentPriceModel
    {
      public Int32 ContentPriceRuleID
      {
        get;
        set;
      }

      public Int32? BrandID
      {
        get;
        set;
      }

      public String BrandName
      {
        get;
        set;
      }

      public Int32 ConnectorID
      {
        get;
        set;
      }

      public Int32 VendorID
      {
        get;
        set;
      }

      public Int32? ProductID
      {
        get;
        set;
      }

      public String ProductDescription
      {
        get;
        set;
      }

      public Int32? ProductGroupID
      {
        get;
        set;
      }

      public String ProductGroupName
      {
        get;
        set;
      }

      public String Margin
      {
        get;
        set;
      }

      public String ContentPriceLabel
      {
        get;
        set;
      }

      public Decimal? UnitPriceIncrease
      {
        get;
        set;
      }

      public Decimal? CostPriceIncrease
      {
        get;
        set;
      }

      public Int32? MinimumQuantity
      {
        get;
        set;
      }

      public Int32 ContentPriceRuleIndex
      {
        get;
        set;
      }

      public Int32 PriceRuleType
      {
        get;
        set;
      }

      public Decimal? FixedPrice
      {
        get;
        set;
      }

      public Int32? ContentPriceCalculationID
      {
        get;
        set;
      }

      public DateTime? FromDate
      {
        get;
        set;
      }

      public DateTime? ToDate
      {
        get;
        set;
      }

      public int? AttributeID { get; set; }

      public string AttributeName { get; set; }

      public string AttributeValue { get; set; }
    }

    [RequiresAuthentication(Functionalities.GetContentPrice)]
    public ActionResult GetList(ContentFilter filter)
    {
      var query = new QueryBuilder()
        .From("[dbo].[ContentPrice] AS [CP]")
        .Join(JoinType.OuterLeft, "[dbo].[Brand] AS [B]", "[CP].[BrandID] = [B].[BrandID]")
        .Join(JoinType.OuterLeft, "[dbo].[Product] AS [P]", "[CP].[ProductID] = [P].[ProductID]")
        .Join(JoinType.OuterLeft, "[dbo].[ProductAttributeMetaData] as [A]", "[CP].AttributeID = [A].AttributeID")
        .Join(JoinType.OuterLeft, "[dbo].[ProductGroupLanguage] AS [PGL]", "[CP].[ProductGroupID] = [PGL].[ProductGroupID]")
        .Join(JoinType.OuterLeft, "[dbo].[ProductAttributeName] AS [PAN]", "[A].[AttributeID] = [PAN].[AttributeID] AND" + string.Format("[PAN].LanguageID = {0}", Client.User.LanguageID))
        .Select("[CP].*")
        .Column("A.AttributeID")
        .Column("CASE WHEN [PAN].Name IS NULL THEN A.AttributeCode ELSE [PAN].Name END as AttributeName")
        .Column("CP.AttributeValue")
        .Column("[B].[Name] AS [BrandName]")
        .Column("[P].[VendorItemNumber] AS [ProductDescription]")
        .Column("[PGL].[Name] AS [ProductGroupName]")
        .OrderBy("[CP].[ContentPriceRuleIndex]");

      using (var database = new Database(Environments.Current.Connection, Database.MsSqlClientProvider))
      {
        return List(database.Query<ContentPriceModel>(query).AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.CreateContentPrice)]
    public ActionResult Create()
    {
      return Create<ContentPrice>();
    }

    [RequiresAuthentication(Functionalities.CreateContentPrice)]
    public ActionResult CreatePerProductGroup(int productGroupMappingID)
    {
      return Create<ContentPrice>((unit, price) => ((IContentPriceService)unit.Service<ContentPrice>()).CreateForProductGroupMapping(productGroupMappingID, price));
    }

    [RequiresAuthentication(Functionalities.GetContentPrice)]
    public ActionResult GetPriceRuleTypes()
    {
      return Json(new
      {
        results = Enum
          .GetValues(typeof(PriceRuleType))
          .Cast<PriceRuleType>()
          .Select(value => new
          {
            ID = (Int32)value,
            Name = value.ToString()
          })
          .ToArray()
      });
    }

    [RequiresAuthentication(Functionalities.DeleteContentPrice)]
    public ActionResult Delete(int ID)
    {
      return Delete<ContentPrice>(c => c.ContentPriceRuleID == ID);
    }

    [RequiresAuthentication(Functionalities.GetContentPrice)]
    public ActionResult GetProductGroupList(int productGroupMappingID)
    {
      return List(unit =>
      {
        var pgID = unit.Service<ProductGroupMapping>().Get(m => m.ProductGroupMappingID == productGroupMappingID).ProductGroupID;

        return (
          from c in unit.Service<ContentPrice>().GetAll()
          where c.ProductGroupID.HasValue && c.ProductGroupID == pgID
          let productGroupName = c.ProductGroup != null ?
            c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
            c.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
          let productName = c.Product != null
            ?
            c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
            c.Product.ProductDescriptions.FirstOrDefault() : null
          select new
          {
            c.ContentPriceRuleID,
            c.VendorID,
            ProductDescription = productName != null
              ? (productName.ProductName ?? productName.ShortContentDescription)
              : c.Product != null
                ? c.Product.VendorItemNumber
                : null,
            c.UnitPriceIncrease,
            c.ProductID,
            c.ConnectorID,
            c.ProductGroupID,
            c.BrandID,
            ProductGroupName = productGroupName != null ? productGroupName.Name : null,
            BrandName = c.Brand != null ? c.Brand.Name : null,
            c.Margin,
            c.MinimumQuantity,
            c.CostPriceIncrease,
            c.ContentPriceRuleIndex,
            c.PriceRuleType,
            c.ContentPriceCalculationID,
            c.FixedPrice,
            ContentPriceCalculation = c.ContentPriceCalculation == null
            ? null : c.ContentPriceCalculation.Name
          }).AsQueryable();
      });
    }

    [RequiresAuthentication(Functionalities.GetContentPrice)]
    public ActionResult GetPriceCalculationList(int contentPriceCalculationID)
    {
      return List(unit => (from l in unit.Service<ContentPriceCalculation>().
                            GetAll(c => c.ContentPriceCalculationID == contentPriceCalculationID).ToList()
                           select new
                           {
                             l.ContentPriceCalculationID,
                             l.Name,
                             l.Calculation,
                             l.CreatedBy,
                             CreationTime = l.CreationTime.ToLocalTime(),
                             LastModificationTime = l.LastModificationTime.ToNullOrLocal(),
                             l.LastModifiedBy
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetContentPrice)]
    public ActionResult CalculatePrice(int productID, string formula, int? ConnectorID)
    {
      if (!ConnectorID.HasValue) ConnectorID = Client.User.ConnectorID;

      using (var unit = GetUnitOfWork())
      {
        try
        {
          var price = ((IContentPriceService)unit.Service<ContentPrice>()).CalculatePrice(productID, formula, ConnectorID);

          return Json(new { price = price.CostPrice, calculatedPrice = price.CalculatePrice, success = true });
        }
        catch (Exception e)
        {
          return Failure("Something went wrong while calculating the new price", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.UpdateContentPrice)]
    public ActionResult Update(int ID)
    {
      return Update<ContentPrice>(c => c.ContentPriceRuleID == ID);
    }

    [RequiresAuthentication(Functionalities.GetContentPrice)]
    public ActionResult GetPriceLedger(int vendorAssortmentID)
    {
      return List(unit => unit
        .Service<ContentLedger>()
        .GetAll()
        .Where(contentLedger => contentLedger.VendorAssortmentID == vendorAssortmentID)
        .Select(contentLedger => new
        {
          contentLedger.LedgerID,
          contentLedger.LedgerDate,
          contentLedger.UnitPrice,
          contentLedger.CostPrice,
          contentLedger.TaxRate,
          contentLedger.Remark
        }));
    }
  }
}
