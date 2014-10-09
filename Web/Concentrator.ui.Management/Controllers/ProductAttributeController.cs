using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;
using System.IO;
using System.Configuration;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Localization;
using Concentrator.Web.Shared.Controllers;
using System.Drawing.Imaging;
using System.Drawing;
using Concentrator.Objects.Images;
using Concentrator.ui.Management.Models;

namespace Concentrator.ui.Management.Controllers
{
	public class ProductAttributeController : BaseController
	{
		[RequiresAuthentication(Functionalities.GetProductAttribute)]
		public ActionResult GetListForProduct(int productID, bool isSearched = false)
		{
			if (isSearched)
			{
				return List(unit => (
          from pv in unit.Service<ProductAttributeValue>().GetAll(c => c.ProductAttributeMetaData.AttributeCode == null || c.ProductAttributeMetaData.AttributeCode.ToLower() != "size") //only show color attributes
					let attribute = pv.ProductAttributeMetaData
					let productGroupName = pv.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(l => l.LanguageID == Client.User.LanguageID) ?? pv.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault()
					let attributeName = attribute.ProductAttributeNames.FirstOrDefault(l => l.LanguageID == Client.User.LanguageID) ?? attribute.ProductAttributeNames.FirstOrDefault()
					where pv.ProductID == productID
					//&& pv.LanguageID == Client.User.LanguageID
					select new
					{
						pv.AttributeValueID,
						AttributeName = attributeName.Name,
						pv.AttributeID,
						pv.Value,
						pv.ProductAttributeMetaData.ProductAttributeGroupID,
						AttributeGroupName = productGroupName.Name,
						AttributeGroupIndex = attribute.ProductAttributeGroupMetaData.Index,
						attribute.FormatString,
						attribute.Sign,
						VendorName = attribute.Vendor.Name,
						attribute.VendorID,
						attribute.DataType,
						pv.ProductAttributeMetaData.IsSearchable,
						attribute.IsVisible,
						ProductID = productID,
						pv.LanguageID,
						pv.ProductAttributeMetaData.Index,
						attribute.Mandatory,
						attribute.ConfigurablePosition
					}).ToList().AsQueryable().Distinct());
			}
			else
			{
				return List(unit => (from pv in unit.Service<ProductAttributeValue>().GetAll()
														 let attribute = pv.ProductAttributeMetaData
														 let productGroupName = pv.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(l => l.LanguageID == Client.User.LanguageID) ?? pv.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault()
														 let attributeName = attribute.ProductAttributeNames.FirstOrDefault(l => l.LanguageID == Client.User.LanguageID) ?? attribute.ProductAttributeNames.FirstOrDefault()
														 where pv.ProductID == productID
														 //&& pv.LanguageID == Client.User.LanguageID
														 select new
														 {
															 pv.AttributeValueID,
															 AttributeName = attributeName.Name,
															 pv.AttributeID,
															 pv.Value,
															 pv.ProductAttributeMetaData.ProductAttributeGroupID,
															 AttributeGroupName = productGroupName.Name,
															 AttributeGroupIndex = attribute.ProductAttributeGroupMetaData.Index,
															 attribute.FormatString,
															 attribute.Sign,
															 VendorName = attribute.Vendor.Name,
															 attribute.VendorID,
															 attribute.DataType,
															 pv.ProductAttributeMetaData.IsSearchable,
															 attribute.IsVisible,
															 ProductID = productID,
															 pv.LanguageID,
															 pv.ProductAttributeMetaData.Index,
															 attribute.Mandatory
														 }).ToList().AsQueryable().Distinct());
			}
		}

		[RequiresAuthentication(Functionalities.GetProductAttribute)]
		public ActionResult GetList()
		{
			return List(unit => 
        from attribute in unit.Scope
          .Repository<ProductAttributeMetaData>()
          .Include(attribute => attribute.ProductAttributeNames)
          .Include(attribute => attribute.ProductAttributeGroupMetaData)
          .GetAll()
				let attributeName = attribute.ProductAttributeNames
          .Where(c => c.LanguageID == Client.User.LanguageID)
          .Select(c => c.Name)
          .FirstOrDefault() ?? attribute.AttributeCode
        let attributeGroup = attribute.ProductAttributeGroupMetaData
        let attributeGroupName = attributeGroup.ProductAttributeGroupNames
          .Where(c => c.LanguageID == Client.User.LanguageID)
          .Select(c => c.Name)
          .FirstOrDefault() ?? attributeGroup.GroupCode
        orderby attribute.Index ascending
				select new
				{
          attribute.AttributeID,
					attributeName,
					attribute.ProductAttributeGroupID,
					AttributeGroupName = attributeGroupName,
					attribute.FormatString,
					attribute.Sign,
          attribute.VendorID,
          attribute.NeedsUpdate,
					attribute.IsVisible,
					attribute.IsSearchable,
					attribute.Index,
					attribute.DataType,
					VendorName = attribute.Vendor.Name,
					attribute.AttributePath,
					attribute.Mandatory,
					attribute.DefaultValue,
					attribute.IsConfigurable,
					attribute.ConfigurablePosition
				});
		}

		[RequiresAuthentication(Functionalities.GetProductAttribute)]
		public ActionResult GetAttributeProductValues(int attributeID, int attributeValueGroupID)
		{
			return List(unit => from at in unit.Service<ProductAttributeValue>().GetAll()
													where at.AttributeValueGroupID == attributeValueGroupID
													select new
													{
														at.ProductID,
														Description = at.Product.ProductDescriptions.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).ShortSummaryDescription,
														at.Value
													});
		}

		[RequiresAuthentication(Functionalities.CreateProductAttribute)]
		public ActionResult Create(Dictionary<string, string> language, HttpPostedFileBase AttributePath)
		{
			return Create<ProductAttributeMetaData>((unit, metadata) =>
			{
				Stream str = null;
				string name = null;

				foreach (string fileName in Request.Files)
				{
					var fileInfo = Request.Files[fileName];

					str = fileInfo.InputStream;
					name = fileInfo.FileName;
				}

				((IProductService)unit.Service<Product>()).CreateProductAttribute(metadata, GetPostedLanguages(), str, name);

			}, isMultipartRequest: true);
		}

		[RequiresAuthentication(Functionalities.UpdateProductAttribute)]
		public ActionResult Update(int id, HttpPostedFileBase AttributePath, string FormatString)
		{
			return Update<ProductAttributeMetaData>(c => c.AttributeID == id, action: (unit, attribute) =>
			{
				attribute.FormatString = FormatString;

        var file = Request.Files.OfType<HttpPostedFileBase>().FirstOrDefault();

				if (file != null)
				{
					var internalPath = Path.Combine(ConfigurationManager.AppSettings["AttributeImageDirectory"], file.FileName);
					var path = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], internalPath);

					using (var stream = file.InputStream)
					{
						var buffer = new byte[stream.Length];

						stream.Read(buffer, 0, (int)stream.Length);

						System.IO.File.WriteAllBytes(path, buffer);
					}

					attribute.AttributePath = internalPath;
				}
			}, isMultipartRequest: true);
		}

		[RequiresAuthentication(Functionalities.DeleteProductAttribute)]
		public ActionResult Delete(int id)
		{
			return Delete<ProductAttributeMetaData>(c => c.AttributeID == id);
		}

		[RequiresAuthentication(Functionalities.GetProductAttribute)]
		public ActionResult GetImage(int id)
		{
			var productAttribute = GetObject<ProductAttributeMetaData>(c => c.AttributeID == id);

			return Json(new
			{
				success = true,
				data = new
				{
					AttributePath = productAttribute.AttributePath
				}
			});
		}

		[RequiresAuthentication(Functionalities.GetProductAttribute)]
		public ActionResult Search(string query)
		{
			query = query.IfNullOrEmpty("").ToLower();

			return Search(unit => from o in unit.Service<ProductAttributeMetaData>().Search(query)
														select new
														{
															AttributeName = o.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
															o.AttributeID
														});
		}

		[RequiresAuthentication(Functionalities.CreateProductAttribute)]
		public ActionResult CreateFromExcel(HttpPostedFileBase file, int productID, int connectorID)
		{
			return Success("Successfully imported");
		}

		[RequiresAuthentication(Functionalities.GetProductAttribute)]
		public ActionResult GetTranslations(int? attributeID)
		{
			return List(unit => (from l in unit.Service<Language>().GetAll()
													 join p in unit.Service<ProductAttributeName>().GetAll().Where(c => attributeID.HasValue ? attributeID == c.AttributeID : true) on l.LanguageID equals p.LanguageID into temp
													 from tr in temp.DefaultIfEmpty()
													 select new
													 {
														 l.LanguageID,
														 Language = l.Name,
														 tr.Name,
														 AttributeID = (tr == null ? 0 : tr.AttributeID)
													 }));
		}

		[RequiresAuthentication(Functionalities.UpdateProductAttribute)]
		public ActionResult SetTranslation(int _LanguageID, int _attributeID, string name)
		{
			using (var unit = GetUnitOfWork())
			{
				var service = ((IProductService)unit.Service<Product>());
				try
				{
					service.SetAttributeTranslations(_LanguageID, _attributeID, name);
					unit.Save();
					return Success("Translations set");
				}
				catch (Exception e)
				{
					return Failure("Something went wrong", e);
				}
			}

		}

		[RequiresAuthentication(Functionalities.Default)]
		public ActionResult GetNotification()
		{
			using (var unit = GetUnitOfWork())
			{
				int connectorID = Client.User.ConnectorID.Value;

				var countUngrouped = (from u in unit.Service<ProductAttributeValue>().GetAll()
															group u by u.Value into grouped
															let attr = grouped.FirstOrDefault()
															select attr.AttributeValueID).Count();

				return View("AttributesPortlet", new AttributesPortletModel
        { 
          UngroupedValues = countUngrouped 
        });
			}
		}

    private static readonly Func<ProductAttributeOption, Object> DefaultSelector = attributeOption => new
    {
      attributeOption.OptionID,
      attributeOption.AttributeOption
    };

    private static readonly IEnumerable<Object> EmptyStore = new Object[0];

    private ActionResult GetAttributeOptionStore(String settingKey, String query, Func<ProductAttributeOption, Object> selector = null)
    {
      var attributeID = (ConfigurationManager.AppSettings[settingKey] ?? String.Empty).ParseToInt();

      return attributeID.HasValue
        ? SimpleList(unit => unit
          .Service<ProductAttributeOption>()
          .GetAll(attributeOption => attributeOption.AttributeID == attributeID.Value)
          .Where(attributeOption => String.IsNullOrEmpty(query) || attributeOption.AttributeOption.Contains(query))
          .Select(selector ?? DefaultSelector)
          .AsQueryable())
        : SimpleList(EmptyStore);
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetMaterialStore(String query)
    {
      return GetAttributeOptionStore("MaterialAttributeID", query, attributeOption => new
      {
        MaterialOptionID = attributeOption.OptionID,
        MaterialAttributeOption = attributeOption.AttributeOption
      });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetPijpwijdteStore(string query)
    {
      return GetAttributeOptionStore("PijpwijdteAttributeID", query, attributeOption => new
      {
        PijpwijdteOptionID = attributeOption.OptionID,
        PijpwijdteAttributeOption = attributeOption.AttributeOption
      });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetKraagvormStore(string query)
    {
      return GetAttributeOptionStore("KraagvormAttributeID", query, attributeOption => new
      {
        KraagvormOptionID = attributeOption.OptionID,
        KraagvormAttributeOption = attributeOption.AttributeOption
      });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetDessinStore(string query)
    {
      return GetAttributeOptionStore("DessinAttributeID", query, attributeOption => new
      {
        DessinOptionID = attributeOption.OptionID,
        DessinAttributeOption = attributeOption.AttributeOption
      });
    }
	}
}
