using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Web.Script.Serialization;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.ui.Management.Models;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Drawing;
using System.IO;
using System.Configuration;
using Concentrator.Objects.Services.DTO;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Connectors;
using System.Dynamic;
using Concentrator.ui.Management.Extensions;

namespace Concentrator.ui.Management.Controllers
{
	public class ProductGroupMappingController : BaseController
	{
		[RequiresAuthentication(Functionalities.GetProductGroupMapping)]
		public ActionResult GetByParent(int parentID, string ids)
		{
			return List(unit => from rp in ((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).GetProductGroupPerParent(parentID, ids)
													select new
													{
														rp.ProductGroupMappingID,
														rp.ProductGroupID,
														ProductGroup =
														rp.ProductGroup.ProductGroupLanguages.FirstOrDefault(
														c => c.LanguageID == Client.User.LanguageID).Name,
														rp.FlattenHierarchy,
														rp.FilterByParentGroup,
														rp.ParentProductGroupMappingID,
														rp.Lineage,
														rp.Score,
														Connector = rp.Connector.Name,
														rp.ConnectorID
													});
		}

		[RequiresAuthentication(Functionalities.GetProductGroupMapping)]
		public ActionResult GetList()
		{
			return List(unit => from rp in unit.Service<ProductGroupMapping>().GetAll()
													where !rp.ParentProductGroupMappingID.HasValue && !Client.User.ConnectorID.HasValue || (Client.User.ConnectorID.HasValue && rp.ConnectorID == Client.User.ConnectorID)
													let productGroupName = rp.ProductGroup != null ?
									rp.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
									rp.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
													select new
													{
														rp.ProductGroupMappingID,
														rp.ProductGroupID,
														ProductGroup = productGroupName != null ? productGroupName.Name : string.Empty,
														rp.FlattenHierarchy,
														rp.FilterByParentGroup,
														rp.Lineage,
														rp.ParentProductGroupMappingID,
														rp.Score,
														Connector = rp.Connector.Name,
														rp.ConnectorID,
														rp.CustomProductGroupLabel,
														rp.ProductGroupMappingPath
													});
		}

		[RequiresAuthentication(Functionalities.GetProductGroupMapping)]
		public ActionResult GetTranslations(int ProductGroupMappingID, string ConnectorID)
		{
			return List(unit => (from l in unit.Service<Language>().GetAll()
													 join p in unit.Service<ProductGroupMappingDescription>().GetAll().Where(c => ProductGroupMappingID == c.ProductGroupMappingID) on l.LanguageID equals p.LanguageID into temp
													 from tr in temp.DefaultIfEmpty()
													 select new
													 {
														 l.LanguageID,
														 Language = l.Name,
														 tr.Description,
														 ProductGroupMappingID = (tr == null ? 0 : tr.ProductGroupMappingID)
													 }));
		}

		[ValidateInput(false)]
		[RequiresAuthentication(Functionalities.Default)]
		public ActionResult SetTranslation(int _LanguageID, int ProductGroupMappingID, string Description)
		{
			if (string.IsNullOrEmpty(Description))
			{
				try
				{
					using (var unit = GetUnitOfWork())
					{
						unit.Service<ProductGroupMappingDescription>().Delete(c => c.ProductGroupMappingID == ProductGroupMappingID && c.LanguageID == _LanguageID);

						unit.Save();
						return Success("Update translation successfully");
					}
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
			else
			{
				try
				{
					using (var unit = GetUnitOfWork())
					{
						var nameG = unit.Service<ProductGroupMappingDescription>().Get(c => c.ProductGroupMappingID == ProductGroupMappingID && c.LanguageID == _LanguageID);
						if (nameG == null)
						{
							nameG = new ProductGroupMappingDescription();
							nameG.ProductGroupMappingID = ProductGroupMappingID;
							nameG.LanguageID = _LanguageID;
							nameG.Description = Description;
							unit.Service<ProductGroupMappingDescription>().Create(nameG);
						}
						else
						{
							nameG.Description = Description;
						}
						unit.Save();
						return Success("Update translation successfully");
					}
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
		}

		[RequiresAuthentication(Functionalities.GetProductGroupMapping)]
		public ActionResult GetTreeView(int ProductGroupMappingID, string ConnectorID)
		{
			int? conID = null;

			if (Client.User.ConnectorID.HasValue)
			{
				conID = Client.User.ConnectorID.Value;
			}
			else
			{
				conID = int.Parse(ConnectorID);
			}

			conID.ThrowIf(c => !c.HasValue, "A connector must be specified");
			var unit = GetUnitOfWork();

			var q = (from i in unit.Service<ProductGroupMapping>().GetAll(x => x.ConnectorID == conID.Value) select i);

			//is it the root 
			if (ProductGroupMappingID == -1)
			{
				q = q.Where(c => c.ParentProductGroupMappingID == null);
			}
			else
			{
				q = q.Where(c => c.ParentProductGroupMappingID == ProductGroupMappingID);
			}

			var result = (from p in q.ToList()
										let productGroupName = p.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
										p.ProductGroup.ProductGroupLanguages.FirstOrDefault()
										let labelObject = p.ProductGroupMappingCustomLabels.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID)
										let label = labelObject != null ? labelObject.CustomLabel : string.Empty
										select new
										{
											p.ProductGroupMappingID,
											p.ProductGroupID,
											score = p.Score,
											text = string.IsNullOrEmpty(label) ? (!string.IsNullOrEmpty(p.CustomProductGroupLabel) ? p.CustomProductGroupLabel + "(C)" : productGroupName.Name) : label,
											leaf = unit.Service<ProductGroupMapping>().Get(c => c.ParentProductGroupMappingID == p.ProductGroupMappingID) == null,
											p.ConnectorID
										}).OrderByDescending(c => c.score).ToList();

			return Json(result);
		}

		[RequiresAuthentication(Functionalities.CreateProductGroupMapping)]
		public ActionResult Create(int? LayoutID)
		{
			return Create<ProductGroupMapping>(isMultipartRequest: true, onCreatingAction: (unit, pmg) =>
			{
				if (LayoutID.HasValue) pmg.MagentoPageLayoutID = LayoutID.Value;

				var m = new MagentoProductGroupSetting();
				SetSpecialPropertyValues<MagentoProductGroupSetting>(m);


				if ((m.ShowInMenu.HasValue && m.ShowInMenu.Value) || (m.IsAnchor.HasValue & m.IsAnchor.Value) || (m.DisabledMenu.HasValue && m.DisabledMenu.Value))
				{
					m.ProductGroupMapping = pmg;
					unit.Service<MagentoProductGroupSetting>().Create(m);
				}

				if (pmg.ConnectorID == 0 && Client.User.ConnectorID.HasValue)
					pmg.ConnectorID = Client.User.ConnectorID.Value;

				foreach (string f in Request.Files)
				{
					var file = Request.Files.Get(f);
					if (!string.IsNullOrEmpty(file.FileName))
					{
						string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
						string internalPath = ConfigurationManager.AppSettings["FTPProductGroupMappingMediaPath"];

						string dirPath = Path.Combine(externalPath, internalPath);

						if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
						string path = Path.Combine(dirPath, file.FileName);


						if (file.FileName != String.Empty)
						{
							file.SaveAs(path);
						}

						if (f == "ProductGroupMappingPath") pmg.ProductGroupMappingPath = file.FileName;
						else pmg.MappingThumbnailImagePath = file.FileName;
					}
				}



			});
		}

		private void DeleteHierarchy(int parent, IServiceUnitOfWork unit)
		{
			var child = unit.Service<ProductGroupMapping>().GetAll(x => x.ParentProductGroupMappingID.HasValue && x.ParentProductGroupMappingID.Value == parent).ToList();

			if (child.Count < 1)
				((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).Delete(parent);
			else
			{
				child.ForEach(x =>
				{
					DeleteHierarchy(x.ProductGroupMappingID, unit);
				});
			}
		}

		[RequiresAuthentication(Functionalities.DeleteProductGroupMapping)]
		public ActionResult Delete(int id)
		{
			using (var unit = GetUnitOfWork())
			{
				try
				{
					if (id < 0 && Client.User.ConnectorID.HasValue)
						((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).DeleteWholeConnectorMapping(Client.User.ConnectorID.Value);
					else
					{
						DeleteHierarchy(id, unit);
						unit.Save();

						var list = unit.Service<ProductGroupMapping>().GetAll(x => x.ProductGroupMappingID == id).Select(x => x.ProductGroupMappingID).ToList();

						foreach (var i in list)
						{
							((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).Delete(i);
						}

						unit.Save();
					}
					unit.Save();

					return Success("Successfully delete product group mapping");
				}
				catch (Exception ex)
				{
					return Failure("Failed to delete product group mapping: " + ex.Message);
				}
			}
		}

		[RequiresAuthentication(Functionalities.CopyProductGroupMapping)]
		public ActionResult Copy(int sourceConnectorID, int destinationConnectorID)
		{
			if (sourceConnectorID == destinationConnectorID)
				return Failure("Destination cannot be the same as Source");

			try
			{
				using (var unit = GetUnitOfWork())
				{
					((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).CopyProductGroupMapping(sourceConnectorID, destinationConnectorID);

					unit.Save();

					return Success("Product group mappings have been successfully copied");
				}
			}
			catch (Exception e)
			{
				return Failure("Something went wrong: " + e.Message);
			}
		}

		[RequiresAuthentication(Functionalities.CreateProductGroupMapping)]
		public ActionResult GenerateBrandMapping(int connectorID, int Score)
		{
			try
			{
				using (var unit = GetUnitOfWork())
				{
					((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).GenerateBrandMapping(connectorID, Score);

					unit.Save();

					return Success("Generate brand mapping success");
				}
			}
			catch (Exception ex)
			{
				return Failure(String.Format("Generate brand mapping failed {0}", ex.Message));
			}
		}

		[RequiresAuthentication(Functionalities.UpdateProductGroupMapping)]
		public ActionResult Update(int ProductGroupMappingID, bool? ShowInMenu, bool? DisabledMenu, bool? IsAnchor, string Relation)
		{
			return Update<ProductGroupMapping>(

										c => c.ProductGroupMappingID == ProductGroupMappingID,
											action: (unit, mapping) =>
											{
												var ex = unit.Service<ProductGroupMapping>().Get(c => c.ProductGroupMappingID == ProductGroupMappingID);
												mapping.ProductGroupMappingPath = ex.ProductGroupMappingPath;
												mapping.MappingThumbnailImagePath = ex.MappingThumbnailImagePath;

												foreach (string f in Request.Files)
												{
													var file = Request.Files.Get(f);
													if (!string.IsNullOrEmpty(file.FileName))
													{
														string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
														string internalPath = ConfigurationManager.AppSettings["FTPProductGroupMappingMediaPath"];

														string dirPath = Path.Combine(externalPath, internalPath);

														if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
														string path = Path.Combine(dirPath, file.FileName);


														if (file.FileName != String.Empty)
														{
															file.SaveAs(path);
														}

														if (f == "ProductGroupMappingPath") mapping.ProductGroupMappingPath = file.FileName;
														else mapping.MappingThumbnailImagePath = file.FileName;
													}
												}


												mapping.FilterByParentGroup = Request.Params["FilterByParentGroup"] != null ? Request.Params["FilterByParentGroup"].Contains("on") ? true : false : false;
												mapping.FlattenHierarchy = Request.Params["FlattenHierarchy"] != null ? Request.Params["FlattenHierarchy"].Contains("on") ? true : false : false;

												var magentoProductGroupSetting = unit.Service<MagentoProductGroupSetting>().Get(x => x.ProductGroupmappingID == ProductGroupMappingID);

												if (magentoProductGroupSetting == null)
												{
													magentoProductGroupSetting = new MagentoProductGroupSetting();
													magentoProductGroupSetting.ProductGroupmappingID = ProductGroupMappingID;
													unit.Service<MagentoProductGroupSetting>().Create(magentoProductGroupSetting);
												}

												SetSpecialPropertyValues(magentoProductGroupSetting);

												if (magentoProductGroupSetting.ShowInMenu.HasValue && !magentoProductGroupSetting.ShowInMenu.Value)
													magentoProductGroupSetting.ShowInMenu = null;


												int? layoutID = Request.Params["LayoutID"] != null ? Request.Params["LayoutID"].ParseToInt() : null;
												if (layoutID.HasValue && layoutID.Value == -1)
												{
													layoutID = null;
												}
												mapping.MagentoPageLayoutID = layoutID;
												mapping.Relation = Relation;

												magentoProductGroupSetting.LastModifiedBy = Client.User.UserID;
												magentoProductGroupSetting.LastModificationTime = DateTime.Now;

                        //inactive groups for connectors

                        var userConnectors = GetUserConnectors();

                        foreach (var con in userConnectors)
                        {
                          var ProductGroupIsCurrentlyActiveForConnector = con.ProductGroupMappingsNotActive.FirstOrDefault(x => x.ProductGroupMappingID == ProductGroupMappingID) == null; //so it is not in de DB

                          var checkboxValue = Request.Params["ConnectorID_" + con.ConnectorID];

                          if (checkboxValue != null)//checked
                          {
                            if (!ProductGroupIsCurrentlyActiveForConnector)//it is in de DB, so remove it 
                              con.ProductGroupMappingsNotActive.Remove(ex);
                          }
                          else//not checked
                          {
                            if (ProductGroupIsCurrentlyActiveForConnector)//it is NOT in de DB, so add it (only store inactive groups in the DB)
                              con.ProductGroupMappingsNotActive.Add(ex);
                          }
                        }

											}, isMultipartRequest: true, properties: new string[] { "Score", "FilterByParent", "FlattenHierarchy", "CustomProductGroupLabel", "Relation" });
		}
    
		[RequiresAuthentication(Functionalities.GetProductGroupMapping)]
		public ActionResult Get(int ProductGroupMappingID)
		{
			using (var unit = GetUnitOfWork())
			{
				var mapp = unit.Service<ProductGroupMapping>().Get(c => c.ProductGroupMappingID == ProductGroupMappingID);
				var productGroupName = mapp.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
									mapp.ProductGroup.ProductGroupLanguages.FirstOrDefault() ?? null;
				var magentoConnectorSetting = unit.Service<MagentoProductGroupSetting>().Get(x => x.ProductGroupmappingID == ProductGroupMappingID) != null ?
					unit.Service<MagentoProductGroupSetting>().Get(x => x.ProductGroupmappingID == ProductGroupMappingID) : null;

        List<ProductGroupMappingConnectorNotActiveModel> productGroupMappingConnectors = new List<ProductGroupMappingConnectorNotActiveModel>();
        
        var userConnectors = GetUserConnectors();

        foreach (var con in userConnectors)
        {
          productGroupMappingConnectors.Add(new ProductGroupMappingConnectorNotActiveModel()
          {
            ConnectorID = con.ConnectorID,
            IsActiveForGroup = true
          });
        
        }


        dynamic dataExpando = new ExpandoObject();

        var data = dataExpando as IDictionary<string, object>;

        data["ProductGroupMappingID"] = mapp.ProductGroupMappingID;
        data["ProductGroupID"] = mapp.ProductGroupID;
        data["ProductGroupName"] = productGroupName != null ? productGroupName.Name : string.Empty;
        data["FlattenHierarchy"] = mapp.FlattenHierarchy;
        data["FilterByParentGroup"] = mapp.FilterByParentGroup;
        data["Score"] = mapp.Score.HasValue ? mapp.Score.Value : mapp.ProductGroup.Score;
        data["CustomProductGroupLabel"] = mapp.CustomProductGroupLabel;
        data["ConnectorName"] = mapp.Connector.Name;
        data["ProductGroupMappingLabel"] = mapp.ProductGroupMappingLabel;
        data["ProductGroupMappingPath"] = mapp.ProductGroupMappingID;
        data["MappingThumbnailImagePath"] = mapp.MappingThumbnailImagePath;
        data["ShowInMenu"] = magentoConnectorSetting != null ? magentoConnectorSetting.ShowInMenu ?? false : false;
        data["DisabledMenu"] = magentoConnectorSetting != null ? magentoConnectorSetting.DisabledMenu ?? false : false;
        data["IsAnchor"] = magentoConnectorSetting != null ? magentoConnectorSetting.IsAnchor ?? false : false;
        data["LayoutID"] = mapp.MagentoPageLayoutID;
        data["Relation"] = mapp.Relation;

        foreach (var conn in productGroupMappingConnectors)
        {
          var val = "ConnectorID_" + conn.ConnectorID;
          data[val] = conn.IsActiveForGroup.ToString().ToLower();
        }

        var json = MyDictionaryToJson(data);

        var dataToReturn = "{\"success\":true, \"data\":" + json+ "}";

        return Content(dataToReturn, "application/json");
			}
		}

    string MyDictionaryToJson(IDictionary<string, object> dict)
    {
      var entries = dict.Select(d => string.Format("\"{0}\": \"{1}\"", d.Key, string.Join(",", d.Value)));

      return "{" + string.Join(",", entries) + "}";
    }

		public ActionResult GetBrandProductHierarchy(int? brandID, int? productID, int? node, int productGroupMappingID, int? connectorID)
		{
			if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;
			connectorID.ThrowIfNull("Connector must be specified");

			using (var unit = GetUnitOfWork())
			{
				List<PriceHierarchyNode> nodes = new List<PriceHierarchyNode>();
				List<ProductDto> products = new List<ProductDto>();
				if (node.HasValue && node == -1)
				{
					//load all the brands under the productGroupMapping
					products = ((IProductService)unit.Service<Product>()).GetByProductGroupMapping(productGroupMappingID, connectorID: connectorID);

					foreach (var p in products)
					{
						if (nodes.FirstOrDefault(c => c.BrandID == p.Brand.BrandID) == null) nodes.Add(new PriceHierarchyNode
						{
							text = p.Brand.Name,
							leaf = false,
							BrandID = p.Brand.BrandID
						});
					}
				}
				else if (brandID.HasValue)
				{
					products = ((IProductService)unit.Service<Product>()).GetByProductGroupMapping(productGroupMappingID, connectorID: connectorID).Where(c => c.Brand.BrandID == brandID).ToList();
				}

				return Json(nodes);
			}
		}

		public ActionResult SearchTree(string query)
		{
			using (var unit = GetUnitOfWork())
			{

				var results = (from m in unit.Service<ProductGroupMapping>().GetAll()
											 join cpg in unit.Service<ContentProductGroup>().GetAll() on m.ProductGroupMappingID equals cpg.ProductGroupMappingID
											 where
											 m.ConnectorID == Client.User.ConnectorID &&
											 (m.CustomProductGroupLabel.Contains(query)
											 ||
											 m.ProductGroup.ProductGroupLanguages.Where(c => c.LanguageID == Client.User.LanguageID).Any(c => c.Name.Contains(query))
											 ||
											 cpg.Product.VendorItemNumber.Contains(query)
											 ||
											 cpg.Product.VendorAssortments.Any(x => x.ShortDescription.Contains(query))
											 ||
											 cpg.Product.VendorAssortments.Any(x => x.CustomItemNumber.Contains(query)))
											 select
											 m.Lineage).ToList();

				List<TreeNode> re = new List<TreeNode>();

				foreach (var mapping in results)
				{
					//build the tree
					var lin = mapping.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
					re.Add(GetNodes(new TreeNode(), lin, int.Parse(lin[0]), unit));
				}

				return Json(re.OrderBy(c => c.leaf));
			}
		}

		private TreeNode GetNodes(TreeNode level, List<string> levels, int currentLevel, IServiceUnitOfWork unit)
		{
			bool node = levels.IndexOf(currentLevel.ToString()) < levels.Count() - 1;

			var mapping = unit.Service<ProductGroupMapping>().Get(c => c.ProductGroupMappingID == currentLevel);
			var productGroupName = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID) ?? mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault();

			level.text = mapping.CustomProductGroupLabel != "" ? mapping.CustomProductGroupLabel + "(C)" : productGroupName.Name;
			level.leaf = !node;
			level.ConnectorID = mapping.ConnectorID;
			level.ProductGroupMappingID = currentLevel;

			if (node)
			{
				TreeNode tn = new TreeNode();
				level.children = new List<TreeNode>();
				level.children.Add(GetNodes(tn, levels, int.Parse(levels[levels.IndexOf(currentLevel.ToString()) + 1]), unit));
			}

			return level;
		}

		[RequiresAuthentication(Functionalities.UpdateProductGroupMapping)]
		public ActionResult Publish(int? sourceConnectorID, int destinationConnectorID, int? productGroupMappingID, Publishables publishables)
		{
			if (!sourceConnectorID.HasValue) sourceConnectorID = Client.User.ConnectorID;

			sourceConnectorID.ThrowIf(c => !c.HasValue);

			HttpContext.Server.ScriptTimeout = 60 * 10;
			try
			{
				if (sourceConnectorID < 1) //root leveL
					sourceConnectorID = Client.User.ConnectorID.Value;

				if (productGroupMappingID < 0) productGroupMappingID = null;

				using (var unit = GetUnitOfWork())
				{
					((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).Publish(sourceConnectorID.Value, destinationConnectorID, productGroupMappingID, null, publishables.Attributes, publishables.ContentPrices, publishables.ContentProducts, publishables.ContentVendorSettings, publishables.ConnectorPublications, publishables.ConnectorProductStatuses, publishables.PreferredContentSettings);

				}
				return Success("Connector published");
			}
			catch (Exception e)
			{
				return Failure("Publishing failed", e);
			}
		}

		[RequiresAuthentication(Functionalities.GetProductGroupMapping)]
		public ActionResult GetCustomLabels(int ProductGroupMappingID)
		{
      using (var unit = GetUnitOfWork())
      {
        List<CustomLabelModel> labelsToReturn = new System.Collections.Generic.List<CustomLabelModel>();
        var allLabelsForMapping = unit.Service<ProductGroupMappingCustomLabel>().GetAll(x => x.ProductGroupMappingID == ProductGroupMappingID);
 
        //first: add null connectors
        var languages = unit.Service<Language>().GetAll();

        foreach (var lan in languages)
        {
          var labelForNullConnector = allLabelsForMapping.FirstOrDefault(x => x.LanguageID == lan.LanguageID && x.ConnectorID == null);

          labelsToReturn.Add(new CustomLabelModel(){
            LanguageID = lan.LanguageID,
            Language = lan.Name,
            CustomLabel = labelForNullConnector != null ? labelForNullConnector.CustomLabel : "",
            ProductGroupMappingID = labelForNullConnector != null ? ProductGroupMappingID : 0,
            ConnectorID = -1,
            ConnectorName = "Default Connector",
          });
        }

        var userConnectors = GetUserConnectors();

        //second: add connector languages
        foreach (var userCon in userConnectors)
        {
          foreach (var connectoLanguage in userCon.ConnectorLanguages)
          {
            var labelForUserConnector = allLabelsForMapping.FirstOrDefault(x => x.LanguageID == connectoLanguage.LanguageID && x.ConnectorID == userCon.ConnectorID);

            labelsToReturn.Add(new CustomLabelModel()
            {
              LanguageID = connectoLanguage.LanguageID,
              Language = connectoLanguage.Language.Name,
              CustomLabel = labelForUserConnector != null ? labelForUserConnector.CustomLabel : "",
              ProductGroupMappingID = labelForUserConnector != null ? ProductGroupMappingID : 0,
              ConnectorID = userCon.ConnectorID,
              ConnectorName = userCon.Name,
            });
          }
        }
			  
        return List((from l in labelsToReturn select l).AsQueryable());
      }
		}

		[ValidateInput(false)]
		[RequiresAuthentication(Functionalities.Default)]
		public ActionResult SetCustomLabel(int _LanguageID, int _ConnectorID, int ProductGroupMappingID, string CustomLabel)
		{
      bool isNullConnector = _ConnectorID == -1;

			if (string.IsNullOrEmpty(CustomLabel))
			{
				try
				{
					using (var unit = GetUnitOfWork())
					{
            if (isNullConnector)
              unit.Service<ProductGroupMappingCustomLabel>().Delete(c => c.ProductGroupMappingID == ProductGroupMappingID && c.LanguageID == _LanguageID
                && c.ConnectorID == null);
            else
              unit.Service<ProductGroupMappingCustomLabel>().Delete(c => c.ProductGroupMappingID == ProductGroupMappingID && c.LanguageID == _LanguageID
                && c.ConnectorID == _ConnectorID);

						unit.Save();
						return Success("Update custom label successfully");
					}
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
			else
			{
				try
				{
					using (var unit = GetUnitOfWork())
					{

            var nameG = new ProductGroupMappingCustomLabel();

            if (isNullConnector)
              nameG = unit.Service<ProductGroupMappingCustomLabel>().Get(c => c.ProductGroupMappingID == ProductGroupMappingID && c.LanguageID == _LanguageID 
                && c.ConnectorID == null);
            else
              nameG = unit.Service<ProductGroupMappingCustomLabel>().Get(c => c.ProductGroupMappingID == ProductGroupMappingID && c.LanguageID == _LanguageID
                && c.ConnectorID == _ConnectorID);
 
						if (nameG == null)
						{
							nameG = new ProductGroupMappingCustomLabel();
							nameG.ProductGroupMappingID = ProductGroupMappingID;
							nameG.LanguageID = _LanguageID;
							nameG.CustomLabel = CustomLabel;
              nameG.ConnectorID = isNullConnector ? (int?) null : _ConnectorID;
							unit.Service<ProductGroupMappingCustomLabel>().Create(nameG);
						}
						else
						{
							nameG.CustomLabel = CustomLabel;
						}
						unit.Save();
						return Success("Update translation successfully");
					}
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
		}

    private List<Connector> GetUserConnectors()
    {
      using (var unit = GetUnitOfWork())
      {
        var userConnectorID = Client.User.ConnectorID;
        var userConnectors = new List<Connector>();

        if (userConnectorID.HasValue)
        {
          userConnectors = (from c in unit.Service<Connector>().GetAll(x =>
             (x.ParentConnectorID == userConnectorID || x.ConnectorID == userConnectorID)
             &&
             !x.Name.Contains("Wehkamp"))
                            select c
                    ).ToList();
        }
        return userConnectors;
      }

    }

    [RequiresAuthentication(Functionalities.GetProductGroupMapping)]
    public ActionResult GetConnectorsForLoggedInUser()
    {
      using (var unit = GetUnitOfWork())
      {
        return Json(new
        {
          connectors = (from c in GetUserConnectors()
                        select new
                        {
                          ConnectorID = c.ConnectorID,
                          ConnectorName = c.Name
                        }
                    )
        });
      }
    }

	}
}
