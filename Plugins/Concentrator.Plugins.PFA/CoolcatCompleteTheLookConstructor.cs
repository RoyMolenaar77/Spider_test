using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services;
using Concentrator.Web.CustomerSpecific.Coolcat.Controllers;
using Concentrator.Plugins.PFA.Configuration;

namespace Concentrator.Plugins.PFA
{
	public class AttributeRule
	{
		public ProductAttributeMetaData Attribute { get; set; }
		public bool EqualsRule { get; set; }
	}

	public class CoolcatCompleteTheLookConstructor : ConcentratorPlugin
	{
		private readonly IDictionary<string, string> _pavItemsDict = new Dictionary<string, string>();
		private readonly IDictionary<string, string> _productGroupRelatedProductGroups = new Dictionary<string, string>();

		//public int vendorID;
		private List<int> _vendorIDs;


		public override string Name
		{
			get { return "Concentrator Related Products Maker Plugin"; }
		}

		protected override void Process()
		{
			var config = GetConfiguration();

			using (var unit = GetUnitOfWork())
			{
				//vendorID = unit.Scope.Repository<Vendor>().GetSingle(x => x.Name == "PFA CC").VendorID;
				_vendorIDs = new List<int>() { 1, 13, 14 };

				foreach (var vendorID in _vendorIDs)
				{
					CompleteTheLook(vendorID, unit, 9);
				}
			}
		}





		private void CompleteTheLook(int vendorID, IUnitOfWork unit, int relatedProductTypeID)
		{
			var relatedProductRepo = unit.Scope.Repository<RelatedProduct>();
			var productsToProcess =
				unit.Scope.Repository<Product>()
						.GetAll(
							c =>
							c.VendorAssortments.Any(l => l.VendorID == vendorID) && c.IsConfigurable &&
							!c.VendorItemNumber.StartsWith("8") && c.ParentProductID == null)
						.ToList();

			//Set all items (CreatedBy=1, RelatedProductTypeID=9, Index=0, VendorID=vendorID) as MarkedForDeletion
			unit.ExecuteStoreCommand(string.Format("UPDATE RelatedProduct SET [MarkedForDeletion] = 1 WHERE [CreatedBy] = 1 AND [RelatedProductTypeID] = 9 AND [VendorID] = {0}", vendorID));


			InitProductAttributeValueCache(unit);
			InitProductGroupRelatedProductGroupsCache();

			var itemsToGroup = (from pp in productsToProcess
													let valueSeason = GetProductAttributeValue(pp.ProductID, PfaCoolcatConfiguration.Current.SeasonAttributeID)
													let inputCode = GetProductAttributeValue(pp.ProductID, PfaCoolcatConfiguration.Current.InputCodeAttributeID)
													let targetGroup = GetProductAttributeValue(pp.ProductID, PfaCoolcatConfiguration.Current.TargetGroupAttributeID)
													let productGroup = GetProductAttributeValue(pp.ProductID, 71)
													where
														(!string.IsNullOrEmpty(valueSeason) && !string.IsNullOrEmpty(inputCode) &&
														 !string.IsNullOrEmpty(targetGroup) && !string.IsNullOrEmpty(productGroup))
													select new
														{

															ProductID = pp.ProductID,
															Season = valueSeason,
															InputCode = inputCode,
															TargetGroup = targetGroup,
															ProductGroup = productGroup
														}).ToList();

			foreach (var productGroup in itemsToGroup.GroupBy(c => new { c.Season, c.InputCode, c.TargetGroup }))
			{
				var results = (from m in productGroup
											 from c in productGroup
											 where c.ProductGroup != m.ProductGroup && c.ProductID != m.ProductID
											 select new { c.ProductID, RelatedProductID = m.ProductID, c.ProductGroup }
											).ToList();

				foreach (var result in results)
				{
					var relatedProduct =
						relatedProductRepo.GetSingle(
							c =>
							c.ProductID == result.ProductID && c.RelatedProductTypeID == relatedProductTypeID &&
							c.RelatedProductID == result.RelatedProductID && c.VendorID == vendorID);


					if (relatedProduct == null)
					{

						//only add relations defined in 1.3.9
						if (ProductGroupIsRelatedToProductGroup(result.ProductGroup, GetProductAttributeValue(result.RelatedProductID, 71)))
						{
							relatedProduct = new RelatedProduct()
								{

									ProductID = result.ProductID,
									RelatedProductID = result.RelatedProductID,
									RelatedProductTypeID = relatedProductTypeID,
									VendorID = vendorID,
									Index = GetIndex(result.ProductGroup, GetProductAttributeValue(result.RelatedProductID, 71)),
									IsActive = true
								};

							relatedProductRepo.Add(relatedProduct);
						}
					}
					else
					{
						if (ProductGroupIsRelatedToProductGroup(result.ProductGroup, GetProductAttributeValue(relatedProduct.RelatedProductID, 71)))
						{
							//Reset the MarkedForDeletion bit
							unit.ExecuteStoreCommand(string.Format("UPDATE RelatedProduct SET [MarkedForDeletion] = 0 WHERE [CreatedBy] = 1 AND [RelatedProductTypeID] = 9 AND [VendorID] = {0} AND ProductID = {1} AND RelatedProductID = {2}", vendorID, result.ProductID, result.RelatedProductID));

							//Recalculate the Index value (only if index <= 100)
							var index = GetIndex(result.ProductGroup, GetProductAttributeValue(relatedProduct.RelatedProductID, 71));
							unit.ExecuteStoreCommand(string.Format("UPDATE RelatedProduct SET [Index] = {0} WHERE [CreatedBy] = 1 AND [RelatedProductTypeID] = 9 AND [VendorID] = {1} AND ProductID = {2} AND RelatedProductID = {3} AND [Index] <= 100", index, vendorID, result.ProductID, result.RelatedProductID));
						}
					}
				}
			}


			//Delete relations that are not in the current list; have a related type 9, have a created by '1' user (concentrator process), and are from the vendor we are currently processing
			unit.ExecuteStoreCommand(string.Format("DELETE FROM RelatedProduct WHERE [MarkedForDeletion] = 1 AND [CreatedBy] = 1 AND [RelatedProductTypeID] = 9 AND [VendorID] = {0}", vendorID));


			//Save the added relations
			unit.Save();
		}



		private void InitProductAttributeValueCache(IUnitOfWork unit)
		{
			var pavItems = unit.Scope.Repository<ProductAttributeValue>().GetAll();

			foreach (var item in pavItems)
			{
				if (_pavItemsDict.ContainsKey(string.Format("{0}_{1}", item.ProductID, item.AttributeID)))
				{
					_pavItemsDict[string.Format("{0}_{1}", item.ProductID, item.AttributeID)] = item.Value;
				}
				else
				{
					_pavItemsDict.Add(string.Format("{0}_{1}", item.ProductID, item.AttributeID), item.Value);
				}
			}
		}

		private string GetProductAttributeValue(int productID, int attributeID)
		{
			return _pavItemsDict.ContainsKey(string.Format("{0}_{1}", productID, attributeID))
							 ? _pavItemsDict[string.Format("{0}_{1}", productID, attributeID)]
							 : string.Empty;
		}


		private void InitProductGroupRelatedProductGroupsCache()
		{
			var list = new ProductGroupRelatedProductGroupController().GetAllProductGroupRelatedProductGroups();

			foreach (var item in list.Where(item => !_productGroupRelatedProductGroups.ContainsKey(item.ProductGroup)))
			{
				_productGroupRelatedProductGroups.Add(item.ProductGroup, item.RelatedProductGroups);
			}
		}

		private bool ProductGroupIsRelatedToProductGroup(string productGroup, string relatedProductGroup)
		{
			if (_productGroupRelatedProductGroups.ContainsKey(productGroup.Substring(1, 1)))
			{
				var item = _productGroupRelatedProductGroups[productGroup.Substring(1, 1)].ToUpperInvariant();

				if (!string.IsNullOrEmpty(item) && item.IndexOf(relatedProductGroup.Substring(1, 1).ToUpperInvariant(), StringComparison.Ordinal) != -1)
				{
					return true;
				}
			}

			return false;
		}

		private int GetIndex(string productGroup, string relatedProductGroup)
		{
			if (_productGroupRelatedProductGroups.ContainsKey(productGroup.Substring(1, 1)))
			{
				var item = _productGroupRelatedProductGroups[productGroup.Substring(1, 1)].ToUpperInvariant();

				if (!string.IsNullOrEmpty(item) && item.IndexOf(relatedProductGroup.Substring(1, 1).ToUpperInvariant(), StringComparison.Ordinal) != -1)
				{
					item = Regex.Replace(item, @"[^A-Z]+", "");
					var productGroupPosition = 100 - item.IndexOf(relatedProductGroup.Substring(1, 1).ToUpperInvariant(), StringComparison.Ordinal);

					return productGroupPosition;
				}
			}

			return 0;
		}
	}
}
