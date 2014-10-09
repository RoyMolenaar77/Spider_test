using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.PFA
{
	public class ProcessProducts : ConcentratorPlugin
	{
		public override string Name
		{
			get { return "Process products into content product matches"; }
		}

		protected override void Process()
		{
			using (var unit = GetUnitOfWork())
			{

				log.Info("Finished the product attribute configuration");

				SyncContentProductMatches(1, unit);
				SyncContentProductMatches(2, unit);
			}
		}

		private void SyncContentProductMatches(int vendorID, IUnitOfWork unit)
		{
			var contentMatchRepo = unit.Scope.Repository<ContentProductMatch>();
			var confiProducts = unit.Scope.Repository<Product>().GetAll(c => c.IsConfigurable && c.VendorAssortments.Any(d => d.VendorID == vendorID));

			var storedRepoList = contentMatchRepo.GetAll().ToList();



			#region level 1 -- highest level

			foreach (var confiProduct in confiProducts)
			{
				var leadingProduct = contentMatchRepo.GetSingle(c => c.ProductID == confiProduct.ProductID && c.Index == 1);//get from DB

				int storeID = 1;
				if (storedRepoList.Count == 0)
					storeID = 1;
				else
					storeID = leadingProduct.Try(c => c.StoreID, storedRepoList.Max(c => c.StoreID) + 1);

				//all configured products - > 
				var relatedConfiguredProducts = confiProduct.RelatedProductsSource.Where(c => c.IsConfigured && c.VendorID == vendorID).Select(c => c.RelatedProductID);

				var desc = confiProduct.VendorAssortments.FirstOrDefault().Try(c => c.ShortDescription, string.Empty);

				if (leadingProduct == null)
				{
					leadingProduct = new ContentProductMatch()
					{
						StoreID = storeID,
						Index = 1,
						Description = desc,
						IsLeading = true,
						ProductID = confiProduct.ProductID
					};

					contentMatchRepo.Add(leadingProduct);
					storedRepoList.Add(leadingProduct);

				}


				foreach (var relatedConfiguredProduct in relatedConfiguredProducts)
				{


					var contentProductMatch = contentMatchRepo.GetSingle(c => c.ProductID == relatedConfiguredProduct && c.Index == 1);
					if (contentProductMatch == null)
					{
						contentProductMatch = new ContentProductMatch()
						{
							ProductID = relatedConfiguredProduct,
							StoreID = storeID,
							Index = 1,
							Description = desc,
							IsLeading = false
						};
						contentMatchRepo.Add(contentProductMatch);
						storedRepoList.Add(contentProductMatch);

					}


				}

				//if (confiProducts.IndexOf(confiProduct) % 1000 == 0) unit.Save(); //prevent a out of mem from ef

			}
			unit.Save();
			#endregion

			#region level 0

			storedRepoList = contentMatchRepo.GetAll().ToList();


			foreach (var confiProduct in confiProducts)
			{
				List<IGrouping<string, Product>> groups = new List<IGrouping<string, Product>>();
				try
				{
					groups = (from c in confiProduct.RelatedProductsSource.Where(c => c.IsConfigured && c.VendorID == vendorID).Select(c => c.RProduct)
										group c by
										c.ProductAttributeValues.Where(l => l.ProductAttributeMetaData.AttributeCode.ToLower() == "color").FirstOrDefault().Value into colorGroups
										select colorGroups
												).ToList();
				}
				catch (Exception e)
				{
					continue;
				}

				foreach (var colorProduct in groups)
				{
					var productIDS = colorProduct.Select(c => c.ProductID);

					var groupExisting = from m in storedRepoList
															where productIDS.Contains(m.ProductID)
															&& m.Index == 0
															select m;//get all the productIDs that are already in the ContentProductMatch table

					int storeID = 0;
					if (groupExisting.ToList().Count == 0)
					{
						storeID = storedRepoList.Count() == 0 ? 1 : storedRepoList.Max(c => c.StoreID) + 1;
					}
					else
					{
						storeID = groupExisting.FirstOrDefault().StoreID;
					}

					List<ContentProductMatch> matches = new List<ContentProductMatch>();

					bool checkIsLeading = false;

					//colorProduct is een lijst  van prodcuten met de kleur: rood
					foreach (var sku in colorProduct)
					{

						var contentMatch = contentMatchRepo.GetSingle(c => c.ProductID == sku.ProductID && c.Index == 0);//you dont have to use storedrepoList
						if (contentMatch == null)//product bestaat nog niet in de DB
						{
							contentMatch = new ContentProductMatch
							{
								Index = 0,
								ProductID = sku.ProductID,
								StoreID = storeID,
								Description = sku.RelatedProductsRelated.First(c => c.IsConfigured).SourceProduct.VendorAssortments.FirstOrDefault().Try(c => c.ShortDescription, string.Empty) + " " + sku.ProductAttributeValues.FirstOrDefault(c => c.ProductAttributeMetaData.AttributeCode.ToLower() == "color").Try(c => c.Value, string.Empty),
								IsLeading = false
							};

							matches.Add(contentMatch);
							storedRepoList.Add(contentMatch);

						}
						else//product bestaat in de DB
						{
							if (contentMatch.IsLeading)
								checkIsLeading = true;
						}
					}
					if (matches.Count > 0 && !checkIsLeading) //er zijn 2 nieuwe producten bij
						matches.FirstOrDefault().IsLeading = true;

					contentMatchRepo.Add(matches);
				}				
			}
			unit.Save();
			#endregion
		}
	}
}
