using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Concentrator.Objects.Bootstrapper.Installation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Prices;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;
using Concentrator.ui.Management.Extensions;
using Concentrator.ui.Management.MasterGroupMappings;
using Concentrator.ui.Management.MasterGroupMappings.Managers;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

using Newtonsoft.Json;

using PetaPoco;

namespace Concentrator.ui.Management.Controllers
{
  public class MasterGroupMappingController : BaseController
  {
    #region Tree
    [RequiresAuthentication(Functionalities.GetMasterGroupMapping)]
    public ActionResult GetListOfVendorProductGroupsPerMGM(int MasterGroupMappingID)
    {
      return List(unit => (from x in unit.Service<MasterGroupMapping>().GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID)
                           from vpg in x.ProductGroupVendors
                           select new
                           {
                             vpg.ProductGroupVendorID,
                             VendorCodeDescription = vpg.VendorName,
                             ProductGroupCode = vpg.VendorProductGroupCode1 ?? vpg.VendorProductGroupCode2 ??
                                         vpg.VendorProductGroupCode3 ?? vpg.VendorProductGroupCode4 ??
                                         vpg.VendorProductGroupCode5 ?? vpg.VendorProductGroupCode6 ??
                                         vpg.VendorProductGroupCode7 ?? vpg.VendorProductGroupCode8 ??
                                         vpg.VendorProductGroupCode9 ?? vpg.VendorProductGroupCode10 ?? string.Empty,
                             vpg.BrandCode,
                             VendorName = vpg.Vendor.Name

                           }));
    }

    [RequiresAuthentication(Functionalities.GetMasterGroupMapping)]
    public ActionResult GetTreeView(int MasterGroupMappingID, int? ConnectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var showProductCounter = (ConfigurationManager.AppSettings["MGMshowProductCounter"] ?? Boolean.FalseString)
          .ParseToBool()
          .GetValueOrDefault();

        var listOfMasterGroupMappings = ConnectorID.HasValue && ConnectorID.Value > 0
          ? GetListOfChildMasterGroupMappingsForMasterGroupMappingTree(MasterGroupMappingID, showProductCounter, ConnectorID.Value)
          : GetListOfChildMasterGroupMappingsForMasterGroupMappingTree(MasterGroupMappingID, showProductCounter, null);

        var masterGroupMappingList = listOfMasterGroupMappings
          .Select(x => new
          {
            MasterGroupMappingID = x.MasterGroupMappingID,
            text = showProductCounter ? x.MasterGroupMappingName + " (" + x.CountProducts + "-" + x.CountNotApprovedProducts + ")" : x.MasterGroupMappingName,
            SourceMasterGroupMappingID = x.SourceMasterGroupMappingID,
            leaf = false
          });

        return Json(masterGroupMappingList);
      }
    }

    [RequiresAuthentication(Functionalities.CreateMasterGroupMapping)]
    public ActionResult Create(int MasterGroupMappingID, int? ProductGroupID)
    {
      try
      {
        if (ProductGroupID.HasValue && ProductGroupID.Value > 1)
        {
          using (var unit = GetUnitOfWork())
          {
            //create parent
            MasterGroupMapping masterGroupMapping = new MasterGroupMapping()
            {
              ProductGroupID = ProductGroupID.Value,
              Score = 0
            };

            if (MasterGroupMappingID != -1)
            {
              masterGroupMapping.ParentMasterGroupMappingID = MasterGroupMappingID;
            }

            unit.Service<MasterGroupMapping>().Create(masterGroupMapping);

            unit.Save();
          }
          return Success("Successfully created object", isMultipartRequest: true, needsRefresh: false);
        }
        return Failure("Failed to create Master Group Mapping!");
      }
      catch (Exception e)
      {
        return Failure("Failed to create object ", e, isMultipartRequest: true);
      }
    }

    /// <summary>
    /// Delete MasterGroupMapping
    /// </summary>
    /// <returns>Succes: Delete References: Products, VendorProductGroups, Languages, Users, Required Attributes. And set MasterGroupMappingID on null in ProductGroupMapping </returns>
    /// <returns>Failure: If MasterGroupMapping used in => CrossReference, Selector, SourceMasterGroupMapping, SourceProductGroupMapping</returns>
    [RequiresAuthentication(Functionalities.DeleteProductGroupMapping)]
    public ActionResult Delete(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (id < 0)
            return Failure("Failed to delete product group mapping, it is not possible to remove the root");
          else
          {
            DeleteHierarchy(id, unit);
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

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfRelatedObjectsToMasterGroupMapping(int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var list = GetMasterGroupMappingRelatedObjects(MasterGroupMappingID, unit);

        return Json(new { results = list });
      }
    }

    #region Drag and Drop
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult CopyTreeNodes(int MasterGroupMappingID, int ParentMasterGroupMappingID, bool CopyProducts, bool CopyAttributes, bool copyCrossReferences)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          CopyNodes(MasterGroupMappingID, ParentMasterGroupMappingID, CopyProducts, CopyAttributes, copyCrossReferences, unit);
        }
        return Success("Master group mapoping successfully copied", needsRefresh: false);
      }
      catch (Exception e)
      {
        return Failure("Failed to copy master group mapping", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult MoveTreeNodes(int DDMasterGroupMappingID, int ParentMasterGroupMappingID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          MasterGroupMapping masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == DDMasterGroupMappingID);
          if (ParentMasterGroupMappingID > -1)
          {
            masterGroupMapping.ParentMasterGroupMappingID = ParentMasterGroupMappingID;
          }
          else
          {
            masterGroupMapping.ParentMasterGroupMappingID = null;
          }
          unit.Save();
        }
        return Success("Master group mapping is successfully moved");
      }
      catch (Exception e)
      {
        return Failure("Failed to move master group mapping ", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult CrossReferenceTreeNode(int MasterGroupMappingID, int CrossReferenceID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var masterGroupMapping = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

          var crossReference = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == CrossReferenceID);

          masterGroupMapping.MasterGroupMappingCrossReferences.Add(crossReference);

          unit.Save();
        }
        return Success("Master Group Mapping is successfully crossed");
      }
      catch (Exception e)
      {
        return Failure("Failed to cross Master Group Mapping. ", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult ReorganizeTreeDirectory(string MasterGroupMappingJsonObject)
    {
      var ListOfIDsModel = new
      {
        MasterGroupMappingID = 0,
        ListOfChildrenID = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(MasterGroupMappingJsonObject, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          int counter = 0;
          foreach (int masterGroupMappingID in ListOfIDs.ListOfChildrenID.Reverse())
          {
            var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == masterGroupMappingID);
            masterGroupMapping.Score = counter;
            counter++;
          }
          unit.Save();
        }
        return Success("Master group mapping tree successfully changed.", needsRefresh: false);
      }
      catch (Exception e)
      {
        return Failure("Failed to change master group mapping tree.", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetTreeNodePathByMasterGroupMappingID(int? MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        ArrayList listOfMasterGroupMappingIDs = new ArrayList();
        if (MasterGroupMappingID.HasValue && MasterGroupMappingID.Value > 0)
        {
          bool hasParent = true;
          do
          {
            var masterGroupmapping = unit.Service<MasterGroupMapping>()
              .Get(m => m.MasterGroupMappingID == MasterGroupMappingID);

            listOfMasterGroupMappingIDs.Add(masterGroupmapping.MasterGroupMappingID);

            if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
              MasterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
            else
              hasParent = false;

          } while (hasParent);
          listOfMasterGroupMappingIDs.Reverse();
        }
        return Json(new { results = listOfMasterGroupMappingIDs });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetTreeNodesPathByMasterGroupMappingID(string MasterGroupMappingIDsJson)
    {
      var MasterGroupMappingIDsModel = new
      {
        MasterGroupMappingIDs = new[] { 0 }
      };
      var MasterGroupMappingIDs = JsonConvert.DeserializeAnonymousType(MasterGroupMappingIDsJson, MasterGroupMappingIDsModel);

      using (var unit = GetUnitOfWork())
      {
        Dictionary<int, List<int>> listOfPathesIDs = new Dictionary<int, List<int>>();
        foreach (var masterGroupMappingID in MasterGroupMappingIDs.MasterGroupMappingIDs)
        {
          List<int> listOfMasterGroupMappingIDs = new List<int>();
          if (masterGroupMappingID > 0)
          {
            bool hasParent = true;
            var MasterGroupMappingID = masterGroupMappingID;
            do
            {
              var masterGroupmapping = unit.Service<MasterGroupMapping>()
                .Get(m => m.MasterGroupMappingID == MasterGroupMappingID);

              listOfMasterGroupMappingIDs.Add(masterGroupmapping.MasterGroupMappingID);

              if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
                MasterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
              else
                hasParent = false;

            } while (hasParent);
            listOfMasterGroupMappingIDs.Reverse();
          }
          listOfPathesIDs.Add(masterGroupMappingID, listOfMasterGroupMappingIDs);
        }
        return Json(new { results = listOfPathesIDs.ToList() });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateMasterGroupMapping(string SaveParamsJson)
    {
      var SaveParamsModel = new
      {
        DroppedMasterGroupMappingID = 0,
        ParentMasterGroupMappingID = 0,
        childrenOfParenMasterGroupMappingID = new[] { 0 }
      };
      var SaveParams = JsonConvert.DeserializeAnonymousType(SaveParamsJson, SaveParamsModel);

      try
      {
        if (SaveParams.DroppedMasterGroupMappingID > 0)
        {
          using (var unit = GetUnitOfWork())
          {
            MasterGroupMapping masterGroupMapping = unit
              .Service<MasterGroupMapping>()
              .Get(x => x.MasterGroupMappingID == SaveParams.DroppedMasterGroupMappingID);

            if (masterGroupMapping != null)
            {
              if (masterGroupMapping.ParentMasterGroupMappingID != SaveParams.ParentMasterGroupMappingID)
              {
                if (SaveParams.ParentMasterGroupMappingID > 0)
                {
                  masterGroupMapping.ParentMasterGroupMappingID = SaveParams.ParentMasterGroupMappingID;
                }
                else
                {
                  masterGroupMapping.ParentMasterGroupMappingID = null;
                }
              }

              int counter = 0;
              foreach (int masterGroupMappingID in SaveParams.childrenOfParenMasterGroupMappingID.Reverse())
              {
                MasterGroupMapping childMasterGroupMapping = unit
                  .Service<MasterGroupMapping>()
                  .Get(x => x.MasterGroupMappingID == masterGroupMappingID);

                childMasterGroupMapping.Score = counter;
                counter++;
              }
              unit.Save();
            }
            else
            {
              return Failure("Failed to update Master Group Mapping!");
            }
          }
          return Success("Successfully updated Master Group Mapping", isMultipartRequest: true, needsRefresh: false);
        }
        return Failure("Failed to update Master Group Mapping!");
      }
      catch (Exception e)
      {
        return Failure("Failed to update Master Group Mapping ", e, isMultipartRequest: true);
      }

    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult CopyOrMoveMatchedProducts(string listOfIDsJson)
    {
      var listOfIDsModel = new
      {
        CopyProducts = false,
        MasterGroupMappingID = 0,
        ListOfRecords = new[] { new { MasterGroupMappingID = 0, ProductID = 0 } }
      };
      var listOfIDs = JsonConvert.DeserializeAnonymousType(listOfIDsJson, listOfIDsModel);

      if (listOfIDs.ListOfRecords.Count() > 0)
      {
        using (var unit = GetUnitOfWork())
        {
          List<int> listOfErrorProducts = new List<int>();

          listOfIDs.ListOfRecords.ForEach(item =>
          {
            MasterGroupMappingProduct masterGroupMappingProduct = unit
              .Service<MasterGroupMappingProduct>()
              .Get(x => x.MasterGroupMappingID == item.MasterGroupMappingID && x.ProductID == item.ProductID);

            if (masterGroupMappingProduct != null)
            {
              MasterGroupMappingProduct tempMasterGroupMappingProduct = unit
                .Service<MasterGroupMappingProduct>()
                .Get(x => x.MasterGroupMappingID == listOfIDs.MasterGroupMappingID && x.ProductID == item.ProductID);
              if (tempMasterGroupMappingProduct == null)
              {
                MasterGroupMappingProduct newMasterGroupMappingProduct = new MasterGroupMappingProduct()
                {
                  MasterGroupMappingID = listOfIDs.MasterGroupMappingID,
                  ProductID = item.ProductID,
                  IsApproved = masterGroupMappingProduct.IsApproved,
                  IsProductMapped = true
                };
                unit.Service<MasterGroupMappingProduct>().Create(newMasterGroupMappingProduct);
                if (!listOfIDs.CopyProducts)
                {
                  masterGroupMappingProduct.IsProductMapped = true;
                }
                unit.Save();
              }
              else
              {
                if (!tempMasterGroupMappingProduct.IsProductMapped)
                {
                  tempMasterGroupMappingProduct.IsProductMapped = true;
                  unit.Save();
                }
                else
                {
                  listOfErrorProducts.Add(item.ProductID);
                }
              }
            }
            else
            {
              listOfErrorProducts.Add(item.ProductID);
            }
          });

          if (listOfErrorProducts.Count > 0)
          {
            return Failure("Not all products are updated.");
          }
          else
          {
            return Success("Products successfully updated.");
          }
        }
      }
      else
      {
        return Failure("There are no items to update.");
      }
    }
    #endregion

    #region Menu "Add Master Group Mapping"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfGroupNames(string filterGroupName)
    {
      using (var unit = GetUnitOfWork())
      {
        var filter = string.IsNullOrEmpty(filterGroupName) ?
           string.Empty :
           string.Format("AND ml.NAME like '%{0}%'", filterGroupName);

        var masterGroupMappingLanguagesQuery = string.Format(@"
                SELECT mm.MasterGroupMappingID
                	,ml.LanguageID
                	,ml.NAME
                	,lang.NAME AS LanguageName
                FROM Mastergroupmapping mm
                INNER JOIN MasterGroupmappingLanguage ml ON ml.MasterGroupMappingID = mm.MasterGroupMappingID
                INNER JOIN [Language] lang ON lang.LanguageID = ml.LanguageID
                WHERE mm.ConnectorID IS NULL {0};
                ", filter);

        var masterGroupMappingNames = unit
          .ExecuteStoreQuery<ListOfMasterGroupMappingLanguages>(masterGroupMappingLanguagesQuery)
          .GroupBy(x => x.MasterGroupMappingID)
          .ToDictionary(x => x.Key, x => x.ToList())
          .Select(mapping => new ListOfGroupNameResult
          {
            IsMasterGroupMapping = true,
            ID = mapping.Key,
            LanguageTranslation = mapping.Value.Select(l => new { Language = l.LanguageName, Translation = l.Name }.ToString()).ToList(),
            GroupName = mapping.Value.Where(l => l.LanguageID == Client.User.LanguageID).Select(v => v.Name).FirstOrDefault()
          }).ToList();

        return List((u) => masterGroupMappingNames.AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult CreateMasterGroupMapping(string ListOfLanguageIDsAndValuesJson)
    {
      var ListOfLanguageIDsAndValuesModel = new
      {
        LanguageIDs = new[] { new { LanguageID = 0, Value = "", LanguageMustBeFilled = false } },
        ParentMasterGroupMappingID = 0,
        ConnectorID = 0
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfLanguageIDsAndValuesJson, ListOfLanguageIDsAndValuesModel);

      using (var unit = GetUnitOfWork())
      {
        var listOfAllFilledLanguagesWithOutSelected = ListOfIDs
          .LanguageIDs
          .Where(x => !string.IsNullOrEmpty(x.Value) && x.LanguageMustBeFilled == false)
          .ToList();

        if (listOfAllFilledLanguagesWithOutSelected.Count > 0)
        {
          StringBuilder message = new StringBuilder();
          message.Append("Not all records are correctly filled. The next languages are filled but not selected:");
          message.Append("<br><br>");
          listOfAllFilledLanguagesWithOutSelected.ForEach(x =>
          {
            var language = unit
              .Service<Language>()
              .Get(l => l.LanguageID == x.LanguageID)
              .Name;

            message.AppendFormat("Language: <b>{0}</b> ({1})", language, x.Value).Append("<br>");
          });
          return Failure(message.ToString());
        }
        else
        {
          var listOfAllFilledSelectedLanguages = ListOfIDs
            .LanguageIDs
            .Where(x => !string.IsNullOrEmpty(x.Value) && x.LanguageMustBeFilled)
            .ToList();

          var listOfNotFilledRequiredLanguages = unit
            .Service<UserLanguage>()
            .GetAll(x => x.UserID == Client.User.UserID)
            .Select(x => x.LanguageID)
            .Except(listOfAllFilledSelectedLanguages.Select(x => x.LanguageID));

          if (listOfNotFilledRequiredLanguages.Count() > 0)
          {
            StringBuilder message = new StringBuilder();
            message.Append("Not all records are correctly filled. The next languages are required but not filled:");
            message.Append("<br><br>");
            listOfNotFilledRequiredLanguages.ForEach(x =>
            {
              var language = unit
                .Service<Language>()
                .Get(l => l.LanguageID == x)
                .Name;

              message.AppendFormat("Language: <b>{0}</b>", language).Append("<br>");
            });
            return Failure(message.ToString());
          }
          else
          {
            // TODO: dit is nodig zolang er een relatie is tussen ProductGroup en MasterGroupMapping. Dit moet uiteindelijk weg.
            var productGroup = unit
              .Service<ProductGroup>()
              .GetAll()
              .First();
            //

            int countChildren;
            if (ListOfIDs.ParentMasterGroupMappingID > 0)
            {
              countChildren = unit
                .Service<MasterGroupMapping>()
                .GetAll(x => x.ParentMasterGroupMappingID == ListOfIDs.ParentMasterGroupMappingID)
                .Count();
            }
            else
            {
              countChildren = unit
                .Service<MasterGroupMapping>()
                .GetAll(x => x.ParentMasterGroupMappingID == null)
                .Count();
            }

            MasterGroupMapping masterGroupMapping = new MasterGroupMapping()
            {
              ProductGroupID = productGroup.ProductGroupID,
              Score = countChildren + 1
            };

            if (ListOfIDs.ParentMasterGroupMappingID != -1)
            {
              masterGroupMapping.ParentMasterGroupMappingID = ListOfIDs.ParentMasterGroupMappingID;
            }

            if (ListOfIDs.ConnectorID > 0)
            {
              masterGroupMapping.ConnectorID = ListOfIDs.ConnectorID;
            }

            unit.Service<MasterGroupMapping>().Create(masterGroupMapping);
            unit.Save();

            listOfAllFilledSelectedLanguages.ForEach(x =>
            {
              MasterGroupMappingLanguage masterGroupMappingLanguage = new MasterGroupMappingLanguage()
              {
                MasterGroupMappingID = masterGroupMapping.MasterGroupMappingID,
                LanguageID = x.LanguageID,
                Name = x.Value
              };
              unit.Service<MasterGroupMappingLanguage>().Create(masterGroupMappingLanguage);
            });
            unit.Save();
            return Success("Successfully created Master Group Mapping");
          }
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult AddSelectedRecordToMasterGroupMapping(string RecordJson)
    {
      var RecordModel = new
      {
        ID = 0,
        IsMasterGroupMapping = true,
        ParentMasterGroupMappingID = 0,
        ConnectorID = 0
      };
      var Record = JsonConvert.DeserializeAnonymousType(RecordJson, RecordModel);

      using (var unit = GetUnitOfWork())
      {
        List<ListOfGroupNameResult> currentLanguages = new List<ListOfGroupNameResult>();

        if (Record.IsMasterGroupMapping)
        {
          currentLanguages = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == Record.ID)
            .MasterGroupMappingLanguages
            .Select(x => new ListOfGroupNameResult()
            {
              GroupName = x.Name,
              ID = x.LanguageID
            })
            .ToList();
        }
        else
        {
          currentLanguages = unit
            .Service<ProductGroup>()
            .Get(x => x.ProductGroupID == Record.ID)
            .ProductGroupLanguages
            .Select(x => new ListOfGroupNameResult()
            {
              GroupName = x.Name,
              ID = x.LanguageID
            })
            .ToList();
        }

        // TODO: dit is nodig zolang er een relatie is tussen ProductGroup en MasterGroupMapping. Dit moet uiteindelijk weg.
        var productGroup = unit
          .Service<ProductGroup>()
          .GetAll()
          .First();
        //

        int countChildren;
        if (Record.ParentMasterGroupMappingID > 0)
        {
          countChildren = unit
            .Service<MasterGroupMapping>()
            .GetAll(x => x.ParentMasterGroupMappingID == Record.ParentMasterGroupMappingID)
            .Count();
        }
        else
        {
          countChildren = unit
            .Service<MasterGroupMapping>()
            .GetAll(x => x.ParentMasterGroupMappingID == null)
            .Count();
        }

        MasterGroupMapping masterGroupMapping = new MasterGroupMapping()
        {
          ProductGroupID = productGroup.ProductGroupID,
          Score = countChildren + 1
        };

        MasterGroupMapping parentMasterGroupMapping = null;
        if (Record.ParentMasterGroupMappingID != -1)
        {
          masterGroupMapping.ParentMasterGroupMappingID = Record.ParentMasterGroupMappingID;
          parentMasterGroupMapping = unit.Service<MasterGroupMapping>().Get(c => c.MasterGroupMappingID == Record.ParentMasterGroupMappingID);

          //copy assigned users from parent
          if (parentMasterGroupMapping != null)
          {
            masterGroupMapping.Users = parentMasterGroupMapping.Users;
          }
        }

        if (Record.ConnectorID > 0)
        {
          masterGroupMapping.ConnectorID = Record.ConnectorID;
        }

        unit.Service<MasterGroupMapping>().Create(masterGroupMapping);
        unit.Save();

        currentLanguages.ForEach(x =>
        {
          MasterGroupMappingLanguage masterGroupMappingLanguage = new MasterGroupMappingLanguage()
          {
            MasterGroupMappingID = masterGroupMapping.MasterGroupMappingID,
            LanguageID = x.ID,
            Name = x.GroupName
          };
          unit.Service<MasterGroupMappingLanguage>().Create(masterGroupMappingLanguage);
        });
        unit.Save();

        return Success("Successfully created Master Group Mapping");
      }
    }
    #endregion

    #region Menu "Find Master Group Mapping"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMasterGroupMappingByConnectorID(int? connectorID)
    {
      var listOfMasterGroupMapping = GetListOfMasterGroupMappingPaths(connectorID, Client.User.LanguageID);
      return List(listOfMasterGroupMapping.AsQueryable());
    }
    #endregion

    #region Menu "Product Control Management"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfProductControle()
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfProductControles = unit
          .Service<ProductControl>()
          .GetAll()
          .Select(x => new
          {
            x.ProductControlID,
            x.ProductControlName,
            x.IsActive
          });

        return Json(new { results = listOfProductControles });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMatchedProductsForProductControleWizard(int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        #region baseQuery
        var baseQuery = @"with filteredProducts as (
select 
					mgp.mastergroupmappingid,
					min(mgp.productid) as productid,
					convert(bit,min(convert(int,isapproved))) isapproved
					from mastergroupmappingproduct mgp
          inner join vendorassortment va on va.productid=mgp.productid
					inner join productmatch pm on mgp.productid = pm.productid and ismatched = 1
					where IsProductMapped = 1 and va.isactive=1
					group by mgp.mastergroupmappingid, pm.productmatchid
					union
					select mgp.mastergroupmappingid,
					mgp.productid,
					isapproved
					from mastergroupmappingproduct mgp
          inner join vendorassortment va on va.productid=mgp.productid
					left join productmatch pm on mgp.productid = pm.productid
					where pm.productmatchid is null and IsProductMapped = 1 and va.isactive=1
					union
					select mgp.mastergroupmappingid,
					mgp.productid,
					isapproved
					from mastergroupmappingproduct mgp
          inner join vendorassortment va on va.productid=mgp.productid
					inner join productmatch pm on mgp.productid = pm.productid and ismatched = 0
					where IsProductMapped = 1 and va.isactive=1
)
{0}";
        #endregion

        var mappingQuery = string.Format(baseQuery, @"select  
fp.productid,
fp.mastergroupmappingid,
b.name,
p.vendoritemnumber,
fp.isapproved
from filteredProducts fp
inner join product p on fp.productid = p.productid
inner join brand b on b.brandid = p.brandid
where isapproved = 0 and p.productid in (Select ProductID FROM VendorAssortment WHERE IsActive=1)");
        var listOfMGMProducts = unit.ExecuteStoreQuery<MasterGroupmappingFilter>(mappingQuery).Where(x => x.MasterGroupMappingID == MasterGroupMappingID).ToList();

        var vendorassortmentQuery = string.Format(baseQuery, string.Format(@"select  
va.*
from filteredProducts fp
inner join vendorassortment va on va.productid = fp.productid
where fp.mastergroupmappingid = {0}", MasterGroupMappingID));
        var vendorAssortments = unit.ExecuteStoreQuery<VendorAssortment>(vendorassortmentQuery).ToList();

        var productBarcodeQuery = string.Format(baseQuery, string.Format(@"select  
pb.*
from filteredProducts fp
inner join productbarcode pb on pb.productid = fp.productid
where fp.mastergroupmappingid = {0}", MasterGroupMappingID));
        var productBarcodes = unit.ExecuteStoreQuery<ProductBarcode>(productBarcodeQuery).ToList();

        var productMediaQuery = string.Format(baseQuery, string.Format(@"select  
pm.*
from filteredProducts fp
inner join productmedia pm on pm.productid = fp.productid
where fp.mastergroupmappingid = {0}", MasterGroupMappingID));
        var productMedias = unit.ExecuteStoreQuery<ProductMedia>(productMediaQuery).ToList();

        string attributeNames = string.Format(@"select pamd.attributeid, pan.name, pagn.name as GroupName, v.name as VendorName from productattributemetadata pamd
inner join productattributename pan on pan.attributeid = pamd.attributeid and pan.languageid = {0}
inner join dbo.ProductAttributeGroupName pagn on pamd.productattributegroupid = pagn.productattributegroupid and pagn.languageid = {0}
inner join vendor v on pamd.vendorid = v.vendorid", Client.User.LanguageID);

        var masterGroupMappingAttributeNames = unit.ExecuteStoreQuery<MasterGroupMappingProducAttributeName>(attributeNames).ToList();

        string productAttributeValuesQuery = string.Format(baseQuery, string.Format(@"select 
pav.*
from filteredProducts fp 
inner join productattributevalue pav on pav.productid = fp.productid
where fp.mastergroupmappingid = {0}", MasterGroupMappingID));
        var productAttributeValues = (from pav in unit.ExecuteStoreQuery<ProductAttributeValue>(productAttributeValuesQuery).ToList()
                                      join pamd in masterGroupMappingAttributeNames on pav.AttributeID equals pamd.AttributeID
                                      select new
                                      {
                                        AttributeID = pav.AttributeID,
                                        AttributeName = pamd.Name,
                                        AttributeGroup = pamd.GroupName,
                                        AttributeValue = pav.Value,
                                        VendorName = pamd.VendorName,
                                        pav.ProductID
                                      });

        var productAttributeMetaData = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(x => x.AttributeCode == "MGMAttributeCheck");

        var countProductsToControl = listOfMGMProducts.Count();

        var listOfProducts = listOfMGMProducts
          .ToList()
          .Select(x =>
          {
            var vendorAssortmentProduct = vendorAssortments.Where(v => v.ProductID == x.ProductID).FirstOrDefault();
            var customItemNumbers = vendorAssortments.Where(v => v.ProductID == x.ProductID).Select(v => v.CustomItemNumber).Distinct().ToList();
            var productBarcode = productBarcodes.Where(z => z.ProductID == x.ProductID).Select(z => z.Barcode).FirstOrDefault();
            var productMedia = productMedias.Where(z => z.ProductID == x.ProductID).OrderBy(z => z.Sequence).Select(z => new { isUrl = z.MediaUrl != null ? true : false, Image = z.MediaUrl ?? z.MediaPath }).FirstOrDefault();
            var productAttributeValue = productAttributeValues.Where(z => z.ProductID == x.ProductID).ToList();
            var mgmAttribute = productAttributeMetaData != null ? productAttributeValue.FirstOrDefault(a => a.AttributeID == productAttributeMetaData.AttributeID) : null;
            return new
            {
              ConcentratorNumber = x.ProductID,
              MasterGroupMappingID,
              DistriItemNumber = customItemNumbers != null && customItemNumbers.Count() > 0 ? string.Join(",", customItemNumbers) : string.Empty,
              ProductName = vendorAssortmentProduct != null ? vendorAssortmentProduct.ShortDescription : string.Empty,
              BrandName = x.Name,
              ShortDescription = vendorAssortmentProduct != null ? vendorAssortmentProduct.ShortDescription : string.Empty,
              Title = string.Empty,// x.ProductDescriptions.Select(y => y.).FirstOrDefault(),
              CustomItemNumber = vendorAssortmentProduct != null ? vendorAssortmentProduct.CustomItemNumber : string.Empty,
              x.VendorItemNumber,
              Barcode = productBarcode,
              LongDescription = vendorAssortmentProduct != null ? vendorAssortmentProduct.LongDescription : string.Empty,
              Images = productMedia,
              ProductApproved = false,
              Step = mgmAttribute != null ? mgmAttribute.AttributeValue : "Step 1 - Product",
              Attributes = productAttributeValue,
              countProductsToControl
            };
          })
          ;

        var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
        return new ContentResult()
        {
          Content = serializer.Serialize(new { results = listOfProducts }),
          ContentType = "application/json"
        };

      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMatchedProductForProductControle(int ProductID, int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var mgmAttribute = unit.Scope.Repository<ProductAttributeValue>().GetSingle(x => x.ProductID == ProductID && x.ProductAttributeMetaData.AttributeCode == "MGMAttributeCheck");

        var listOfProducts = (
          from x in unit.Service<Product>()
          .GetAll(x => x.ProductID == ProductID)
          .ToList()
          let customItemNumbers = x.VendorAssortments.Select(v => v.CustomItemNumber).Distinct()
          select new
           {
             ConcentratorNumber = x.ProductID,
             MasterGroupMappingID = MasterGroupMappingID,
             DistriItemNumber = customItemNumbers != null && customItemNumbers.Count() > 0 ? string.Join(",", customItemNumbers) : string.Empty,
             ProductName = x.VendorAssortments.Select(v => v.ShortDescription).FirstOrDefault(),
             BrandName = x.Brand.Name,
             Title = string.Empty,// x.ProductDescriptions.Select(y => y.Product.Title).FirstOrDefault(),
             ShortDescription = x.VendorAssortments.Select(v => v.ShortDescription).FirstOrDefault(),
             CustomItemNumber = x.VendorAssortments.Select(v => v.CustomItemNumber).FirstOrDefault(),
             x.VendorItemNumber,
             Barcode = x.ProductBarcodes.Select(pb => pb.Barcode).FirstOrDefault(),
             LongDescription = x.VendorAssortments.Select(v => v.LongDescription).FirstOrDefault(),
             Images = x.ProductMedias.Select(pm => pm.MediaUrl),
             Step = mgmAttribute != null ? mgmAttribute.Value : "Step 1 - Product",
             Attributes = x.ProductAttributeValues.Select(a => new
             {
               AttributeID = a.AttributeID,
               AttributeName = a.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID).Name,
               AttributeGroup = a.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID).Name,
               AttributeValue = a.Value,
               VendorName = a.ProductAttributeMetaData.Vendor.Name
             })
           });
        return Json(new { results = listOfProducts });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfProductControleOfProduct(int ProductID, int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var productControles = unit
          .Service<ProductControl>()
          .GetAll(x => x.IsActive == true);
        var masterGroupMapping = unit.Service<MasterGroupMapping>()
          .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
        var countAttributes = masterGroupMapping
          .ProductAttributeMetaDatas
          .Count;

        ArrayList listOfItems = new ArrayList();

        if (productControles.Where(x => x.ProductControlID == 1).Select(x => x.IsActive).FirstOrDefault())
        {
          var listItem = new
          {
            ProductControlID = productControles.Where(x => x.ProductControlID == 1).Select(x => x.ProductControlID),
            ProductControlName = productControles.Where(x => x.ProductControlID == 1).Select(x => x.ProductControlName),
            ProductControlApproved = ProductControlProductImage(unit, ProductID)
          };
          listOfItems.Add(listItem);
        }

        if (productControles.Where(x => x.ProductControlID == 2).Select(x => x.IsActive).FirstOrDefault())
        {
          var listItem = new
          {
            ProductControlID = productControles.Where(x => x.ProductControlID == 2).Select(x => x.ProductControlID),
            ProductControlName = (countAttributes > 0) ? (productControles.Single(x => x.ProductControlID == 2).ProductControlName) : ("No Required Attributes Available"),
            ProductControlApproved = ProductControlProductAttributes(masterGroupMapping, unit, ProductID)
          };
          listOfItems.Add(listItem);
        }

        if (productControles.Where(x => x.ProductControlID == 3).Select(x => x.IsActive).FirstOrDefault())
        {
          var listItem = new
          {
            ProductControlID = productControles.Where(x => x.ProductControlID == 3).Select(x => x.ProductControlID),
            ProductControlName = productControles.Where(x => x.ProductControlID == 3).Select(x => x.ProductControlName),
            ProductControlApproved = ProductControlProductMatch(masterGroupMapping, unit, ProductID)
          };
          listOfItems.Add(listItem);
        }

        return Json(new { results = listOfItems });
      }
    }

    private bool ProductControlShortDescriptionIsNotEmpty(MasterGroupMapping masterGroupMapping, IServiceUnitOfWork unit, int ProductID)
    {
      return unit.Scope.Repository<ProductDescription>().GetAll(x => !string.IsNullOrEmpty(x.ShortContentDescription) && x.ProductID == ProductID).Count() > 0 ? true : false;
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfAttributeValues(int ProductID, int MasterGroupMappingID)
    {
      return List((unit) => (from x in unit.Service<ProductAttributeMetaData>().GetAll(x => x.MasterGroupMappings.Any(m => m.MasterGroupMappingID == MasterGroupMappingID)).ToList()
                             let attributes = x.ProductAttributeValues.Where(c => c.LanguageID == Client.User.LanguageID && c.ProductID == ProductID)
                             let productAttributeValueID = attributes.Count() > 0 ? attributes.FirstOrDefault().AttributeValueID : -1
                             let productAttributeValues = attributes.Count() > 0 ? attributes.FirstOrDefault().Value : string.Empty
                             select new
                             {
                               ProductID,
                               x.AttributeID,
                               AttributeGroupName = x.ProductAttributeGroupMetaData.ProductAttributeGroupNames.Where(c => c.LanguageID == Client.User.LanguageID).Select(p => p.Name).FirstOrDefault(),
                               AttributeName = x.ProductAttributeNames.Where(c => c.LanguageID == Client.User.LanguageID).Select(p => p.Name).FirstOrDefault(),
                               VendorName = x.Vendor.Name,
                               AttributeValue = productAttributeValues,
                               AttributeValueID = productAttributeValueID,
                             }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfAllMatchedProductsForControl()
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfMGMProducts = unit
          .Service<MasterGroupMappingProduct>()
          .GetAll(x => x.IsApproved == false && x.IsProductMapped == true)
          .Select(x => new { MasterGroupMappingID = x.MasterGroupMappingID, Product = x.Product })
          ;

        var countProductsToControl = listOfMGMProducts.Count();

        var listOfProducts = listOfMGMProducts
          .Take(50)
          .ToList()
          .Select(x =>
          {
            var customItemNumbers = x.Product.VendorAssortments.Select(v => v.CustomItemNumber).Distinct();
            return new
            {
              ConcentratorNumber = x.Product.ProductID,
              x.MasterGroupMappingID,
              DistriItemNumber = customItemNumbers != null && customItemNumbers.Count() > 0 ? string.Join(",", customItemNumbers) : string.Empty,
              ProductName = x.Product.VendorAssortments.Select(v => v.ShortDescription).FirstOrDefault(),
              BrandName = x.Product.Brand.Name,
              ShortDescription = x.Product.VendorAssortments.Select(v => v.ShortDescription).FirstOrDefault(),
              Title = string.Empty,// x.Product.ProductDescriptions.Select(y => y.Product.Title).FirstOrDefault(),
              CustomItemNumber = x.Product.VendorAssortments.Select(v => v.CustomItemNumber).FirstOrDefault(),
              x.Product.VendorItemNumber,
              Barcode = x.Product.ProductBarcodes.Select(pb => pb.Barcode).FirstOrDefault(),
              LongDescription = x.Product.VendorAssortments.Select(v => v.LongDescription).FirstOrDefault(),
              Images = x.Product.ProductMedias.Select(pm => pm.MediaUrl),
              Attributes = x.Product.ProductAttributeValues.Select(a => new
              {
                AttributeID = a.AttributeID,
                AttributeName = a.ProductAttributeMetaData.ProductAttributeNames.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.Name).FirstOrDefault(),
                AttributeGroup = a.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.Name).FirstOrDefault(),
                AttributeValue = a.Value,
                VendorName = a.ProductAttributeMetaData.Vendor.Name
              }),
              countProductsToControl
            };
          })
          ;
        var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
        return new ContentResult()
        {
          Content = serializer.Serialize(new { results = listOfProducts }),
          ContentType = "application/json"
        };

      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateProductControle(int id, bool? IsActive)
    {
      return Update<ProductControl>(pc => pc.ProductControlID == id, (unit, pc) =>
      {
        pc.IsActive = IsActive.HasValue ? IsActive.Value : false;
      });
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateAttributeValues(int _AttributeValueID, int _AttributeID, int _ProductID, string AttributeValue)
    {
      if (_AttributeValueID > 0)
      {
        return Update<ProductAttributeValue>(pav => pav.AttributeValueID == _AttributeValueID, (unit, pav) =>
        {
          pav.Value = AttributeValue;
        });
      }
      else
      {
        ProductAttributeValue productAttributeValue = new ProductAttributeValue()
        {
          AttributeID = _AttributeID,
          ProductID = _ProductID,
          Value = AttributeValue,
          LanguageID = Client.User.LanguageID,
          CreatedBy = Client.User.UserID,
          CreationTime = DateTime.Now.ToUniversalTime()
        };

        try
        {
          using (var unit = GetUnitOfWork())
          {
            unit.Service<ProductAttributeValue>().Create(productAttributeValue);
            unit.Save();
          }
          return Success("Attribute is successfully updated.");
        }
        catch (Exception e)
        {
          return Failure("Failed to update the attribute. ", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetAllAttributesForAllMatchingProducts(int productID)
    {
      using (var unit = GetUnitOfWork())
      {

        var productMatch = unit.Service<ProductMatch>().Get(x => x.ProductID == productID);

        var productMatchID = productMatch != null ? productMatch.ProductMatchID : -1;

        if (productMatchID > 0)
        {
          var matchedProducts = unit.Service<ProductMatch>().GetAll(x => x.ProductMatchID == productMatchID)
              .Where(x => x.Product.VendorAssortments.Any(va => va.IsActive))
              .ToList();

          var listOfProducts = (from pm in matchedProducts
                                select new
                                {
                                  ProductID = pm.ProductID,
                                  VendorItemNumber = pm.Product.VendorItemNumber,
                                  VendorName = pm.Product.VendorAssortments.FirstOrDefault() != null ? pm.Product.VendorAssortments.FirstOrDefault().Vendor.Name : "",
                                  Primary = pm.Primary ? " Primary" : string.Empty
                                }).OrderByDescending(x => x.Primary).ToList();
          return Json(new { results = listOfProducts, productIDs = matchedProducts.Select(x => x.ProductID) });
        }
        else
        {
          var listOfProducts = (from pm in unit.Service<Product>().GetAll(x => x.ProductID == productID)
                                select new
                                {
                                  ProductID = pm.ProductID,
                                  VendorItemNumber = pm.VendorItemNumber,
                                  VendorName = pm.VendorAssortments.FirstOrDefault() != null ? pm.VendorAssortments.FirstOrDefault().Vendor.Name : "",
                                  Primary = string.Empty
                                }).OrderByDescending(x => x.Primary).ToList();
          return Json(new { results = listOfProducts, productIDs = productID });
        }
      }
    }


    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetAllAttributesInfoForAllMatchingProducts(string productIDs, int productID, string AttributeName)
    {
      int userLanguage = Client.User.LanguageID;

      bool filterOnAttributeName = !string.IsNullOrEmpty(AttributeName);

      var trimSpacesAttributeValues = ConfigurationManager.AppSettings["TrimSpacesAttributeValues"].ParseToBool();

      using (var unit = GetUnitOfWork())
      {

        List<int> matchedProductIDs = StringToIntList(productIDs).ToList();

        var dynamicAttributeData = new List<dynamic>();

        if (matchedProductIDs.Count > 1)
        {
          var productattributeQuery = string.Format(@"SELECT
                          pm2.ProductID,
                          pav.AttributeID,
                          pav.AttributeValueID,
                          pav.Value,
                          pam.MaxLength,
                          pan.Name
                          FROM ProductMatch pm
                          INNER JOIN ProductMatch pm2 on pm.ProductMatchID = pm2.ProductMatchID
                          INNER JOIN ProductAttributeValue pav on pm2.ProductID = pav.ProductID and pav.LanguageID = {0}
                          INNER JOIN ProductAttributeMetaData pam on pam.AttributeID = pav.AttributeID
                          INNER JOIN ProductAttributeName pan on pav.AttributeID = pan.AttributeID and pav.LanguageID = {0}
                          WHERE pm.ProductID = {1}", Client.User.LanguageID, matchedProductIDs[0]);

          var attributeValues = unit.ExecuteStoreQuery<MasterGroupMappingProductMatchAttribute>(productattributeQuery).ToList();


          //var attributeValues = unit.Service<ProductAttributeValue>().GetAll(x => matchedProductIDs.Contains(x.ProductID) && x.LanguageID == userLanguage).ToList();

          var attributeIDs = attributeValues.Select(x => x.AttributeID).Distinct().ToList();

          foreach (var attID in attributeIDs)
          {
            var sd = new ExpandoObject();

            var model = sd as IDictionary<String, object>;

            string attributeName = "No Name";
            int attributeValueID = -1;
            var attribute = attributeValues.FirstOrDefault(x => x.AttributeID == attID);
            if (attribute != null)
            {
              attributeName = attribute.Name;
              attributeValueID = attribute.AttributeValueID;
            }

            if (filterOnAttributeName)
            {
              if (!attributeName.Contains(AttributeName))
              {
                continue;
              }
            }
            model["AttributeName"] = attributeName;
            model["AttributeID"] = attID;
            model["AttributeValueID"] = attributeValueID;
            model["MaxInputLength"] = attribute.MaxLength;

            var attValues = attributeValues.FindAll(x => x.AttributeID == attID);

            string attributeValue = "";
            bool first = true;
            bool isMatched = true;
            foreach (var attValue in attValues)
            {
              if (first)
              {
                attributeValue = attValue.Value;
                first = false;
              }
              else
              {
                if (attributeValue != attValue.Value)
                {
                  isMatched = false;
                }
              }

              model[attValue.ProductID.ToString()] = (trimSpacesAttributeValues.HasValue && trimSpacesAttributeValues.Value)
                                                       ? attValue.Value.Trim()
                                                       : attValue.Value;
            }

            model["Matched"] = isMatched ? "Matched" : "Not Matched";

            dynamicAttributeData.Add(model);
          }
        }
        else
        {
          var productattributeQuery = string.Format(@"select  
pav.productid,
pav.attributeid,
pav.attributevalueid,
pav.value,
pam.MaxLength,
pan.name
from Productattributevalue pav 
inner join ProductAttributeMetaData pam on pam.AttributeID = pav.AttributeID
inner join productattributename pan on pav.attributeid = pan.attributeid and pav.languageid = {0}
where pav.productid = {1} and pav.languageid = {0}", Client.User.LanguageID, productID);

          var attributeValues = unit.ExecuteStoreQuery<MasterGroupMappingProductMatchAttribute>(productattributeQuery).ToList();


          //var attributeValues = unit.Service<ProductAttributeValue>().GetAll(x => matchedProductIDs.Contains(x.ProductID) && x.LanguageID == userLanguage).ToList();

          var attributeIDs = attributeValues.Select(x => x.AttributeID).Distinct().ToList();

          foreach (var attID in attributeIDs)
          {
            var sd = new ExpandoObject();

            var model = sd as IDictionary<String, object>;

            string attributeName = "No Name";
            int attributeValueID = -1;
            var attribute = attributeValues.FirstOrDefault(x => x.AttributeID == attID);
            if (attribute != null)
            {
              attributeName = attribute.Name;
              attributeValueID = attribute.AttributeValueID;
            }

            if (filterOnAttributeName)
            {
              if (!attributeName.Contains(AttributeName))
              {
                continue;
              }
            }
            model["AttributeName"] = attributeName;
            model["AttributeID"] = attID;
            model["AttributeValueID"] = attributeValueID;
            model["MaxInputLength"] = attribute.MaxLength;

            var attValues = attributeValues.FindAll(x => x.AttributeID == attID);

            string attributeValue = "";
            bool first = true;
            bool isMatched = true;
            foreach (var attValue in attValues)
            {
              if (first)
              {
                attributeValue = attValue.Value;
                first = false;
              }
              else
              {
                if (attributeValue != attValue.Value)
                {
                  isMatched = false;
                }
              }

              model[attValue.ProductID.ToString()] = (trimSpacesAttributeValues.HasValue && trimSpacesAttributeValues.Value)
                                                       ? attValue.Value.Trim()
                                                       : attValue.Value;
            }

            model["Matched"] = isMatched ? "Matched" : "Not Matched";

            dynamicAttributeData.Add(model);
          }
        }
        dynamicAttributeData = dynamicAttributeData.OrderByDescending(x => x.Matched).ToList();
        var serializer = new JavaScriptSerializer();
        serializer.RegisterConverters(new JavaScriptConverter[] { new ExpandoJSONConverter() });
        var json = serializer.Serialize(dynamicAttributeData);

        return Content("{results: " + json + ", total: " + dynamicAttributeData.Count + " }", "application/json");
      }

    }


    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateAttributesForMatchingProducts(int productID, int attributeID, string attributeValue)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          ProductAttributeValue productAttributeValue = unit.Service<ProductAttributeValue>().Get(x => x.ProductID == productID && x.AttributeID == attributeID && x.LanguageID == Client.User.LanguageID);

          if (productAttributeValue == null)
          {
            productAttributeValue = new ProductAttributeValue()
            {
              LanguageID = Client.User.LanguageID,
              AttributeID = attributeID,
              ProductID = productID,
              Value = attributeValue,
              CreationTime = DateTime.Now.ToUniversalTime(),
              CreatedBy = Client.User.UserID
            };

            unit.Service<ProductAttributeValue>().Create(productAttributeValue);

          }
          else
          {
            var attributeOptions = unit.Service<ProductAttributeOption>().GetAll(c => c.AttributeID == attributeID);
            if (attributeOptions.Count() > 1 && attributeOptions.Where(c => c.AttributeOption == attributeValue).Count() == 0)
            {
              return Failure("This is not a valid option");
            }
            productAttributeValue.Value = attributeValue;
          }

          unit.Save();

        }
        return Success("Update successfull");
      }

      catch (Exception e)
      {
        return Failure("Could not update product attribute value. Error: " + e.Message);

      }

    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult CheckFinished(int MasterGroupmappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var newStatus = (int)MatchStatuses.New;

          //Get all products with "new" matches
          var matchedProducts = (from pm in unit.Scope.Repository<ProductMatch>().GetAll()
                                 join mp in unit.Scope.Repository<MasterGroupMappingProduct>().GetAll(mgmp => mgmp.IsProductMapped) on pm.ProductID equals mp.ProductID
                                 join va in unit.Scope.Repository<VendorAssortment>().GetAll(x => x.IsActive) on mp.ProductID equals va.ProductID
                                 join pm2 in unit.Scope.Repository<ProductMatch>().GetAll() on pm.ProductMatchID equals pm2.ProductMatchID
                                 where mp.MasterGroupMappingID == MasterGroupmappingID && pm2.MatchStatus == newStatus
                                 select mp).ToList();

          //Select all primary a match
          var primarys = (from pm in unit.Scope.Repository<ProductMatch>().GetAll()
                          join mp in unit.Scope.Repository<MasterGroupMappingProduct>().GetAll() on pm.ProductID equals mp.ProductID
                          join va in unit.Scope.Repository<VendorAssortment>().GetAll(x => x.IsActive) on mp.ProductID equals va.ProductID
                          join pm2 in unit.Scope.Repository<ProductMatch>().GetAll() on pm.ProductMatchID equals pm2.ProductMatchID
                          where mp.MasterGroupMappingID == MasterGroupmappingID && mp.IsProductMapped && pm2.isMatched
                          group pm2 by pm2.ProductMatchID into grouped
                          select new
                          {
                            matchID = grouped.Key,
                            primarySet = grouped.Any(x => x.Primary == true),
                            products = grouped.Select(x => x.ProductID)
                          }).ToList();

          //For the single products check the step
          var productAttrMgms = (from pav in unit.Scope.Repository<ProductAttributeValue>().GetAll(x => x.ProductAttributeMetaData.AttributeCode == "MGMAttributeCheck")
                                 join mp in unit.Scope.Repository<MasterGroupMappingProduct>().GetAll(x => x.IsProductMapped) on pav.ProductID equals mp.ProductID
                                 join va in unit.Scope.Repository<VendorAssortment>().GetAll(x => x.IsActive) on mp.ProductID equals va.ProductID
                                 join pm in unit.Scope.Repository<ProductMatch>().GetAll() on mp.ProductID equals pm.ProductID into lf
                                 from left in lf.DefaultIfEmpty()
                                 where mp.MasterGroupMappingID == MasterGroupmappingID && pav.Value == "Step 2 - Attribute"
                                  && (left.ProductMatchID == null || ((left.isMatched && left.Primary) || !left.isMatched))
                                 select mp).ToList();

          if (matchedProducts.Count() == 0 && productAttrMgms.Count() == 0 && !primarys.Any(x => x.primarySet == false))
            return Success("Check complete", isMultipartRequest: true);

          if (productAttrMgms.Count() > 0)
          {
            productAttrMgms.ForEach(mgm =>
            {
              if (!primarys.Any(x => x.products.Any(y => y == mgm.ProductID))
                && matchedProducts.Count() == 0)
                mgm.IsApproved = false;
            });

            unit.Save();
          }

          if (matchedProducts.Count() > 0)
          {
            matchedProducts.ForEach(mp =>
            {
              mp.IsApproved = false;
            });
            unit.Save();

            return Success("Check matches", isMultipartRequest: true);
          }

          if (primarys.Any(x => x.primarySet == false))
          {
            primarys.Where(x => x.primarySet == false).ForEach(check =>
            {
              var mgms = unit.ExecuteStoreQuery<MasterGroupMappingProduct>(
                  string.Format(@"select mgmp.* from productmatch pm
                inner join MasterGroupMappingProduct mgmp on pm.ProductID = mgmp.ProductID
                where pm.isMatched = 1 
                and mgmp.MasterGroupMappingID = {0}
                and pm.ProductMatchID = {1}
                and mgmp.IsApproved = 1", MasterGroupmappingID, check.matchID)).ToList();

              mgms.ForEach(mgm =>
              {
                unit.ExecuteStoreCommand("update mastergroupmappingproduct set IsApproved = 0 where mastergroupmappingid = {0} and productid = {1}", MasterGroupmappingID, mgm.ProductID);
              });
            });

            return Success("Check attributes and primay's", isMultipartRequest: true);
          }

          primarys.Where(x => x.primarySet == true).ForEach(check =>
          {
            var mgms = unit.ExecuteStoreQuery<MasterGroupMappingProduct>(
                string.Format(@"select distinct mgmp.* from productmatch pm
                              inner join MasterGroupMappingProduct mgmp on pm.ProductID = mgmp.ProductID
                              left join productattributevalue pav on pav.productid = mgmp.productid 
					                      and pav.attributeid in ( 
						                      select attributeid from productattributemetadata pamd where pamd.attributecode = 'MGMAttributeCheck'
					                      )
                              where pm.isMatched = 1 
                              and mgmp.MasterGroupMappingID = {0}
                              and pm.ProductMatchID = {1}
                              and mgmp.IsApproved = 1
                              and (pav.attributeid is null or pav.value not like 'Step 3%')", MasterGroupmappingID, check.matchID)).ToList();

            mgms.ForEach(mgm =>
            {
              unit.ExecuteStoreCommand("update mastergroupmappingproduct set IsApproved = 0 where mastergroupmappingid = {0} and productid = {1}", MasterGroupmappingID, mgm.ProductID);
            });
          });

          return Success("Second round to check attributes", isMultipartRequest: true);
        }
        catch
        {
          return Failure("Something went wrong", isMultipartRequest: true);
        }
      }
    }
    #endregion

    #region Menu "Assign User"
    [RequiresAuthentication(Functionalities.AssignUserToMasterGroupMapping)]
    public ActionResult GetAssignedUsers(int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var selectedMasterGroupMapping = unit.Scope
          .Repository<MasterGroupMapping>()
          .Include(masterGroupMapping => masterGroupMapping.Users)
          .GetSingle(x => x.MasterGroupMappingID == MasterGroupMappingID);

        if (selectedMasterGroupMapping != null)
        {
          var userList = selectedMasterGroupMapping
            .Users
            .Select(user => new
            {
              MasterGroupMappingID,
              user.UserID,
              Name = user.Firstname + ' ' + user.Lastname
            })
            .ToList();

          return Json(new
          {
            results = userList
          });
        }
        else
        {
          return Failure("The root master group mapping cannot have any users assigned.");
        }
      }

    }

    [RequiresAuthentication(Functionalities.AssignUserToMasterGroupMapping)]
    public ActionResult AssignUserTo(int masterGroupMappingID, int userID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          AssignUserTo(masterGroupMappingID, userID, unit);

        }
        return Success(string.Format("User successfully assigned to MasterGroupMapping: Note : Assign User to Role {0} if needed!"
          , MasterGroupMappingConstants.MasterGroupMappingAssigneeRoleName)
          , needsRefresh: false);
      }
      catch (Exception e)
      {
        return Failure("Failed to assign user ", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.AssignUserToMasterGroupMapping)]
    public ActionResult UnassignedUserFrom(int _masterGroupMappingID, int _userID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var manager = new MasterGroupMappingUserAssignmentManager(
            unit.Service<Role>(),
            unit.Service<MasterGroupMapping>(),
            unit.Service<User>(),
            unit.Service<FunctionalityRole>()
          );


          var childMasterGroupMappingHierarchy = GetListOfMasterGroupMappingHierarchy(_masterGroupMappingID);

          var masterGroupMappingIDs = childMasterGroupMappingHierarchy.Select(x => x.MasterGroupMappingID).ToList();

          foreach (var id in masterGroupMappingIDs)
          {
            manager.Unassign(id, _userID);
          }

          unit.Save();
        }
        return Success("User successfully unassigned", needsRefresh: false);
      }
      catch (Exception e)
      {
        return Failure("Failed to unassign user ", e, isMultipartRequest: true);
      }

    }
    #endregion

    #region Menu "Attribute Management"
    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult GetListOfMatchedMasterGroupMappingAttributes(int MasterGroupMappingID)
    {
      return List(unit =>
        unit.Service<ProductAttributeMetaData>()
        .GetAll(x => x.MasterGroupMappings.Any(m => m.MasterGroupMappingID == MasterGroupMappingID))
        .Select(x => new
        {
          MasterGroupMappingID,
          x.AttributeID,
          AttributeName = x.ProductAttributeNames.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID).Name,
          AttributeGroupName = x.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID).Name,
          VendorName = x.Vendor.Name,
          x.DefaultValue
        })
      );
    }

    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult AddAttributeToMasterGroupMapping(int AttributeID, int MasterGroupMappingID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var productAttribute = unit.Service<ProductAttributeMetaData>()
            .Get(x => x.AttributeID == AttributeID);
          var masterGroupMapping = unit.Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
          masterGroupMapping.ProductAttributeMetaDatas.Add(productAttribute);
          unit.Save();
        }
        return Success("Successfully created object", isMultipartRequest: true, needsRefresh: false);
      }
      catch (Exception e)
      {
        return Failure("Failed to create object ", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult DeleteAttributeFromMasterGroupMapping(int AttributeID, int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (AttributeID < 0 || MasterGroupMappingID < 0)
            return Failure("Failed to delete attribute");
          else
          {
            var productAttribute = unit.Service<ProductAttributeMetaData>()
              .Get(x => x.AttributeID == AttributeID);
            var masterGroupMapping = unit.Service<MasterGroupMapping>()
              .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
            masterGroupMapping.ProductAttributeMetaDatas.Remove(productAttribute);
          }
          unit.Save();
          return Success("Successfully delete attribute");
        }
        catch (Exception ex)
        {
          return Failure("Failed to delete attribute: " + ex.Message);
        }
      }
    }

    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult CopyAttributeFromMasterGroupMapping(string ListOfMasterGroupMappingIDsJson)
    {
      var ListOfIDsModel = new
      {
        CopyFromMasterGroupMappingID = 0,
        CopyToMasterGroupMappingIDs = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfMasterGroupMappingIDsJson, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var copyFromMasterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == ListOfIDs.CopyFromMasterGroupMappingID);
          foreach (int MasterGroupMappingID in ListOfIDs.CopyToMasterGroupMappingIDs)
          {
            var copyToMasterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
            copyFromMasterGroupMapping.ProductAttributeMetaDatas.ToList().ForEach(x =>
            {
              copyToMasterGroupMapping.ProductAttributeMetaDatas.Add(x);
            });
          }
          unit.Save();
        }
        return Success("Attributes are successfully copied.");
      }
      catch (Exception e)
      {
        return Failure("Failed to copy the attributes. ", e);
      }
    }

    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult UpdateMatchedProductsPerAttribtue(int _AttributeValueID, int _AttributeID, int _ConcentratorNumber, string AttributeValue)
    {
      if (_AttributeValueID > 0)
      {
        return Update<ProductAttributeValue>(pav => pav.AttributeValueID == _AttributeValueID, (unit, pav) =>
        {
          pav.Value = AttributeValue;
        });
      }
      else
      {
        ProductAttributeValue productAttributeValue = new ProductAttributeValue()
        {
          AttributeID = _AttributeID,
          ProductID = _ConcentratorNumber,
          Value = AttributeValue,
          LanguageID = Client.User.LanguageID,
          CreatedBy = Client.User.UserID,
          CreationTime = DateTime.Now.ToUniversalTime()
        };


        try
        {
          using (var unit = GetUnitOfWork())
          {
            unit.Service<ProductAttributeValue>().Create(productAttributeValue);
            unit.Save();
          }
          return Success("Attribute is successfully updated.");
        }
        catch (Exception e)
        {
          return Failure("Failed to update the attribute. ", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult GetListOfMatchedProductsPerAttribtue(int MasterGroupMappingID, int AttributeID)
    {
      using (var unit = GetUnitOfWork())
      {
        var languageID = Client.User.LanguageID;
        var languageCode = unit.Service<Language>().Get(x => x.LanguageID == languageID).DisplayCode;

        var query = string.Format(@";
          WITH ProductsInMasterGroupMapping
          AS (
	          SELECT mp.ProductID
		          ,va.ShortDescription
		          ,b.NAME
		          ,p.VendorItemNumber
	          FROM dbo.MasterGroupMapping m
	          INNER JOIN dbo.MasterGroupMappingProduct mp ON m.MasterGroupMappingID = mp.MasterGroupMappingID
		          AND IsProductMapped = 1
	          INNER JOIN dbo.Product p ON mp.ProductID = p.ProductID
	          INNER JOIN dbo.Brand b ON p.BrandID = b.BrandID
	          INNER JOIN dbo.VendorAssortment va ON va.VendorAssortmentID = (
			          SELECT TOP 1 VendorAssortmentID
			          FROM dbo.VendorAssortment
			          WHERE ProductID = p.ProductID
			          )
	          WHERE m.MasterGroupMappingID = {0}
	          )
	          ,ProductAttribute
          AS (
	          SELECT pim.ProductID
		          ,AttributeValueID
		          ,AttributeID
		          ,Value
	          FROM ProductsInMasterGroupMapping pim
	          INNER JOIN dbo.ProductAttributeValue pav ON pav.ProductID = pim.ProductID
	          WHERE LanguageID = {1}
		          AND AttributeID = {2}
	          )
          SELECT pim.ProductID
	          ,ShortDescription
	          ,NAME BrandName
	          ,VendorItemNumber
	          ,AttributeValueID
	          ,AttributeID
	          ,Value
          FROM ProductsInMasterGroupMapping pim
          LEFT JOIN ProductAttribute pa ON pim.ProductID = pa.ProductID
          ORDER BY pim.ProductID
        ", MasterGroupMappingID, languageID, AttributeID);

        var listOfProducts =
          (from productAttribute in unit.ExecuteStoreQuery<ProductAttributeValueModel>(query).ToList()
           select new
             {
               ConcentratorNumber = productAttribute.ProductID,
               ProductName = productAttribute.ShortDescription,
               Brand = productAttribute.BrandName,
               productAttribute.VendorItemNumber,
               AttributeValue = productAttribute.Value,
               productAttribute.AttributeValueID,
               AttributeID,
               LanguageCode = languageCode
             });
        return List(listOfProducts.AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.MasterGroupMappingAttributeManagement)]
    public ActionResult GetListOfAllMasterGroupMappingNames(int MasterGroupMappingID, int? ConnectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var repository = unit.Scope
          .Repository<MasterGroupMapping>()
          .Include(mgm => mgm.MasterGroupMappingLanguages)
          .Include(mgm => mgm.MasterGroupMappingParent);

        var listOfMatchedMasterGroupMappings = repository
          .GetAll(x => x.MasterGroupMappingID != MasterGroupMappingID)
          .Where(x => (ConnectorID.HasValue && ConnectorID.Value > 0) ? x.ConnectorID == ConnectorID : x.ConnectorID == null)
          .ToList()
          .Select(x =>
          {
            var masterGroupMappingID = x.MasterGroupMappingID;
            var hasParent = true;
            var masterGroupMappingPadName = "";

            while (hasParent)
            {
              var masterGroupmapping = repository.GetSingle(m => m.MasterGroupMappingID == masterGroupMappingID);

              var tempMasterGroupMappingPadName = masterGroupmapping
                .MasterGroupMappingLanguages
                .Where(c => c.LanguageID == Client.User.LanguageID)
                .Select(c => c.Name)
                .FirstOrDefault();

              masterGroupMappingPadName.Insert(0, tempMasterGroupMappingPadName == null
                ? "Master group mapping name is empty!"
                : tempMasterGroupMappingPadName);

              if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
              {
                masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
                masterGroupMappingPadName = "->" + masterGroupMappingPadName;
              }
              else
              {
                hasParent = false;
              }
            }

            return new
            {
              MasterGroupMappingID = x.MasterGroupMappingID,
              Check = false,
              MasterGroupMappingPad = masterGroupMappingPadName
            };
          })
          .OrderBy(o => o.MasterGroupMappingPad);

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }
    #endregion

    #region Menu "Cross Reference Management"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfCrossReferences(int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var list = unit
          .Service<MasterGroupMapping>()
          .GetAll(x => x.MasterGroupMappings.Any(y => y.MasterGroupMappingID == MasterGroupMappingID))
          .ToList()
          .Select(x => new
          {
            CrossReferenceID = x.MasterGroupMappingID,
            CrossReferenceName = x.MasterGroupMappingLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
            CrossReferencePath = GetMasterGroupMappingPath(x.MasterGroupMappingID, Client.User.LanguageID, unit),
            CountProducts = countProductsInMasterGroupMapping(x.MasterGroupMappingID, unit)
          });

        return Json(new { results = list });
      }
    }

    // query review
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMasterGroupMappingAttributes(int MasterGroupMappingID)
    {
      return List(unit =>
        unit
          .Service<Product>()
          .GetAll(x => x.MasterGroupMappingProducts.Any(y => y.MasterGroupMappingID == MasterGroupMappingID))
          .Select(x => new { product = x, x.ProductID })
          .Select(x => x.product.ProductAttributeValues.Select(y => new { Attribute = y.ProductAttributeMetaData }))
          .SelectMany(x => x)
          .GroupBy(x => x.Attribute.AttributeID)
          .Select(x => new { AttributeID = x.Key, Attribute = x.FirstOrDefault() })
          .Select(x => new
          {
            AttributeID = x.Attribute.Attribute.AttributeID,
            AttributeName = x.Attribute.Attribute.ProductAttributeNames.Where(ag => ag.LanguageID == Client.User.LanguageID).Select(y => y.Name).FirstOrDefault(),
            AttributeGroupName = x.Attribute.Attribute.ProductAttributeGroupMetaData.ProductAttributeGroupNames.Where(ag => ag.LanguageID == Client.User.LanguageID).Select(y => y.Name).FirstOrDefault(),
            FormatString = x.Attribute.Attribute.Sign,
            IsVisible = x.Attribute.Attribute.IsVisible,
            IsSearchable = x.Attribute.Attribute.IsSearchable,
            MasterGroupMappingID
          })
          .OrderBy(x => x.AttributeGroupName)
      );
    }

    // query review
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfCrossReferenceAttributes(int MasterGroupMappingID, int? CrossReferenceID)
    {
      using (var unit = GetUnitOfWork())
      {
        ArrayList listOfAttributes = new ArrayList();

        IEnumerable<MasterGroupMapping> listOfCrossReferences = new List<MasterGroupMapping>();

        if (CrossReferenceID.HasValue && CrossReferenceID.Value > 0)
        {
          listOfCrossReferences = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MasterGroupMappingID)
            .MasterGroupMappingCrossReferences
            .Where(x => x.MasterGroupMappingID == CrossReferenceID.Value)
            ;
        }
        else
        {
          listOfCrossReferences = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MasterGroupMappingID)
            .MasterGroupMappingCrossReferences;
        }


        listOfCrossReferences.ForEach(cr =>
        {
          var list = unit
            .Service<Product>()
            .GetAll(x => x.MasterGroupMappingProducts.Any(y => y.MasterGroupMappingID == cr.MasterGroupMappingID))
            .Select(x => new { product = x, x.ProductID })
            .Select(x => x.product.ProductAttributeValues.Select(y => new { Attribute = y.ProductAttributeMetaData }))
            .SelectMany(x => x)
            .GroupBy(x => x.Attribute.AttributeID)
            .Select(x => new { AttributeID = x.Key, Attribute = x.FirstOrDefault() })
            .Select(x => new
            {
              AttributeID = x.Attribute.Attribute.AttributeID,
              AttributeName = x.Attribute.Attribute.ProductAttributeNames.Where(ag => ag.LanguageID == Client.User.LanguageID).Select(y => y.Name).FirstOrDefault(),
              AttributeGroupName = x.Attribute.Attribute.ProductAttributeGroupMetaData.ProductAttributeGroupNames.Where(ag => ag.LanguageID == Client.User.LanguageID).Select(y => y.Name).FirstOrDefault(),
              FormatString = x.Attribute.Attribute.Sign,
              IsVisible = x.Attribute.Attribute.IsVisible,
              IsSearchable = x.Attribute.Attribute.IsSearchable,
              cr.MasterGroupMappingID
            })
            .OrderBy(x => x.AttributeGroupName)
            ;
          listOfAttributes.AddRange(list.ToArray());
        });
        return Json(new { results = listOfAttributes });
      }
    }

    // query review
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfRelatedAttributes(int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        ArrayList listOfAttributes = new ArrayList();

        var list = unit
          .Service<MasterGroupMappingRelatedAttribute>()
          .GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID && x.ParentID == null);

        list.ForEach(x =>
        {
          ArrayList listOfCrossReferencesNames = new ArrayList();
          if (x.CrossReferenceRelatedAttribute.Count > 0)
          {
            x.CrossReferenceRelatedAttribute.ForEach(cr =>
            {
              var crossReferenceName = cr
                .ProductAttributeMetaData
                .ProductAttributeNames
                .Where(y => y.LanguageID == Client.User.LanguageID)
                .Select(m => m.Name)
                .FirstOrDefault();

              var crossReferenceValue = cr
                .AttributeValue;

              var crossReferenceNameValue = crossReferenceName +
                " (" +
                crossReferenceValue +
                ")";

              listOfCrossReferencesNames.Add(crossReferenceNameValue);
            });
          }
          var item = new
          {
            RelatedAttributeID = x.RelatedAttributeID,
            AttributeID = x.AttributeID,
            AttributeName = x.ProductAttributeMetaData.ProductAttributeNames.Where(y => y.LanguageID == Client.User.LanguageID).Select(m => m.Name).FirstOrDefault(),
            AttributeValue = x.AttributeValue,
            CrossReferenceAttributeName = listOfCrossReferencesNames
          };
          listOfAttributes.Add(item);
        });

        return Json(new { results = listOfAttributes });
      }
    }

    // query review
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfCrossReferenceRelatedAttributes(int RelatedAttributeID)
    {
      using (var unit = GetUnitOfWork())
      {
        var RelatedAttribute = unit
          .Service<MasterGroupMappingRelatedAttribute>()
          .Get(x => x.RelatedAttributeID == RelatedAttributeID);

        if (RelatedAttribute != null)
        {
          var listOfRelatedAttributes = RelatedAttribute.CrossReferenceRelatedAttribute.Select(x => new
          {
            x.RelatedAttributeID,
            AttributeName = x.ProductAttributeMetaData.ProductAttributeNames.Where(y => y.LanguageID == Client.User.LanguageID).Select(m => m.Name).FirstOrDefault(),
            x.AttributeValue
          });
          return Json(new { results = listOfRelatedAttributes });
        }
        else
        {
          return Json(new { });
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfRelatedAttributeValues(string listOfIDsJson)
    {
      var listOfIDsModel = new
      {
        MasterGroupMappingID = 0,
        CrossReferenceID = 0,
        AttributeID = 0,
        MasterGroupMappingAttributeID = 0,
        CrossReferenceAttributeID = 0,
        IsForMasterGroupMappingGrid = false,
        IsForCrossReferenceGrid = false
      };
      var listOfIDs = JsonConvert.DeserializeAnonymousType(listOfIDsJson, listOfIDsModel);

      using (var unit = GetUnitOfWork())
      {
        List<string> listOfRelatedAttributeValues = new List<string>();

        if (listOfIDs.IsForMasterGroupMappingGrid)
        {
          listOfRelatedAttributeValues = unit
            .Service<MasterGroupMappingRelatedAttribute>()
            .GetAll(x =>
              x.MasterGroupMappingID == listOfIDs.MasterGroupMappingID &&
              x.AttributeID == listOfIDs.AttributeID &&
              x.CrossReferenceRelatedAttribute.Any(cr => cr.AttributeID == listOfIDs.CrossReferenceAttributeID)
            )
            .Select(x => x.AttributeValue)
            .Distinct()
            .ToList();
        }

        if (listOfIDs.IsForCrossReferenceGrid)
        {
          listOfRelatedAttributeValues = unit
            .Service<MasterGroupMappingRelatedAttribute>()
            .GetAll(x =>
              x.MasterGroupMappingID == listOfIDs.CrossReferenceID &&
              x.AttributeID == listOfIDs.AttributeID &&
              x.ParentRelatedAttribute.MasterGroupMappingID == listOfIDs.MasterGroupMappingID &&
              x.ParentRelatedAttribute.AttributeID == listOfIDs.MasterGroupMappingAttributeID
            )
            .Select(x => x.AttributeValue)
            .Distinct()
            .ToList();
        }


        var atttributeMGMListIDs = unit.Service<MasterGroupMappingRelatedAttribute>().GetAll(x => x.MasterGroupMappingID == listOfIDs.MasterGroupMappingID).Select(x => x.RelatedAttributeID);

        var matchedToAttributes = (from rec in unit.Service<MasterGroupMappingRelatedAttribute>().GetAll(x => x.MasterGroupMappingID == listOfIDs.MasterGroupMappingID)
                                   join r in unit.Service<MasterGroupMappingRelatedAttribute>().GetAll() on rec.RelatedAttributeID equals r.ParentID into recs
                                   select new
                                   {
                                     attributeValue = rec.AttributeValue,
                                     mappedToValues = recs
                                   });


        var listOfValues = unit
          .Service<ProductAttributeValue>()
          .GetAll(x => x.AttributeID == listOfIDs.AttributeID)
          .GroupBy(x => x.Value).ToList()
          .Select(x => new { ProductAttributeValue = x.FirstOrDefault() })
          .Select(x => new
          {
            IsAttributeMapped = listOfRelatedAttributeValues.Contains(x.ProductAttributeValue.Value),
            //x.related
            mappedToValue = matchedToAttributes.FirstOrDefault(y => y.attributeValue == x.ProductAttributeValue.Value) != null ?
            matchedToAttributes.FirstOrDefault(y => y.attributeValue == x.ProductAttributeValue.Value).mappedToValues.Select(z => new { z.AttributeValue, z.AttributeID, z.MasterGroupMappingID }).Where(o => o.MasterGroupMappingID == listOfIDs.CrossReferenceID) : null,

            x.ProductAttributeValue.Value,
            x.ProductAttributeValue.AttributeID,
            Selected = false
          })
          .OrderBy(x => x.Value)
          ;

        return Json(new { results = listOfValues });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult CreatRelatedAttribute(string ListOfAttributeValuesJson)
    {
      var ListOfAttributeValuesModel = new
      {
        MasterGroupMappingID = 0,
        MasterGroupMappingAttributeID = 0,
        MasterGroupMappingAttributeValues = new[] { "" },
        CrossReferenceID = 0,
        CrossReferenceAttributeID = 0,
        CrossReferenceAttributeValues = new[] { "" }
      };
      var ListOfAttributeValues = JsonConvert.DeserializeAnonymousType(ListOfAttributeValuesJson, ListOfAttributeValuesModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          ListOfAttributeValues.MasterGroupMappingAttributeValues.ForEach(masterGroupMappingValue =>
          {
            MasterGroupMappingRelatedAttribute masterGroupMappingRelatedAttribute = unit
              .Service<MasterGroupMappingRelatedAttribute>()
              .Get(ra =>
                ra.MasterGroupMappingID == ListOfAttributeValues.MasterGroupMappingID &&
                ra.ParentID == null &&
                ra.AttributeID == ListOfAttributeValues.MasterGroupMappingAttributeID &&
                ra.AttributeValue == masterGroupMappingValue
              );

            if (masterGroupMappingRelatedAttribute == null)
            {
              masterGroupMappingRelatedAttribute = new MasterGroupMappingRelatedAttribute()
              {
                MasterGroupMappingID = ListOfAttributeValues.MasterGroupMappingID,
                AttributeID = ListOfAttributeValues.MasterGroupMappingAttributeID,
                AttributeValue = masterGroupMappingValue
              };
            }

            ListOfAttributeValues.CrossReferenceAttributeValues.ForEach(crossReferenceValue =>
            {
              MasterGroupMappingRelatedAttribute crRelatedAttribute = new MasterGroupMappingRelatedAttribute()
              {
                MasterGroupMappingID = ListOfAttributeValues.CrossReferenceID,
                AttributeID = ListOfAttributeValues.CrossReferenceAttributeID,
                ParentRelatedAttribute = masterGroupMappingRelatedAttribute,
                AttributeValue = crossReferenceValue
              };

              unit.Service<MasterGroupMappingRelatedAttribute>().Create(crRelatedAttribute);
            });

          });
          unit.Save();
        }
        return Success("Relation is successfully created.");
      }
      catch (Exception e)
      {
        return Failure("Failed to creat the relation. ", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult DeleteCrossReferenceRelatedAttribute(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (id < 0)
            return Failure("Failed to delete related attribute");
          else
          {
            MasterGroupMappingRelatedAttribute masterGroupMappingRelatedAttribute = unit
              .Service<MasterGroupMappingRelatedAttribute>()
              .Get(x => x.RelatedAttributeID == id);

            if (masterGroupMappingRelatedAttribute.ParentRelatedAttribute.CrossReferenceRelatedAttribute.Count == 1)
            {
              unit.Service<MasterGroupMappingRelatedAttribute>().Delete(masterGroupMappingRelatedAttribute.ParentRelatedAttribute);
            }
            unit.Service<MasterGroupMappingRelatedAttribute>().Delete(masterGroupMappingRelatedAttribute);
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

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult DeleteRelatedAttributeMapping(int RelatedAttributeID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (RelatedAttributeID < 0)
            return Failure("Failed to delete related attribute mapping");
          else
          {
            MasterGroupMappingRelatedAttribute masterGroupMappingRelatedAttribute = unit
              .Service<MasterGroupMappingRelatedAttribute>()
              .Get(x => x.RelatedAttributeID == RelatedAttributeID);

            unit.Service<MasterGroupMappingRelatedAttribute>().Delete(x => x.ParentID == masterGroupMappingRelatedAttribute.RelatedAttributeID);

            unit.Save();

            unit.Service<MasterGroupMappingRelatedAttribute>().Delete(masterGroupMappingRelatedAttribute);
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

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnCrossReferenceAllOfMasterGroupMapping(int MasterGroupMappingID, bool? Delete)
    {
      try
      {
        using (var t = new TransactionScope())
        using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
        {
          var query = @"
DECLARE @@id int = @0
;WITH Children as (
	SELECT MasterGroupMappingID FROM MasterGroupMapping WHERE ParentMasterGroupMappingID = @@id

	UNION ALL 

	SELECT m.MasterGroupMappingID FROM MasterGroupMapping m
	INNER JOIN Children c ON m.ParentMasterGroupMappingID = c.MasterGroupMappingID
)
Update MasterGroupMapping SET SourceMasterGroupMappingID = null WHERE SourceMasterGroupMappingID IN (SELECT MasterGroupMappingID FROM Children)
Update MasterGroupMapping SET SourceMasterGroupMappingID = null WHERE SourceMasterGroupMappingID = @@id

;WITH Children as (
	SELECT MasterGroupMappingID FROM MasterGroupMapping WHERE ParentMasterGroupMappingID = @@id

	UNION ALL 

	SELECT m.MasterGroupMappingID FROM MasterGroupMapping m
	INNER JOIN Children c ON m.ParentMasterGroupMappingID = c.MasterGroupMappingID
)
DELETE FROM MasterGroupMapping WHERE MasterGroupMappingID IN (SELECT MasterGroupMappingID FROM Children) 

;WITH Children as (
	SELECT MasterGroupMappingID FROM MasterGroupMapping WHERE ParentMasterGroupMappingID = @@id

	UNION ALL 

	SELECT m.MasterGroupMappingID FROM MasterGroupMapping m
	INNER JOIN Children c ON m.ParentMasterGroupMappingID = c.MasterGroupMappingID
)
DELETE FROM MasterGroupMappingCrossReference WHERE CrossReferenceID IN (SELECT MasterGroupMappingID FROM Children) 
DELETE FROM MasterGroupMappingCrossReference WHERE CrossReferenceID = @@id

DELETE FROM MasterGroupMapping WHERE MasterGroupMappingID = @@id";

          db.Execute(query, MasterGroupMappingID);

          t.Complete();
        }

        var message = "References are successfully uncrossed";
        if (Delete.HasValue && Delete.Value)
          message += " and Master Group Mapping successfully deleted.";

        return Success(message);
      }
      catch (Exception e)
      {
        return Failure("Failed to uncross references. ", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnCrossReference(string MgmIDAndCrIDJson)
    {
      var MgmIDAndCrIDModel = new { MasterGroupMappingID = 0, CrossReferenceID = 0 };
      var MgmIDAndCrID = JsonConvert.DeserializeAnonymousType(MgmIDAndCrIDJson, MgmIDAndCrIDModel);
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var masterGroupMapping = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MgmIDAndCrID.MasterGroupMappingID);
          var crossReference = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MgmIDAndCrID.CrossReferenceID);

          DeleteRelatedAttributes(MgmIDAndCrID.MasterGroupMappingID, MgmIDAndCrID.CrossReferenceID, unit);

          masterGroupMapping.MasterGroupMappingCrossReferences.Remove(crossReference);

          unit.Save();
        }
        return Success("Reference is successfully uncrossed.");
      }
      catch (Exception e)
      {
        return Failure("Failed to uncross the reference. ", e);
      }
    }

    #endregion

    #region Menu "Rename" & "Language Wizard"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetMasterGroupMappingTranslations(int? masterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfTranslations =
          from language in unit.Service<Language>().GetAll()
          join mgmLanguage in unit
            .Service<MasterGroupMappingLanguage>()
            .GetAll(x => masterGroupMappingID.HasValue ? x.MasterGroupMappingID == masterGroupMappingID.Value : true) on language.LanguageID equals mgmLanguage.LanguageID into mgmName
          from masterGroupMappingName in mgmName.DefaultIfEmpty()
          select new
          {
            language.LanguageID,
            Language = language.Name,
            masterGroupMappingName.Name,
            MasterGroupMappingID = masterGroupMappingName == null ? masterGroupMappingID : masterGroupMappingName.MasterGroupMappingID,
            LanguageMustBeFilled = string.IsNullOrEmpty(masterGroupMappingName.Name) ? false : true
          };

        return Json(new { results = listOfTranslations });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetMasterGroupMappingLanguages(int? masterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfTranslations = (
          from language in unit.Service<Language>().GetAll()
          join mgmLanguage in unit
            .Service<MasterGroupMappingLanguage>()
            .GetAll(x => masterGroupMappingID.HasValue ? x.MasterGroupMappingID == masterGroupMappingID.Value : true) on language.LanguageID equals mgmLanguage.LanguageID into mgmName
          from masterGroupMappingName in mgmName.DefaultIfEmpty()
          select new
          {
            language.LanguageID,
            LanguageName = language.Name,
            LanguageValue = masterGroupMappingName.Name,
            MasterGroupMappingID = masterGroupMappingName == null ? masterGroupMappingID : masterGroupMappingName.MasterGroupMappingID,
            LanguageMustBeFilled = string.IsNullOrEmpty(masterGroupMappingName.Name) ? false : true
          })
          .OrderBy(x => x.LanguageName)
          ;

        return Json(new { results = listOfTranslations });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateMasterGroupMapping)]
    public ActionResult UpdateMasterGroupMappingLanguage(string ListOfLanguageIDsAndValuesJson)
    {
      var ListOfLanguageIDsAndValuesModel = new
      {
        LanguageIDs = new[] { new { LanguageID = 0, Value = "", LanguageMustBeFilled = false } },
        MasterGroupMappingID = 0
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfLanguageIDsAndValuesJson, ListOfLanguageIDsAndValuesModel);
      using (var unit = GetUnitOfWork())
      {
        var listOfAllFilledLanguagesWithOutSelected = ListOfIDs
          .LanguageIDs
          .Where(x => x.LanguageMustBeFilled == false)
          .ToList();

        if (listOfAllFilledLanguagesWithOutSelected.Count > 0)
        {
          StringBuilder message = new StringBuilder();
          message.Append("Not all records are correctly filled. The next languages are filled but not selected:");
          message.Append("<br><br>");
          listOfAllFilledLanguagesWithOutSelected.ForEach(x =>
          {
            var language = unit
              .Service<Language>()
              .Get(l => l.LanguageID == x.LanguageID)
              .Name;

            message.AppendFormat("Language: <b>{0}</b> ({1})", language, x.Value).Append("<br>");
          });
          return Failure(message.ToString());
        }
        else
        {
          var listOfAllFilledSelectedLanguages = ListOfIDs
            .LanguageIDs
            .Where(x => x.LanguageMustBeFilled)
            .ToList();

          MasterGroupMapping masterGroupMapping = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == ListOfIDs.MasterGroupMappingID);

          if (masterGroupMapping == null)
          {
            StringBuilder message = new StringBuilder();
            message.Append("Faild to rename Master Group Mapping. Master Group Mapping does not exist.");
            return Failure(message.ToString());
          }
          else
          {
            listOfAllFilledSelectedLanguages.ForEach(l =>
            {
              MasterGroupMappingLanguage masterGroupMappingLanguage = unit
                .Service<MasterGroupMappingLanguage>()
                .Get(x => x.LanguageID == l.LanguageID && x.MasterGroupMappingID == masterGroupMapping.MasterGroupMappingID);

              if (masterGroupMappingLanguage == null)
              {
                if (!string.IsNullOrEmpty(l.Value))
                {
                  masterGroupMappingLanguage = new MasterGroupMappingLanguage()
                  {
                    MasterGroupMappingID = masterGroupMapping.MasterGroupMappingID,
                    LanguageID = l.LanguageID,
                    Name = l.Value
                  };
                  unit.Service<MasterGroupMappingLanguage>().Create(masterGroupMappingLanguage);
                }
              }
              else
              {
                if (string.IsNullOrEmpty(l.Value))
                {
                  unit.Service<MasterGroupMappingLanguage>().Delete(masterGroupMappingLanguage);
                }
                else
                {
                  masterGroupMappingLanguage.Name = l.Value;
                }
              }
            });
            unit.Save();
            return Success("Successfully updated Master Group Mapping");
          }
        }
      }
    }

    // Moet aangepast worden door jezzy
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult SetTranslation(int _LanguageID, int MasterGroupMappingID, string name, bool? LanguageMustBeFilled)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (!string.IsNullOrEmpty(name))
          {
            if (LanguageMustBeFilled.HasValue && LanguageMustBeFilled.Value || !string.IsNullOrEmpty(unit.Service<ProductGroupLanguage>().Get(x => x.LanguageID == _LanguageID && x.ProductGroupID == MasterGroupMappingID).Try(x => x.Name, string.Empty)))
            {
              var masterGroupMappingLanguage = unit
                .Service<MasterGroupMappingLanguage>()
                .Get(x => x.LanguageID == _LanguageID && x.MasterGroupMappingID == MasterGroupMappingID);

              if (masterGroupMappingLanguage == null)
              {
                masterGroupMappingLanguage = new MasterGroupMappingLanguage()
                {
                  MasterGroupMappingID = MasterGroupMappingID,
                  LanguageID = _LanguageID
                };
                unit.Service<MasterGroupMappingLanguage>().Create(masterGroupMappingLanguage);
              }

              masterGroupMappingLanguage.Name = name;
              unit.Save();
              return Success("Translations set");
            }
            else
            {
              return Failure("Checkbox not checked");
            }
          }
          else if (LanguageMustBeFilled.HasValue && !LanguageMustBeFilled.Value)
          {
            return Failure("Checkbox not checked");
          }
          else
          {
            //remove language record
            unit.Service<MasterGroupMappingLanguage>().Delete(x => x.LanguageID == _LanguageID && x.MasterGroupMappingID == MasterGroupMappingID);
            unit.Save();
            return Success("Translation set");
          }
        }
        catch (Exception e)
        {
          return Failure("Something went wrong", e.InnerException);
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMasterGroupMappingsLanguage(bool showEmptyRecords, string filterName, int languageID)
    {
      using (var unit = GetUnitOfWork())
      {
        var AllMappingPaths = GetListOfMasterGroupMappingPaths(null, languageID);

        var filter = string.IsNullOrEmpty(filterName) ?
                        string.Empty :
                        string.Format("AND ml.NAME like '%{0}%'", filterName);

        var masterGroupMappingLanguagesQuery = string.Format(@"
                SELECT mm.MasterGroupMappingID
                	,lang.LanguageID
                	,ml.NAME
                	,lang.NAME AS LanguageName
                  ,(
		                 SELECT count(*)
		                 FROM MasterGroupMappingProductGroupVendor AS pgv
		                 WHERE pgv.MasterGroupMappingID = mm.MasterGroupMappingID
		                 ) AS MATCHED
                FROM Mastergroupmapping mm
                LEFT JOIN MasterGroupmappingLanguage ml ON ml.MasterGroupMappingID = mm.MasterGroupMappingID and ml.LanguageID = {0}
                INNER JOIN [Language] lang ON lang.LanguageID = {0}
                WHERE mm.ConnectorID IS NULL;
                ", languageID, filter);

        var masterGroupMappingLanguages = unit.ExecuteStoreQuery<ListOfMasterGroupMappingLanguages>(masterGroupMappingLanguagesQuery)
          .GroupBy(x => x.MasterGroupMappingID)
          .ToDictionary(x => x.Key, x => x.ToList());

        var data = (from x in masterGroupMappingLanguages
                    let languageRec = x.Value.Where(lang => lang.LanguageID == languageID).FirstOrDefault()
                    where showEmptyRecords ? string.IsNullOrEmpty(languageRec.Name) : !string.IsNullOrEmpty(languageRec.Name)
                    select new
                    {
                      MasterGroupMappingName = string.IsNullOrEmpty(languageRec.Name) ? "MasterGroupMapping NAME EMPTY!" : languageRec.Name,
                      languageID,
                      LanguageName = languageRec.LanguageName,
                      Matched = languageRec.Matched,
                      x.Key,
                      MasterGroupMappingPath = AllMappingPaths.Where(path => path.MasterGroupMappingID == x.Key).Select(y => y.MasterGroupMappingPath).FirstOrDefault()
                    }).ToList();

        return List(data.AsQueryable());
      }
    }
    #endregion

    #endregion

    #region Tabblad "Vendor Product Groups"
    [RequiresAuthentication(Functionalities.GetProductGroupVendor)]
    public ActionResult GetListOfProductGroupVendors(int? VendorID, int? ShowID, int? IsBlocked, int? MasterGroupMappingID, string ProductGroupBrandCode)
    {
      var nullOrEmpty = new[] { null, "" };
      return List(unit =>
          unit.Service<ProductGroupVendor>().GetAll()
            .Where(x => (ShowID.HasValue ? (ShowID == 3 ? x.MasterGroupMappings.Count == 0 : (ShowID == 2 ? x.MasterGroupMappings.Count > 0 : true)) : true))
            .Where(x => (IsBlocked.HasValue ? (IsBlocked == 3 ? x.IsBlocked == false : (IsBlocked == 2 ? x.IsBlocked == true : true)) : true))
            .Where(x => VendorID.HasValue ? x.VendorID == VendorID : true)
            .Where(x => (MasterGroupMappingID.HasValue && MasterGroupMappingID.Value > 0) ? (x.MasterGroupMappings.Any(m => m.MasterGroupMappingID == MasterGroupMappingID)) : true)
            .Where(x => nullOrEmpty.Contains(ProductGroupBrandCode) ||
              x.VendorName.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode1.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode2.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode3.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode4.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode5.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode6.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode7.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode8.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode9.Contains(ProductGroupBrandCode) ||
              x.VendorProductGroupCode10.Contains(ProductGroupBrandCode) ||
              x.BrandCode.Contains(ProductGroupBrandCode))
            .Select(x => new
            {
              x.ProductGroupVendorID,
              x.VendorID,
              VendorName = x.Vendor.Name,
              x.ProductGroupID,
              x.VendorProductGroupCode1,
              x.VendorProductGroupCode2,
              x.VendorProductGroupCode3,
              x.VendorProductGroupCode4,
              x.VendorProductGroupCode5,
              x.VendorProductGroupCode6,
              x.VendorProductGroupCode7,
              x.VendorProductGroupCode8,
              x.VendorProductGroupCode9,
              x.VendorProductGroupCode10,
              x.BrandCode,
              VendorProductGroupName = x.VendorName,
              IsBlocked = x.IsBlocked
            })
      );
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfVendorProductsGroup(string ListOfProductGroupIDsJson)
    {
      using (var unit = GetUnitOfWork())
      {
        var ListOfIDsModel = new
        {
          MasterGroupMappingIDs = new[] { 0 },
          ProductGroupVendorIDs = new[] { 0 }
        };
        var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfProductGroupIDsJson, ListOfIDsModel);

        var productGroupVendorIds = ListOfIDs.ProductGroupVendorIDs.ToArray();

        var listOfVendorAssortments = unit.Service<ProductGroupVendor>()
          .GetAll(x => productGroupVendorIds.Contains(x.ProductGroupVendorID))
          .Select(x => new { ProductGroupVendor = x, x.VendorAssortments })
          .Select(x => x.VendorAssortments.Select(y => new
          {
            y.VendorAssortmentID,
            check = true,
            ShortDescription = y.ShortDescription,
            y.ProductID,
            y.CustomItemNumber,
            y.Product.VendorItemNumber,
            productGroupVendorName = x.ProductGroupVendor.VendorName
          }))
          .SelectMany(x => x)
          .OrderBy(x => x.productGroupVendorName);

        var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
        return new ContentResult()
        {
          Content = serializer.Serialize(new { results = listOfVendorAssortments }),
          ContentType = "application/json"
        };
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListVendorProductsByMasterGroupMappingAndMultipleVendorProductGroup(string ListOfIDsJson)
    {
      var ListOfVpgAndMgmIDs = JsonConvert.DeserializeObject<List<MatchedMasterGroupMappingVendorProductGroup>>(ListOfIDsJson);
      using (var unit = GetUnitOfWork())
      {
        var listOfVendorAssortments = (from unMatchItem in ListOfVpgAndMgmIDs
                                       let listOfProductIDs = unit
          .Service<MasterGroupMappingProduct>()
          .GetAll(x => x.MasterGroupMappingID == unMatchItem.MasterGroupMappingID && x.IsProductMapped)
          .Select(x => x.ProductID)
                                       from vpgID in unMatchItem.ListOfVendorProductGroupIDs
                                       from va in unit.Service<VendorAssortment>()
          .GetAll(x => x.ProductGroupVendors.Any(y => y.ProductGroupVendorID == vpgID))
                                       let productgroupvendorName = va.ProductGroupVendors.FirstOrDefault(x => x.ProductGroupVendorID == vpgID)
                                       select new
                                       {
                                         va.VendorAssortmentID,
                                         Check = true,
                                         ShortDescription = va.ShortDescription,
                                         va.ProductID,
                                         ProductGroupVendorID = vpgID,
                                         productGroupVendorName = productgroupvendorName.Vendor.Name + " " + productgroupvendorName.VendorName,
                                         va.CustomItemNumber,
                                         va.Product.VendorItemNumber,
                                         unMatchItem.MasterGroupMappingID
                                       });

        return Json(new { results = listOfVendorAssortments });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListVendorProductsByMasterGroupMappingAndVendorProductGroup(string ListOfIDsJson)
    {
      var ListOfVpgAndMgmIDsModel = new { VendorProductGroupID = 0, ListOfMasterGroupMappingIDs = new[] { 0 } };
      var ListOfVpgAndMgmIDs = JsonConvert.DeserializeAnonymousType(ListOfIDsJson, ListOfVpgAndMgmIDsModel);
      using (var unit = GetUnitOfWork())
      {
        var listOfProductIDs = unit
          .Service<MasterGroupMappingProduct>()
          .GetAll(x => ListOfVpgAndMgmIDs.ListOfMasterGroupMappingIDs.Contains(x.MasterGroupMappingID) && x.IsProductMapped)
          .Select(x => x.ProductID);

        var listOfVendorAssortments = unit
          .Service<VendorAssortment>()
          .GetAll(x => x.ProductGroupVendors.Any(y => y.ProductGroupVendorID == ListOfVpgAndMgmIDs.VendorProductGroupID))
          .Where(x => listOfProductIDs.Contains(x.ProductID))
          .Select(x => new
          {
            x.VendorAssortmentID,
            Check = true,
            ShortDescription = x.ShortDescription,
            x.ProductID,
            productGroupVendorName = x.Vendor.Name,
            x.CustomItemNumber,
            x.Product.VendorItemNumber
          });
        return Json(new { results = listOfVendorAssortments });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult MatchVendorProductGroups(string listOfMgmAndVpIDsJson)
    {
      var ListOfIDsModel = new
      {
        MasterGroupMappingIDs = new[] { 0 },
        ProductIDs = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(listOfMgmAndVpIDsJson, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          foreach (int MasterGroupMappingID in ListOfIDs.MasterGroupMappingIDs)
          {
            var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
            foreach (int ProductID in ListOfIDs.ProductIDs.Distinct())
            {
              MasterGroupMappingProduct masterGroupMappingProduct = unit
                .Service<MasterGroupMappingProduct>()
                .Get(x => x.MasterGroupMappingID == MasterGroupMappingID && x.ProductID == ProductID);

              if (masterGroupMappingProduct == null)
              {
                masterGroupMappingProduct = new MasterGroupMappingProduct()
                {
                  MasterGroupMappingID = MasterGroupMappingID,
                  ProductID = ProductID,
                  IsApproved = false,
                  IsCustom = true,
                  IsProductMapped = true
                };
                masterGroupMapping.MasterGroupMappingProducts.Add(masterGroupMappingProduct);
              }
              else
              {
                if (!masterGroupMappingProduct.IsProductMapped)
                {
                  masterGroupMappingProduct.IsProductMapped = true;
                }
              }
            }
          }
          unit.Save();
        }
        return Success("Product groups are successfully matched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to match the product group.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfVendorNames()
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfVerndorNames = unit.Service<ProductGroupVendor>()
          .GetAll()
          .GroupBy(x => x.VendorID)
          .Select(x =>
            new
            {
              VendorID = x.Key,
              productGroupVendor = x.FirstOrDefault()
            })
          .Select(x => new
          {
            VendorID = x.VendorID,
            VendorName = x.productGroupVendor.Vendor.Name
          })
          ;
        return Json(new
        {
          results = listOfVerndorNames
        });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult MatchProductGroupVendor(string ListOfProductGroupIDsJson)
    {
      var ListOfIDsModel = new
      {
        MasterGroupMappingIDs = new[] { 0 },
        ProductGroupVendorIDs = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfProductGroupIDsJson, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          foreach (int MasterGroupMappingID in ListOfIDs.MasterGroupMappingIDs)
          {
            var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
            foreach (int productGroupVendorID in ListOfIDs.ProductGroupVendorIDs)
            {
              ProductGroupVendor productGroupVendor = unit.Service<ProductGroupVendor>().Get(x => x.ProductGroupVendorID == productGroupVendorID);
              productGroupVendor.IsBlocked = false;
              masterGroupMapping.ProductGroupVendors.Add(productGroupVendor);
            }
          }
          unit.Save();
        }
        return Success("Product groups are successfully matched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to match the product group.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnMatchMultipleVendorProductGroup(string ListOfIDsJson, string MasterGroupMappingIDs)
    {

      var listOfMasterGroupMappingIDs = JsonConvert.DeserializeObject<List<int>>(MasterGroupMappingIDs);
      var listOfProductsPerGroupForMasterGroupMapping = JsonConvert.DeserializeObject<List<MatchedVendorProductsPerGroupFoMasterGroupMapping>>(ListOfIDsJson);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          foreach (var productGroup in listOfProductsPerGroupForMasterGroupMapping)
          {
            var productGroupVendor = unit.Service<ProductGroupVendor>().Get(x => x.ProductGroupVendorID == productGroup.VendorProductGroupID);

            var productIds = unit
              .Service<VendorAssortment>()
              .GetAll()
              .Where(x => x.ProductGroupVendors.Any(y => y.ProductGroupVendorID == productGroupVendor.ProductGroupVendorID))
              .Select(x => x.ProductID);

            // delete mapping (Master Group Mapping <-> Product)
            // todo: Review met Jezzy
            //unit
            //  .Service<MasterGroupMappingProduct>()
            //  .Delete(x => listOfMasterGroupMappingIDs.Contains(x.MasterGroupMappingID) && productIds.Contains(x.ProductID));
            unit
              .Service<MasterGroupMappingProduct>()
              .Update(m => m.IsProductMapped = false, x => listOfMasterGroupMappingIDs.Contains(x.MasterGroupMappingID) && productIds.Contains(x.ProductID));

            // delete mapping (Master Group Mapping <-> Product Group Vendor)

            foreach (int masterGroupMappingID in listOfMasterGroupMappingIDs)
            {
              var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == masterGroupMappingID);
              productGroupVendor.MasterGroupMappings.Remove(masterGroupMapping);
            }
          }
          unit.Save();
        }
        return Success("Vendor Product Group is successfully unmatched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to unmatch the vendor product group.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnMatchVendorProductGroup(string ListOfIDsJson)
    {
      var listOfVpgAndMgmAndVpIDsModel = new { VendorProductGroupID = 0, ListOfMasterGroupMappingIDs = new[] { 0 }, ListOfVendorProductIDs = new[] { 0 } };
      var listOfVpgAndMgmAndVpIDs = JsonConvert.DeserializeAnonymousType(ListOfIDsJson, listOfVpgAndMgmAndVpIDsModel);
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var productGroupVendor = unit.Service<ProductGroupVendor>().Get(x => x.ProductGroupVendorID == listOfVpgAndMgmAndVpIDs.VendorProductGroupID);

          var productIds = unit
            .Service<VendorAssortment>()
            .GetAll()
            .Where(x => x.ProductGroupVendors.Any(y => y.ProductGroupVendorID == productGroupVendor.ProductGroupVendorID))
            .Select(x => x.ProductID);

          // delete mapping (Master Group Mapping <-> Product)
          unit
            .Service<MasterGroupMappingProduct>()
            //.Delete(x => listOfVpgAndMgmAndVpIDs.ListOfMasterGroupMappingIDs.Contains(x.MasterGroupMappingID) && productIds.Contains(x.ProductID));
            .Update(m => m.IsProductMapped = false, x => listOfVpgAndMgmAndVpIDs.ListOfMasterGroupMappingIDs.Contains(x.MasterGroupMappingID) && productIds.Contains(x.ProductID));

          // delete mapping (Master Group Mapping <-> Product Group Vendor)

          foreach (int masterGroupMappingID in listOfVpgAndMgmAndVpIDs.ListOfMasterGroupMappingIDs)
          {
            var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == masterGroupMappingID);
            productGroupVendor.MasterGroupMappings.Remove(masterGroupMapping);
          }
          unit.Save();
        }
        return Success("Vendor Product Group is successfully unmatched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to unmatch the vendor product group.", e);
      }
    }

    #region Drag and Drop
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMatchedMasterGroupMappingNames(int MasterGroupMappingID, int? ConnectorID, string productIDs = "-1")
    {
      using (var db = new PetaPoco.Database(Environments.Current.Connection, Database.MsSqlClientProvider))
      {
        var isCurrentUserAdmin = Client.User.CurrentRole.FunctionalityRoles
          .FirstOrDefault(x => x.FunctionalityName == Enum.GetName(typeof(Functionalities), Functionalities.MasterGroupMappingAdministrator)) != null;

        var masterGroupmappingName = db.Fetch<string>(@"
          SELECT NAME AS MasterGroupMappingName
          FROM MasterGroupMappingLanguage
          WHERE MasterGroupMappingID = @0 AND LanguageID = @1"
          , MasterGroupMappingID
          , Client.User.LanguageID)
          .FirstOrDefault();

        var userID = Client.User.UserID;

        var prodis = productIDs.Split(',').Select<string, int>(int.Parse).ToArray();

        var connectorIDFilter = string.Format("ConnectorID {0}", (ConnectorID.HasValue && ConnectorID.Value > 0) ? "= " + ConnectorID.ToString() : "is NULL");

        var masterGroupMappingBaseQueryWithName = string.Format(@"
                    SELECT mm.MasterGroupMappingID
                    FROM MasterGroupMapping mm
                    INNER JOIN MasterGroupMappingLanguage ml ON ml.MasterGroupMappingID = mm.MasterGroupMappingID
                    {3}
                    WHERE LanguageID = {0}
                    	AND NAME = '{1}'
                    	AND {2}
            ", Client.User.LanguageID, masterGroupmappingName, connectorIDFilter, !isCurrentUserAdmin ?
             string.Format(@"INNER JOIN MasterGroupMappingUser mgmu on mgmu.mastergroupmappingid = mm.mastergroupmappingid and mgmu.UserID = {0}", userID) : string.Empty);

        var masterGroupMappingBaseQueryWithoutName = string.Format(@"
                    SELECT MasterGroupMappingID
                    FROM MasterGroupMapping mgm
                    {2}
                    WHERE MasterGroupMappingID = {0}
                      AND {1}
            ", MasterGroupMappingID, connectorIDFilter, !isCurrentUserAdmin ?
             string.Format("INNER JOIN MasterGroupMappingUser mgmu on mgmu.mastergroupmappingid = mm.mastergroupmappingid and mgmu.UserID = {0}", userID) : string.Empty);

        var listOfMatchedMasterGroupMappings = db.Fetch<Int32>(string.IsNullOrEmpty(masterGroupmappingName) ?
          masterGroupMappingBaseQueryWithoutName : masterGroupMappingBaseQueryWithName)
        .Select(x =>
        {
          int masterGroupMappingID = x;
          bool hasParent = true;
          var check = false;
          var MasterGroupMappingPathName = "";
          do
          {

            var masterGroupMapping = db.Single<MasterGroupMappingModel>(@"
                        SELECT ParentMasterGroupMappingID
                        	,ml.NAME AS MasterGroupMappingName
                        FROM MasterGroupMapping mm
                        LEFT JOIN MasterGroupMappingLanguage ml ON ml.MasterGroupMappingID = mm.MasterGroupMappingID
                        	AND ml.LanguageID = @1
                        WHERE mm.MasterGroupMappingID = @0
                  ", masterGroupMappingID, Client.User.LanguageID);

            var tempMasterGroupMappingPathName = masterGroupMapping.MasterGroupMappingName ?? "MasterGroupMapping NAME EMPTY!";

            if (string.IsNullOrEmpty(MasterGroupMappingPathName))
            {
              check = db.Single<Int32>(string.Format(@"
                    SELECT count(*)
                    FROM MasterGroupMappingProduct
                    WHERE MAsterGroupMappingID = {0}
                    	AND ProductID IN ({1})
                     ", masterGroupMappingID, string.Join(",", prodis))) > 0;
            }
            MasterGroupMappingPathName = tempMasterGroupMappingPathName + MasterGroupMappingPathName;

            if (masterGroupMapping.ParentMasterGroupMappingID.HasValue)
            {
              masterGroupMappingID = masterGroupMapping.ParentMasterGroupMappingID.Value;
              MasterGroupMappingPathName = "->" + MasterGroupMappingPathName;
            }
            else
            {
              hasParent = false;
            }
          } while (hasParent);

          return new
          {
            MasterGroupMappingID = x,
            Check = check || x == MasterGroupMappingID ? (int)CheckBoxBoolean.CheckedDisabled : (int)CheckBoxBoolean.UncheckedEnabled,
            MasterGroupMappingPad = MasterGroupMappingPathName
          };
        })
        .OrderBy(o => o.MasterGroupMappingPad)
        ;

        listOfMatchedMasterGroupMappings.ForEach((obj, id) =>
        {
          var prop = obj.GetType().GetProperties().FirstOrDefault(x => x.Name == "Check");
        });

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }

    #endregion

    #region <-> Tabblad "Matched Product Groups"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfVendorAssortment(int ProductGroupVendorID)
    {

      using (var unit = GetUnitOfWork())
      {
        var listOfVendorAssortments = unit.Service<VendorAssortment>().GetAll(x => x.ProductGroupVendors.Any(p => p.ProductGroupVendorID == ProductGroupVendorID))
            .Where(x => x.IsActive == true)
            .Select(x => new
            {
              x.VendorAssortmentID,
              ConcentratorNumber = x.ProductID,
              DistriItemNumber = x.CustomItemNumber,
              ProductName = x.ShortDescription,
              Brand = x.Product.Brand.Name,
              ProductCode = x.Product.VendorItemNumber,
              Image = x.Product.ProductMedias.Count > 0 ? true : false,
              PossibleMatch = x.Product.ProductMatch.MatchPercentage
            });

        var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
        return new ContentResult()
        {
          Content = serializer.Serialize(new { results = listOfVendorAssortments }),
          ContentType = "application/json"
        };
      }

      //return List(unit =>
      //    unit.Service<VendorAssortment>().GetAll(x => x.ProductGroupVendors.Any(p => p.ProductGroupVendorID == ProductGroupVendorID))
      //      .Where(x => x.IsActive == true)
      //      .Select(x => new
      //      {
      //        x.VendorAssortmentID,
      //        ConcentratorNumber = x.ProductID,
      //        DistriItemNumber = x.CustomItemNumber,
      //        ProductName = x.ShortDescription,
      //        Brand = x.Product.Brand.Name,
      //        ProductCode = x.Product.VendorItemNumber,
      //        Image = x.Product.ProductMedias.Count > 0 ? true : false,
      //        PossibleMatch = x.Product.ProductMatch.MatchPercentage
      //      })
      //);
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnMatchProductGroupVendorAndMasterGroupMapping(string ListOfMasterGroupMappingIDsJson)
    {
      var ListOfIDsModel = new
      {
        ProductGroupVendorID = 0,
        MasterGroupMappingIDs = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfMasterGroupMappingIDsJson, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var productGroupVendor = unit.Service<ProductGroupVendor>().Get(x => x.ProductGroupVendorID == ListOfIDs.ProductGroupVendorID);

          var productIds = unit.Service<VendorAssortment>()
            .GetAll()
            .Where(x => x.ProductGroupVendors.Any(y => y.ProductGroupVendorID == productGroupVendor.ProductGroupVendorID))
            .Select(x => x.ProductID);
          var masterGroupMappingIds = ListOfIDs.MasterGroupMappingIDs.ToArray();
          unit
            .Service<MasterGroupMappingProduct>()
            //.Delete(x => productIds.Contains(x.ProductID) && masterGroupMappingIds.Contains(x.MasterGroupMappingID));
            .Update(m => m.IsProductMapped = false, x => productIds.Contains(x.ProductID) && masterGroupMappingIds.Contains(x.MasterGroupMappingID));

          foreach (int masterGroupMappingID in ListOfIDs.MasterGroupMappingIDs)
          {
            var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == masterGroupMappingID);
            productGroupVendor.MasterGroupMappings.Remove(masterGroupMapping);
          }
          unit.Save();
        }
        return Success("Product group is successfully unmatched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to unmatch the product group.", e);
      }
    }



    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMultipleMatchedMasterGroupMappings(string records)
    {
      var ListOfProductGroupVendorIDsModel = new
        {
          GridMasterGroupMappingID = 0,
          ProductGroupVendorIDs = new[] { 0 }
        };
      var ListOfProductGroupVendorIDs = JsonConvert.DeserializeAnonymousType(records, ListOfProductGroupVendorIDsModel);


      using (var unit = GetUnitOfWork())
      {
        var listOfMatchedMasterGroupMappings = (from pgv in ListOfProductGroupVendorIDs.ProductGroupVendorIDs
                                                from mgm in unit.Service<MasterGroupMapping>()
           .GetAll(x => x.ProductGroupVendors.Any(m => m.ProductGroupVendorID == pgv))
                                                select new { mgm, ProductGroupVendorID = pgv })
             .ToList()
             .Select(x =>
             {
               int masterGroupMappingID = x.mgm.MasterGroupMappingID;
               bool hasParent = true;
               var MasterGroupMappingPadName = "";
               do
               {
                 var masterGroupmapping = unit.Service<MasterGroupMapping>()
                   .Get(m => m.MasterGroupMappingID == masterGroupMappingID);

                 var tempMasterGroupMappingPadName = masterGroupmapping
                   .MasterGroupMappingLanguages.Try(m => m.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name, "MasterGroupMapping NAME EMPTY!");

                 MasterGroupMappingPadName = tempMasterGroupMappingPadName + MasterGroupMappingPadName;

                 if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
                 {
                   masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
                   MasterGroupMappingPadName = "->" + MasterGroupMappingPadName;
                 }
                 else
                 {
                   hasParent = false;
                 }
               } while (hasParent);
               var productgroupvendorname = x.mgm.ProductGroupVendors.FirstOrDefault(pv => pv.ProductGroupVendorID == x.ProductGroupVendorID);

               return new
               {
                 ProductGroupVendorID = x.ProductGroupVendorID,
                 ProductGroupVendorName = productgroupvendorname.Vendor.Name + " " + productgroupvendorname.VendorName,
                 MasterGroupMappingID = x.mgm.MasterGroupMappingID,
                 Check = ListOfProductGroupVendorIDs.GridMasterGroupMappingID > 0 ? (ListOfProductGroupVendorIDs.GridMasterGroupMappingID == x.mgm.MasterGroupMappingID ? true : false) : false,
                 MasterGroupMappingPad = MasterGroupMappingPadName
               };
             })
             .OrderBy(o => o.MasterGroupMappingPad)
             ;

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMatchedMasterGroupMappings(int ProductGroupVendorID, int? GridMasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfMatchedMasterGroupMappings = unit.Service<MasterGroupMapping>()
          .GetAll(x => x.ProductGroupVendors.Any(m => m.ProductGroupVendorID == ProductGroupVendorID))
          .ToList()
          .Select(x =>
          {
            int masterGroupMappingID = x.MasterGroupMappingID;
            bool hasParent = true;
            var MasterGroupMappingPadName = "";
            do
            {
              var masterGroupmapping = unit.Service<MasterGroupMapping>()
                .Get(m => m.MasterGroupMappingID == masterGroupMappingID);

              var tempMasterGroupMappingPadName = masterGroupmapping
                .MasterGroupMappingLanguages
                .FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name;

              MasterGroupMappingPadName = tempMasterGroupMappingPadName + MasterGroupMappingPadName;

              if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
              {
                masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
                MasterGroupMappingPadName = "->" + MasterGroupMappingPadName;
              }
              else
              {
                hasParent = false;
              }
            } while (hasParent);

            return new
            {
              MasterGroupMappingID = x.MasterGroupMappingID,
              ProductGroupVendorName = x.ProductGroupVendors.FirstOrDefault(pv => pv.ProductGroupVendorID == ProductGroupVendorID).Vendor.Name,// + " " x.ProductGroupVendors.FirstOrDefault().VendorName,
              Check = GridMasterGroupMappingID.HasValue ? (GridMasterGroupMappingID.Value == x.MasterGroupMappingID ? true : false) : false,
              MasterGroupMappingPad = MasterGroupMappingPadName
            };
          })
          .OrderBy(o => o.MasterGroupMappingPad)
          ;

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductGroupVendor)]
    public ActionResult UpdateProductGroupVendors(int _ProductGroupVendorID, Boolean? IsBlocked)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var productGroupVendor = unit.Service<ProductGroupVendor>().Get(x => x.ProductGroupVendorID == _ProductGroupVendorID);

          productGroupVendor.IsBlocked = IsBlocked.GetValueOrDefault();

          unit.Save();

          if (IsBlocked.HasValue && IsBlocked.Value)
          {
            var removeProductGroupVendorsFromMasterGroupMappingsQuery = string.Format(@"
                  DELETE
                  FROM MasterGroupMappingProductGroupVendor
                  WHERE ProductGroupVendorID = {0}
            ", _ProductGroupVendorID);

            unit.ExecuteStoreCommand(removeProductGroupVendorsFromMasterGroupMappingsQuery);

            var unmapProductsInVendorProductGroupQuery = string.Format(@"
                 UPDATE MasterGroupMappingProduct
                 SET IsProductMapped = 0
                 WHERE ProductID IN (
                 		SELECT productid
                 		FROM VendorAssortment va
                 		INNER JOIN VendorProductGroupAssortment vpga ON vpga.VendorAssortmentID = va.VendorAssortmentID
                 		WHERE ProductGroupVendorID = {0}
                 		)
            ", _ProductGroupVendorID);

            unit.ExecuteStoreCommand(unmapProductsInVendorProductGroupQuery);

          }
        }
        return Success("Product group is successfully blocked.");
      }
      catch (Exception e)
      {
        return Failure("Failed to block the product group.", e);
      }
    }
    #endregion

    #endregion

    #region Tabblad "Vendor Products"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfVendorProducts(
      int? MatchedUnMatchedVendorProducts,
      int? ImageVendorProducts,
      int? MasterGroupMappingID,
      int? ProductBlockStatus,
      string ProductID,
      string UniversalSearchTerm)
    {

      var connectorID = Client.User.ConnectorID;

      using (var unit = GetUnitOfWork())
      {
        bool? isBlocked = null;
        if (ProductBlockStatus.HasValue)
        {
          switch (ProductBlockStatus.Value)
          {
            case 1:
              break;
            case 2:
              isBlocked = true;
              break;
            case 3:
              isBlocked = false;
              break;
            default:
              throw new NotImplementedException(string.Format("ProductBlockStatus have value {0}", ProductBlockStatus));
          }
        }

        bool? isProductMapped = null;
        if (MatchedUnMatchedVendorProducts.HasValue)
        {
          switch (MatchedUnMatchedVendorProducts.Value)
          {
            case 1:
              break;
            case 2:
              isProductMapped = true;
              break;
            case 3:
              isProductMapped = false;
              break;
            default:
              throw new NotImplementedException(string.Format("MatchedUnMatchedVendorProducts have value {0}", MatchedUnMatchedVendorProducts));
          }
        }

        bool? hasProductImage = null;
        if (ImageVendorProducts.HasValue)
        {
          switch (ImageVendorProducts.Value)
          {
            case 1: break;
            case 2:
              hasProductImage = true;
              break;
            case 3:
              hasProductImage = false;
              break;
            default:
              throw new NotImplementedException(string.Format("ImageVendorProducts have value {0}", ImageVendorProducts));
          }
        }

        var data = ((IMasterGroupMappingService)unit.Service<MasterGroupMapping>()).GetListOfMasterGroupMappingProducts(
          MasterGroupMappingID,
          isBlocked,
          ProductID,
          UniversalSearchTerm,
          isProductMapped,
          hasProductImage,
          connectorID
          );

        return List(data
          .Select(x => new
            {
              ConcentratorNumber = x.ProductID,
              ProductName = x.ShortDescription,
              Brand = x.BrandName,
              ProductCode = x.VendorItemNumber,
              Image = x.HasProductAnImage,
              PossibleMatch = x.MatchPercentage,
              CountVendorAssortment = x.CountVendorItemNumbers,
              x.CountMatches,
              ProductControl =
                          (MasterGroupMappingID.HasValue && MasterGroupMappingID.Value > 0)
                            ? (x.IsApproved ? ("Approved") : ("Rejected"))
                            : "NotMatched",
              MasterGroupMappingID = (MasterGroupMappingID.HasValue && MasterGroupMappingID.Value > 0) ? MasterGroupMappingID.Value : 0,
              x.IsBlocked,
              x.IsConfigurable
            })
          );
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult MatchVendorProduct(string ListOfToMatchIDsJson)
    {
      var ListOfIDsModel = new
      {
        MasterGroupMappingIDs = new[] { 0 },
        ProductIDs = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfToMatchIDsJson, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          foreach (int MasterGroupMappingID in ListOfIDs.MasterGroupMappingIDs)
          {
            var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

            var groups = new List<ContentProductGroup>();
            ConstructContentProductGroupHierarchy(groups, ListOfIDs.ProductIDs, MasterGroupMappingID, unit);

            foreach (int ProductID in groups.Select(x => x.ProductID))
            {
              var product = unit.Service<Product>().Get(x => x.ProductID == ProductID);
              if (product != null)
              {
                MasterGroupMappingProduct tempMasterGroupMappingProduct = unit
                  .Service<MasterGroupMappingProduct>()
                  .Get(x => x.ProductID == ProductID && x.MasterGroupMappingID == MasterGroupMappingID);

                product.IsBlocked = false;
                if (tempMasterGroupMappingProduct == null)
                {
                  MasterGroupMappingProduct masterGroupMappingProduct = new MasterGroupMappingProduct()
                  {
                    MasterGroupMappingID = MasterGroupMappingID,
                    ProductID = ProductID,
                    IsCustom = true,
                    IsApproved = false,
                    IsProductMapped = true
                  };
                  masterGroupMapping.MasterGroupMappingProducts.Add(masterGroupMappingProduct);
                }
                else
                {
                  tempMasterGroupMappingProduct.IsProductMapped = true;
                }
              }
            }
          }
          unit.Save();
        }
        return Success("Product are successfully matched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to match the product.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateProduct(int ProductID, bool IsBlocked)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = unit
          .Service<Product>()
          .Get(x => x.ProductID == ProductID);
        if (product == null)
        {
          return Failure("Failed to update product. Product (" + ProductID + ") does not exist.");
        }
        else
        {
          try
          {
            product.IsBlocked = IsBlocked;
            if (IsBlocked)
              unit
                .Service<MasterGroupMappingProduct>()
                .Update(m => m.IsProductMapped = false, x => x.ProductID == ProductID);
            //.Delete(p => p.ProductID == ProductID);
            unit.Save();
            return Success("Product is successfully updated.");
          }
          catch
          {
            return Failure("Failed to update product (" + ProductID + ").");
          }
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMachtedMasterGroupMappingsForProduct(int ProductID, int? MasterGroupMapping)
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfMatchedMasterGroupMappings = unit
          .Service<MasterGroupMapping>()
          .GetAll(x => x.MasterGroupMappingProducts.Any(m => m.ProductID == ProductID))
          .ToList()
          .Select(x =>
          {
            int masterGroupMappingID = x.MasterGroupMappingID;
            bool hasParent = true;
            var MasterGroupMappingPadName = "";
            do
            {
              var masterGroupmapping = unit.Service<MasterGroupMapping>()
                .Get(m => m.MasterGroupMappingID == masterGroupMappingID);

              var tempMasterGroupMappingPadName = masterGroupmapping
                .MasterGroupMappingLanguages
                .FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name;

              MasterGroupMappingPadName = tempMasterGroupMappingPadName + MasterGroupMappingPadName;

              if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
              {
                masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
                MasterGroupMappingPadName = "->" + MasterGroupMappingPadName;
              }
              else
              {
                hasParent = false;
              }
            } while (hasParent);

            return new
            {
              MasterGroupMappingID = x.MasterGroupMappingID,
              Check = MasterGroupMapping.HasValue ? (MasterGroupMapping.Value == x.MasterGroupMappingID ? true : false) : false,
              MasterGroupMappingPad = MasterGroupMappingPadName
            };
          })
          .OrderBy(o => o.MasterGroupMappingPad)
          ;

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }

    #region <-> Tabblad "Matched Vendor Products"
    // Bug: Als productmatchid niet bestaat! Dan loopt de code vast!! 
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMatchedProducts(int ProductID)
    {
      using (var unit = GetUnitOfWork())
      {
        var productMatchQuery = string.Format(@"
          SELECT pm.productid AS sourceproductid
	          ,pm2.ismatched
	          ,p.productid
	          ,p.vendoritemnumber
	          ,v.NAME AS VendorName
	          ,pm2.matchstatus
	          ,b.NAME AS Brand
	          ,pm2.matchpercentage
	          ,pm2.[primary]
	          ,va.shortdescription
	          ,v.vendorid
          FROM productmatch pm
          INNER JOIN productmatch pm2 ON pm.productmatchid = pm2.productmatchid
          INNER JOIN vendorassortment va ON pm2.productid = va.productid and va.isactive = 1
          INNER JOIN product p ON p.productid = pm2.productid
          INNER JOIN vendor v ON va.vendorid = v.vendorid
          INNER JOIN brand b ON b.brandid = p.brandid
          WHERE pm.productid = {0}
        ", ProductID);

        var productMatches = unit.ExecuteStoreQuery<MasterGroupMappingProductMatch>(productMatchQuery).Where(x => x.sourceproductid == ProductID).ToList();

        var productBarcodeQuery = string.Format(@"
          SELECT pb.*
          FROM productmatch pm
          INNER JOIN productmatch pm2 ON pm.productmatchid = pm2.productmatchid
          INNER JOIN productbarcode pb ON pm2.productid = pb.productid
          WHERE pm.productid = {0}
        ", ProductID);

        var productBarcodes = unit.ExecuteStoreQuery<ProductBarcode>(productBarcodeQuery).ToList();

        var productMediaQuery = string.Format(@"
          SELECT pb.*
          FROM productmatch pm
          INNER JOIN productmatch pm2 ON pm.productmatchid = pm2.productmatchid
          INNER JOIN productmedia pb ON pm2.productid = pb.productid
          WHERE pm.productid = {0}
        ", ProductID);

        var productMedias = unit.ExecuteStoreQuery<ProductMedia>(productMediaQuery).ToList();

        var productattributeQuery = string.Format(@"
          SELECT pm2.productid
	          ,pav.attributeid
	          ,pav.value
	          ,pan.NAME
          FROM productmatch pm
          INNER JOIN productmatch pm2 ON pm.productmatchid = pm2.productmatchid
          INNER JOIN Productattributevalue pav ON pm2.productid = pav.productid
	          AND pav.languageid = {0}
          INNER JOIN productattributename pan ON pav.attributeid = pan.attributeid
	          AND pav.languageid = {0}
          WHERE pm.productid = {1}
        ", Client.User.LanguageID, ProductID);

        var productAttributeValues = unit.ExecuteStoreQuery<MasterGroupMappingProductMatchAttribute>(productattributeQuery).ToList();

        var listOfProducts = (from pm in productMatches
                              let productBarcode = productBarcodes.Where(x => x.ProductID == pm.ProductID).Select(x => x.Barcode).FirstOrDefault()
                              let vendorProductBarcode = productBarcodes.Where(x => x.ProductID == pm.ProductID && x.VendorID == pm.VendorID).Select(x => x.Barcode).FirstOrDefault()
                              let productMedia = productMedias.Where(x => x.ProductID == pm.ProductID && x.TypeID == 1).OrderBy(x => x.Sequence).Select(x => x.MediaUrl).FirstOrDefault()
                              let productAttributes = productAttributeValues.Where(x => x.ProductID == pm.ProductID)
                              select new
                                {
                                  Check = pm.IsMatched,
                                  ProductID = pm.ProductID,
                                  VendorItemNumber = pm.VendorItemNumber,
                                  MatchStatus = pm.MatchStatus == 1 ? "NEW" : pm.MatchStatus == 2 ? "Accepted" : pm.MatchStatus == 3 ? "Declined" : "Unknown", // Enum.GetName(typeof(MatchStatuses), pm.MatchStatus),
                                  VendorName = pm.VendorName,
                                  VendorDescription = pm.ShortDescription,
                                  Brand = pm.Brand,
                                  Barcode = vendorProductBarcode != null ? vendorProductBarcode : productBarcode != null ? productBarcode : string.Empty,
                                  Description = string.Empty, //p.ProductDescriptions.Count > 0 && p.ProductDescriptions.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID && x.ProductID == d.ProductID) != null ? x.ProductDescriptions.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID && x.ProductID == d.ProductID).ShortContentDescription : string.Empty,
                                  Title = string.Empty,//x.ProductDescriptions.Select(y => y.Product.Title).FirstOrDefault(),
                                  Image = productMedia != null ? productMedia : string.Empty,
                                  MatchPercentage = pm.MatchPercentage,
                                  pm.Primary,
                                  Attributes = productAttributes.Select(pav => new
                                  {
                                    AttributeID = pav.AttributeID,
                                    AttributeName = pav.Name,
                                    AttributeValue = pav.Value
                                  })
                                }
          )
        ;

        return Json(new { results = listOfProducts });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfVendorAssortmentsByProductID(int ProductID)
    {
      return List(unit => unit.Service<VendorAssortment>()
                            .GetAll(x => x.ProductID == ProductID)
                            .Select(x => new
                            {
                              x.VendorAssortmentID,
                              DistriItemNumber = x.CustomItemNumber,
                              ProductName = x.ShortDescription,
                              VendorName = x.Vendor.Name
                            })
        );
    }

    // query review!
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfProductsForAddProductToMatch(int ProductID, string ForGrid)
    {
      bool hasProductMatch;
      int productMatchID = 0;
      using (var unit = GetUnitOfWork())
      {
        var productMatch = unit
          .Service<Product>()
          .Get(x => x.ProductID == ProductID)
          .ProductMatch;
        if (productMatch == null)
          hasProductMatch = false;
        else
        {
          if (productMatch.isMatched)
            hasProductMatch = true;
          else
            hasProductMatch = false;
          productMatchID = productMatch.ProductMatchID;
        }
      }

      if (hasProductMatch)
      {
        if (ForGrid == "product")
        {
          return List(unit =>
          {
            var listOfProductIDsInProductMatchGroup = unit
              .Service<ProductMatch>()
              .GetAll(x => x.ProductMatchID == productMatchID && x.isMatched == true)
              .Select(x => x.Product);

            var mgmID = unit.Scope.Repository<MasterGroupMappingProduct>().GetSingle(x => x.ProductID == ProductID);

            if (mgmID == null)
            {
              var result = unit
               .Service<Product>()
               .GetAll(x => x.VendorAssortments.Where(va => va.IsActive == true).Count() > 0)
               .Except(listOfProductIDsInProductMatchGroup)
               .Select(x => new
               {
                 ConcentratorNumber = x.ProductID,
                 ProductName = x.VendorAssortments.Select(va => va.ShortDescription).FirstOrDefault(),
                 Brand = x.Brand.Name,
                 ProductCode = x.VendorItemNumber
               });
              return result;
            }
            else
            {
              var result = unit
              .Service<Product>()
              .GetAll(x => x.VendorAssortments.Where(va => va.IsActive == true).Count() > 0)
              .Except(listOfProductIDsInProductMatchGroup)
              .Select(x => new
              {
                ConcentratorNumber = x.ProductID,
                ProductName = x.VendorAssortments.Select(va => va.ShortDescription).FirstOrDefault(),
                Brand = x.Brand.Name,
                ProductCode = x.VendorItemNumber
              });
              return result;
            }

          });
        }
        else
        {
          return List(unit =>
            unit
              .Service<ProductMatch>()
              .GetAll(x => x.ProductMatchID == productMatchID && x.isMatched == true)
              .Select(x => x.Product)
              .Select(x => new
              {
                ConcentratorNumber = x.ProductID,
                ProductName = x.VendorAssortments.Select(va => va.ShortDescription).FirstOrDefault(),
                Brand = x.Brand.Name,
                ProductCode = x.VendorItemNumber
              })
          );
        }
      }
      else
      {
        if (ForGrid == "product")
        {
          return List(unit =>
             unit
              .Service<Product>()
              .GetAll(x => x.VendorAssortments.Where(va => va.IsActive == true).Count() > 0)
              .Where(x => x.ProductID != ProductID)
              .Select(x => new
              {
                ConcentratorNumber = x.ProductID,
                ProductName = x.VendorAssortments.Select(va => va.ShortDescription).FirstOrDefault(),
                Brand = x.Brand.Name,
                ProductCode = x.VendorItemNumber
              })
          );
        }
        else
        {

          return List(unit =>
             unit
              .Service<Product>()
              .GetAll(x => x.ProductID == ProductID)
              .Select(x => new
              {
                ConcentratorNumber = x.ProductID,
                ProductName = x.VendorAssortments.Select(va => va.ShortDescription).FirstOrDefault(),
                Brand = x.Brand.Name,
                ProductCode = x.VendorItemNumber
              })
          );
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateProductsForAddProductToMatch(string ListOfIDs)
    {
      var ListOfProductAndMatchIDsModel = new { ProductID = 0, NewMatchProductIDs = new[] { 0 }, Action = "" };
      var ListOfProductAndMatchIDs = JsonConvert.DeserializeAnonymousType(ListOfIDs, ListOfProductAndMatchIDsModel);
      try
      {
        using (var unit = GetUnitOfWork())
        {
          if (ListOfProductAndMatchIDs.Action == "match")
          {
            ProductMatch baseProductMatch = unit.Service<ProductMatch>().Get(x => x.ProductID == ListOfProductAndMatchIDs.ProductID);
            if (baseProductMatch == null)
            {
              var maxProductMatchID = unit.Service<ProductMatch>().GetAll().Select(x => x.ProductMatchID).Max();
              baseProductMatch = new ProductMatch();
              baseProductMatch.ProductID = ListOfProductAndMatchIDs.ProductID;
              baseProductMatch.ProductMatchID = ++maxProductMatchID;
              baseProductMatch.MatchStatus = (int)MatchStatuses.Accepted;
              baseProductMatch.isMatched = true;
              unit.Service<ProductMatch>().Create(baseProductMatch);
            }
            else
            {
              baseProductMatch.isMatched = true;
            }

            foreach (int NewMatchProductID in ListOfProductAndMatchIDs.NewMatchProductIDs)
            {
              ProductMatch toMatchProductMatch = unit.Service<ProductMatch>().Get(x => x.ProductID == NewMatchProductID);
              if (toMatchProductMatch == null)
              {
                toMatchProductMatch = new ProductMatch();
                toMatchProductMatch.ProductID = NewMatchProductID;
                toMatchProductMatch.ProductMatchID = baseProductMatch.ProductMatchID;
                toMatchProductMatch.isMatched = true;
                toMatchProductMatch.MatchStatus = (int)MatchStatuses.Accepted;
                unit.Service<ProductMatch>().Create(toMatchProductMatch);
              }
              else
              {
                toMatchProductMatch.MatchStatus = (int)MatchStatuses.Accepted;
                toMatchProductMatch.ProductMatchID = baseProductMatch.ProductMatchID;
                toMatchProductMatch.isMatched = true;
              }
            }
          }
          else
          {
            foreach (int UnMatchProductID in ListOfProductAndMatchIDs.NewMatchProductIDs)
            {
              ProductMatch toUnMatchProductMatch = unit.Service<ProductMatch>().Get(x => x.ProductID == UnMatchProductID);
              if (toUnMatchProductMatch != null)
              {
                toUnMatchProductMatch.isMatched = false;
                toUnMatchProductMatch.MatchStatus = (int)MatchStatuses.Declined;
              }
            }
          }
          unit.Save();
        }
        return Success("Product is successfully matched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to match the product.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnMatchMultipleVendorProductByMasterGroupMapping(string ListOfMasterGroupMappingIDsJson)
    {

      var unMatchList = JsonConvert.DeserializeObject<List<MasterGroupMappingUnMatchItem>>(ListOfMasterGroupMappingIDsJson);

      try
      {

        using (var unit = GetUnitOfWork())
        {
          foreach (var toUnMatch in unMatchList)
          {
            var masterGroupMappingIds = toUnMatch.MasterGroupMappingIDs.ToArray();
            unit
              .Service<MasterGroupMappingProduct>()
              .Update(m => m.IsProductMapped = false, x => x.ProductID == toUnMatch.ProductID && masterGroupMappingIds.Contains(x.MasterGroupMappingID));
            //.Delete(x => x.ProductID == toUnMatch.ProductID && masterGroupMappingIds.Contains(x.MasterGroupMappingID));
          }
          unit.Save();
        }
        return Success("Product group is successfully unmatched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to unmatch the product group.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UnMatchVendorProductAndMasterGroupMapping(string ListOfMasterGroupMappingIDsJson)
    {
      var ListOfIDsModel = new
      {
        ProductID = 0,
        MasterGroupMappingIDs = new[] { 0 }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfMasterGroupMappingIDsJson, ListOfIDsModel);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var masterGroupMappingIds = ListOfIDs.MasterGroupMappingIDs.ToArray();
          unit.Service<MasterGroupMappingProduct>().Update(x => x.IsProductMapped = false, y => y.ProductID == ListOfIDs.ProductID && masterGroupMappingIds.Contains(y.MasterGroupMappingID));
          unit.Save();
        }
        return Success("Product group is successfully unmatched.");
      }
      catch (Exception e)
      {
        return Failure("Failed to unmatch the product group.", e);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateMatchProductsWizard(string Products)
    {
      var rows = JsonConvert.DeserializeObject<List<ProductMatchWizardItems>>(Products);
      var firstRow = rows.First();

      if (firstRow != null)
      {
        using (var unit = GetUnitOfWork())
        {
          var productMatch = unit.Scope.Repository<ProductMatch>().GetSingle(x => x.ProductID == firstRow.ProductID);

          if (productMatch != null)
          {
            var matches = unit.Scope.Repository<ProductMatch>().GetAll(x => x.ProductMatchID == productMatch.ProductMatchID).ToList();

            matches.ForEach(m =>
            {
              var match = rows.Where(x => x.ProductID == m.ProductID);

              m.MatchStatus = ((int)MatchStatuses.Declined);

              if (match.Any(x => !x.IsMatch))
              {
                m.isMatched = false;
                m.MatchStatus = (int)MatchStatuses.Declined;
              }
              else if (match.Any(x => x.IsMatch))
              {
                m.isMatched = true;
                m.MatchStatus = (int)MatchStatuses.Accepted;
              }
            });
          }

          unit.Save();
        }
      }

      return Success("Succesfully save product matches.");
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateMatchProductWizard(int ProductID, Boolean IsMatch)
    {
      return Update<ProductMatch>(pm => pm.ProductID == ProductID, (unit, pm) =>
       {
         pm.isMatched = IsMatch;
         pm.MatchStatus = IsMatch ? ((int)MatchStatuses.Accepted) : ((int)MatchStatuses.Declined);
       });
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult SetProductMatchAsPrimary(int productID)
    {
      try
      {

        using (var unit = GetUnitOfWork())
        {
          var product = unit.Service<ProductMatch>().Get(x => x.ProductID == productID);

          var productMatches = unit.Service<ProductMatch>().GetAll(x => x.ProductMatchID == product.ProductMatchID);

          foreach (var prodMatch in productMatches)
          {
            prodMatch.Primary = false;
          }

          product.Primary = true;

          unit.Save();

          return Success("Succesfully updated product match.");
        }
      }
      catch (Exception e)
      {
        return Failure("Could not update product match. Error: " + e.Message);
      }

    }

    #endregion

    #endregion

    #region Tabblad "Matched Vendor Products"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateVendorProduct(int ConcentratorNumber, int MasterGroupMappingID, Boolean ProductControleVariable)
    {
      if (ProductControleVariable)
      {
        using (var unit = GetUnitOfWork())
        {
          var match = unit.Scope.Repository<ProductMatch>().GetSingle(x => x.ProductID == ConcentratorNumber);

          if (match != null)
          {
            var matches = (from pm in unit.Scope.Repository<ProductMatch>().GetAll(x => x.ProductID != ConcentratorNumber && x.ProductMatchID == match.ProductMatchID && x.isMatched == true)
                           join mg in unit.Scope.Repository<MasterGroupMappingProduct>().GetAll() on pm.ProductID equals mg.ProductID
                           select mg).ToList();

            matches.ForEach(m =>
            {
              if (m.ProductID != ConcentratorNumber)
              {
                m.IsApproved = true;
                AttributesChecked(m.ProductID, true);
              }
            });
            unit.Save();
          }
        }
      }

      var productControle = ProductControl(ConcentratorNumber, MasterGroupMappingID);
      if (!ProductControleVariable || productControle)
      {
        return Update<MasterGroupMappingProduct>(p => p.MasterGroupMappingID == MasterGroupMappingID && p.ProductID == ConcentratorNumber,
          (unit, masterGroupMappingProduct) =>
          {
            if (ProductControleVariable)
            {
              if (productControle)
              {
                if (masterGroupMappingProduct.IsApproved == false)
                  AttributesChecked(ConcentratorNumber, true);

                masterGroupMappingProduct.IsApproved = true;
              }
            }
            else
            {
              masterGroupMappingProduct.IsApproved = false;
              AttributesChecked(ConcentratorNumber, false);
            }
          });
      }
      else
      {
        return Failure("Product cannot be control. One or more conditions are missed!");
      }
    }

    private ProductAttributeValue GetAttributeCheckStep(int concentratorNumber, IUnitOfWork unit)
    {
      int mgmAttributeCheckID = GetMGMAttributeCheck();
      var pav = unit.Scope.Repository<ProductAttributeValue>().GetSingle(x => x.AttributeID == mgmAttributeCheckID && x.ProductID == concentratorNumber);

      if (pav == null)
      {
        pav = new ProductAttributeValue()
        {
          AttributeID = mgmAttributeCheckID,
          ProductID = concentratorNumber,
          Value = "Step 1 - Product"
        };
        unit.Scope.Repository<ProductAttributeValue>().Add(pav);
        unit.Save();
      }

      return pav;
    }

    private int GetMGMAttributeCheck()
    {
      using (var unit = GetUnitOfWork())
      {
        int _concentratorVendorID = -1;

        if (!int.TryParse(ConfigurationManager.AppSettings["ConcentratorVendorID"], out _concentratorVendorID))
          throw new Exception("ConcentratorVendorID is not set in Config");

        var _concentratorVendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == _concentratorVendorID);

        if (_concentratorVendor == null)
          throw new Exception("ConcentratorVendor is not valid");

        var checkAttribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(x => x.AttributeCode == "MGMAttributeCheck");

        if (checkAttribute == null)
        {
          var checkAttributeGroup = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetSingle(x => x.GroupCode == "MGM");
          var languages = unit.Scope.Repository<Language>().GetAll().ToList();

          if (checkAttributeGroup == null)
          {
            checkAttributeGroup = new ProductAttributeGroupMetaData()
            {
              Index = 0,
              GroupCode = "MGM",
              VendorID = _concentratorVendorID
            };
            unit.Scope.Repository<ProductAttributeGroupMetaData>().Add(checkAttributeGroup);

            languages.ForEach(language =>
            {
              var attributeGroupName = new ProductAttributeGroupName()
              {
                Name = "MGM attributes",
                LanguageID = language.LanguageID,
                ProductAttributeGroupMetaData = checkAttributeGroup
              };
              unit.Scope.Repository<ProductAttributeGroupName>().Add(attributeGroupName);
            });
          }

          checkAttribute = new ProductAttributeMetaData()
          {
            AttributeCode = "MGMAttributeCheck",
            ProductAttributeGroupMetaData = checkAttributeGroup,
            IsVisible = false,
            NeedsUpdate = false,
            VendorID = _concentratorVendorID,
            IsSearchable = false,
            Mandatory = false,
            IsConfigurable = false,
            IsSlider = false
          };
          unit.Scope.Repository<ProductAttributeMetaData>().Add(checkAttribute);

          languages.ForEach(language =>
          {
            var attributeGroupName = new ProductAttributeName()
            {
              Name = "MGM attribute check",
              LanguageID = language.LanguageID,
              ProductAttributeMetaData = checkAttribute
            };
            unit.Scope.Repository<ProductAttributeName>().Add(attributeGroupName);
          });

          unit.Save();
        }

        return checkAttribute.AttributeID;
      }
    }

    private void AttributesChecked(int concentratorNumber, bool approved)
    {
      using (var unit = GetUnitOfWork())
      {
        var currentStep = GetAttributeCheckStep(concentratorNumber, unit);

        if (approved)
        {
          if (currentStep.Value == "Step 1 - Product")
            currentStep.Value = "Step 2 - Attribute";
          else if (currentStep.Value == "Step 2 - Attribute")
            currentStep.Value = "Step 3 - Primary's / Finished";
        }
        else
          unit.Scope.Repository<ProductAttributeValue>().Delete(currentStep);

        unit.Save();
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMultipleMatchedMasterGroupMappingsAndVendorProducts(string data, int? ConnectorID)
    {
      var rows = JsonConvert.DeserializeObject<List<MatchedMasterGroupMappingProduct>>(data);


      using (var unit = GetUnitOfWork())
      {

        var listOfMatchedMasterGroupMappings = (from rec in rows
                                                from x in unit.Service<MasterGroupMapping>()
         .GetAll(x => x.MasterGroupMappingProducts.Any(m => m.ProductID == rec.ProductID) && (ConnectorID.HasValue ? x.ConnectorID == ConnectorID.Value : ConnectorID == null))
                                                select new
                                                {
                                                  MGM = x,
                                                  rec.ProductID,
                                                  MGMID = rec.MasterGroupMappingID
                                                })
          .ToList()
          .Select(x =>
          {
            int masterGroupMappingID = x.MGM.MasterGroupMappingID;
            bool hasParent = true;
            var MasterGroupMappingPadName = "";
            do
            {
              var masterGroupmapping = unit.Service<MasterGroupMapping>()
                .Get(m => m.MasterGroupMappingID == masterGroupMappingID);

              var tempMasterGroupMappingPadName = masterGroupmapping
                .MasterGroupMappingLanguages.Try(l => l.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name, "PRODUCTGROUP NAME EMPTY!");

              MasterGroupMappingPadName = tempMasterGroupMappingPadName + MasterGroupMappingPadName;

              if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
              {
                masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
                MasterGroupMappingPadName = "->" + MasterGroupMappingPadName;
              }
              else
              {
                hasParent = false;
              }
            } while (hasParent);

            return new
            {
              MasterGroupMappingID = x.MGM.MasterGroupMappingID,
              Check = x.MGMID.HasValue ? (x.MGMID.Value == x.MGM.MasterGroupMappingID ? true : false) : false,
              MasterGroupMappingPad = MasterGroupMappingPadName,
              ConcentratorID = x.ProductID
            };
          })
          .OrderBy(o => o.MasterGroupMappingPad)
          ;

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMatchedMasterGroupMappingAndVendorProduct(int ProductID, int? GridMasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfMatchedMasterGroupMappings = unit.Service<MasterGroupMapping>()
          .GetAll(x => x.MasterGroupMappingProducts.Any(m => m.ProductID == ProductID) && x.ConnectorID == null)
          .ToList()
          .Select(x =>
          {
            int masterGroupMappingID = x.MasterGroupMappingID;
            bool hasParent = true;
            var MasterGroupMappingPadName = "";
            do
            {
              var masterGroupmapping = unit.Service<MasterGroupMapping>()
                .Get(m => m.MasterGroupMappingID == masterGroupMappingID);

              var tempMasterGroupMappingPadName = masterGroupmapping
                .MasterGroupMappingLanguages
                .Where(c => c.LanguageID == Client.User.LanguageID)
                .Select(n => n.Name)
                .FirstOrDefault();

              MasterGroupMappingPadName = tempMasterGroupMappingPadName + MasterGroupMappingPadName;

              if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
              {
                masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
                MasterGroupMappingPadName = "->" + MasterGroupMappingPadName;
              }
              else
              {
                hasParent = false;
              }
            } while (hasParent);

            return new
            {
              MasterGroupMappingID = x.MasterGroupMappingID,
              Check = GridMasterGroupMappingID.HasValue ? (GridMasterGroupMappingID.Value == x.MasterGroupMappingID ? true : false) : false,
              MasterGroupMappingPad = MasterGroupMappingPadName
            };
          })
          .OrderBy(o => o.MasterGroupMappingPad)
          ;

        return Json(new { results = listOfMatchedMasterGroupMappings });
      }
    }

    #endregion

    #region Tabblad "Connector Mapping"

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult AddMasterGroupMappingToConnectorMapping(string SaveParamsJson)
    {
      var SaveParamsModel = new
      {
        ConnectorID = 0,
        copyChildren = false,
        MasterGroupMappingID = 0,
        ParentConnectorMappingID = 0
      };
      var SaveParams = JsonConvert.DeserializeAnonymousType(SaveParamsJson, SaveParamsModel);

      try
      {
        if (SaveParams.ConnectorID > 0 && SaveParams.MasterGroupMappingID > 0)
        {
          using (var unit = GetUnitOfWork())
          {
            CopyMasterGroupMappingToConnectorMapping(SaveParams.MasterGroupMappingID, SaveParams.ParentConnectorMappingID, SaveParams.ConnectorID, SaveParams.copyChildren, unit);
          }
          return Success("Successfully created object", isMultipartRequest: true, needsRefresh: false);
        }
        return Failure("Failed to create Product Group Mapping!");
      }
      catch (Exception e)
      {
        return Failure("Failed to create object ", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult DeleteProductGroupMapping(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (id < 0)
            return Failure("Failed to delete product group mapping, it is not possible to remove the root");
          else
          {
            DeleteProductGroupMappingHierarchy(id, unit);
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

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateProductGroupMapping(string SaveParamsJson)
    {
      var SaveParamsModel = new
      {
        ProductGroupMappingID = 0,
        ParentProductGroupMappingID = 0,
        childrenOfParentProductGroupMappingIDs = new[] { 0 }
      };
      var SaveParams = JsonConvert.DeserializeAnonymousType(SaveParamsJson, SaveParamsModel);

      try
      {
        if (SaveParams.ProductGroupMappingID > 0)
        {
          using (var unit = GetUnitOfWork())
          {
            var productGroupMapping = unit
              .Service<ProductGroupMapping>()
              .Get(x => x.ProductGroupMappingID == SaveParams.ProductGroupMappingID);
            if (productGroupMapping != null)
            {
              if (productGroupMapping.ParentProductGroupMappingID != SaveParams.ParentProductGroupMappingID)
              {
                if (SaveParams.ParentProductGroupMappingID > 0)
                {
                  productGroupMapping.ParentProductGroupMappingID = SaveParams.ParentProductGroupMappingID;
                }
                else
                {
                  productGroupMapping.ParentProductGroupMappingID = null;
                }
              }

              int counter = 0;
              foreach (int productGroupMappingID in SaveParams.childrenOfParentProductGroupMappingIDs)
              {
                var childProductGroupMapping = unit
                  .Service<ProductGroupMapping>()
                  .Get(x => x.ProductGroupMappingID == productGroupMappingID);

                childProductGroupMapping.Score = counter;
                counter++;
              }
              unit.Save();
            }
            else
            {
              return Failure("Failed to create Master Group Mapping!");
            }
          }
          return Success("Successfully created object", isMultipartRequest: true, needsRefresh: false);
        }
        return Failure("Failed to create Master Group Mapping!");
      }
      catch (Exception e)
      {
        return Failure("Failed to create object ", e, isMultipartRequest: true);
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetConnectorMappingSettings(int MasterGroupMappingID)
    {
      //todo:
      //date: 24-4-2014
      //ticket: 66732
      //On this moment of writing there are two different ways to get mgm settings --> for magento: MagentoProductGroupSetting,  and the new implementation: (MasterGroupMappingSetting) (now used for the new dixons connector)
      //In the future the new implementation needs to be active for all connectors and the magento settings needs to be removed

      using (var unit = GetUnitOfWork())
      {
        MasterGroupMapping masterGroupMapping = unit
          .Service<MasterGroupMapping>()
          .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

        if (masterGroupMapping == null)
        {
          return Failure("Product Group Mapping does not exist!");
        }
        else
        {
          ArrayList listOfSettings = new ArrayList();

          var MasterGroupMappingSettings = new
          {
            Name = "MasterGroupMappingSettings",
            FlattenHierarchy = masterGroupMapping.FlattenHierarchy,
            FilterByParentGroup = masterGroupMapping.FilterByParentGroup,
            ExportID = masterGroupMapping.ExportID
          };

          listOfSettings.Add(MasterGroupMappingSettings);

          var connectorGroupSettings =
          masterGroupMapping.Connectors.Select(c => new
          {
            Name = "ConnectorGroupSetting",
            ConnectorName = c.Name,
            ConnectorID = c.ConnectorID
          }
          ).ToArray();

          listOfSettings.AddRange(connectorGroupSettings);

          #region Image Settings
          //Add default
          listOfSettings.Add(new
          {
            Name = "ImageConnector",
            ConnectorName = "Default"
          });

          //current connector
          listOfSettings.Add(new
          {
            Name = "ImageConnector",
            ConnectorName = masterGroupMapping.Connector.Name,
            ConnectorID = masterGroupMapping.Connector.ConnectorID,
            Languages = masterGroupMapping.Connector.ConnectorLanguages.Select(x => new
            {
              Country = x.Country,
              Name = x.Language.Name,
              Code = x.Language.DisplayCode,
              LanguageID = x.LanguageID
            }).ToList()
          });

          //add child connectors for image setting overview
          var imageConnectors = masterGroupMapping.Connector.ChildConnectors.ToList().Select(c => new
          {
            Name = "ImageConnector",
            ConnectorName = c.Name,
            ConnectorID = c.ConnectorID,
            Languages = c.ConnectorLanguages.Select(x => new
            {
              Country = x.Country,
              Name = x.Language.Name,
              Code = x.Language.DisplayCode,
              LanguageID = x.LanguageID
            }).ToList()
          }).ToArray();

          listOfSettings.AddRange(imageConnectors);
          #endregion

          if (masterGroupMapping.ConnectorID.HasValue && masterGroupMapping.ConnectorID.Value > 0)
          {
            //see first comment in this method
            if (masterGroupMapping.Connector.ConnectorSystemID.HasValue && masterGroupMapping.Connector.ConnectorSystemID.Value == 2)
            {
              MagentoProductGroupSetting magentoProductGroupSetting = unit
                .Service<MagentoProductGroupSetting>()
                .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

              if (magentoProductGroupSetting == null)
              {
                var MagentoSettings = new
                {
                  Name = "MagentoSettings",
                  ListView = false,
                  DisableInNaviagtion = false,
                  DisableInProductGroup = false
                };
                listOfSettings.Add(MagentoSettings);
              }
              else
              {
                var MagentoSettings = new
                {
                  Name = "MagentoSettings",
                  ListView = magentoProductGroupSetting.IsAnchor.HasValue ? magentoProductGroupSetting.IsAnchor.Value : false,
                  DisableInNaviagtion = magentoProductGroupSetting.ShowInMenu.HasValue ? magentoProductGroupSetting.ShowInMenu.Value : false,
                  DisableInProductGroup = magentoProductGroupSetting.DisabledMenu.HasValue ? magentoProductGroupSetting.DisabledMenu.Value : false,
                  PageLayoutID = masterGroupMapping.MagentoPageLayoutID
                };
                listOfSettings.Add(MagentoSettings);
              }
            }
            //see first comment in this method
            //else if (masterGroupMapping.Connector.ConnectorSystemID.HasValue && masterGroupMapping.Connector.ConnectorSystemID.Value == 6) //is wpos connectors
            //{
            //  var wposSettings = unit.Service<MasterGroupMappingSetting>().GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID).ToList();

            //  foreach (var setting in wposSettings)
            //  {
            //    bool isBooleanSetting = false;

            //    isBooleanSetting = bool.TryParse(setting.Value, out isBooleanSetting);

            //    if (isBooleanSetting)
            //    {
            //      var wposSetting = new
            //      {
            //        Name = "WposSettings",
            //        Code = setting.Code,
            //        SettingValue = bool.Parse(setting.Value)
            //      };
            //      listOfSettings.Add(wposSetting);
            //    }
            //  }
            //}
          }

          //The generic stuff...
          var masterGroupMappingSettings = (from setting in unit.Service<MasterGroupMappingSetting>().GetAll()
                                            join values in unit.Service<MasterGroupMappingSettingValue>().GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID)
                                                                                        on setting.MasterGroupMappingSettingID equals values.MasterGroupMappingSettingID
                                            into settingWithValues
                                            from settingWithValue in settingWithValues.DefaultIfEmpty()

                                            join defaultValue in unit.Service<MasterGroupMappingSettingTemplate>().GetAll()
                                                                                        on setting.MasterGroupMappingSettingID equals defaultValue.MasterGroupMappingSettingID
                                            into defaultValues
                                            from defaults in defaultValues.DefaultIfEmpty()

                                            select new
                                            {
                                              setting.MasterGroupMappingSettingID,
                                              setting.Name,
                                              setting.Type,
                                              setting.Group,
                                              settingWithValue.LanguageID,
                                              Value = string.IsNullOrEmpty(settingWithValue.Value) ? defaults.DefaultValue : settingWithValue.Value,
                                            }).ToList();

          foreach (var setting in masterGroupMappingSettings)
          {
            listOfSettings.Add(new
            {
              SettingID = setting.MasterGroupMappingSettingID,
              Group = setting.Group,
              Name = setting.Name,
              Type = setting.Type,
              Value = setting.Value,
              LanguageID = setting.LanguageID
            });
          }
          return Json(new { data = listOfSettings });
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateConnectorMappingSettings(string SaveParamsJson)
    {
      var listOfParams = JsonConvert.DeserializeObject<UpdateConnectorMappingSetting>(SaveParamsJson);

      using (var unit = GetUnitOfWork())
      {
        MasterGroupMapping masterGroupMapping = unit
          .Service<MasterGroupMapping>()
          .Get(x => x.MasterGroupMappingID == listOfParams.MasterGroupMappingID);

        if (masterGroupMapping == null)
        {
          return Failure("Connector Mapping does not exist!");
        }
        else
        {
          try
          {
            masterGroupMapping.FlattenHierarchy = listOfParams.FlattenHierachy;
            masterGroupMapping.FilterByParentGroup = listOfParams.FilterByParentGroup;
            if (listOfParams.ExportID.HasValue && listOfParams.ExportID.Value > 0)
            {
              masterGroupMapping.ExportID = listOfParams.ExportID.Value;
            }
            else
            {
              masterGroupMapping.ExportID = null;
            }

            masterGroupMapping.MagentoPageLayoutID = listOfParams.PageLayoutID;

            if (masterGroupMapping.ConnectorID.HasValue && masterGroupMapping.ConnectorID.Value > 0 &&
              masterGroupMapping.Connector.ConnectorSystemID.HasValue &&
              masterGroupMapping.Connector.ConnectorSystemID.Value == 2)
            {
              var magentoProductGroupSetting = unit
               .Service<MagentoProductGroupSetting>()
               .Get(x => x.MasterGroupMappingID == listOfParams.MasterGroupMappingID);
              if (magentoProductGroupSetting == null)
              {
                magentoProductGroupSetting = new MagentoProductGroupSetting()
                {
                  ProductGroupmappingID = unit.Service<ProductGroupMapping>().GetAll().First().ProductGroupMappingID,
                  IsAnchor = listOfParams.ListView,
                  ShowInMenu = listOfParams.DisableInNavigation,
                  DisabledMenu = listOfParams.DisableInProductGroup,
                  MasterGroupMappingID = listOfParams.MasterGroupMappingID
                };
                unit.Service<MagentoProductGroupSetting>().Create(magentoProductGroupSetting);
              }
              else
              {
                magentoProductGroupSetting.IsAnchor = listOfParams.ListView;
                magentoProductGroupSetting.ShowInMenu = listOfParams.DisableInNavigation;
                magentoProductGroupSetting.DisabledMenu = listOfParams.DisableInProductGroup;
              }
            }

            listOfParams.ConnectorGroupSettings.ForEach(x =>
            {

              var connectorGroup = masterGroupMapping.Connectors.FirstOrDefault(y => y.ConnectorID == x.ConnectorID);

              if (!x.IsActive)
              {

                if (connectorGroup == null)
                {
                  var connector = unit.Service<Connector>().Get(c => c.ConnectorID == x.ConnectorID);
                  masterGroupMapping.Connectors.Add(connector);
                }
              }
              else
              {
                if (connectorGroup != null)
                  masterGroupMapping.Connectors.Remove(connectorGroup);
              }

            });

            //Generic Settings
            foreach (var masterGroupMappingSetting in listOfParams.GenericSettings)
            {
              var setting = masterGroupMapping.MasterGroupMappingSettingValues.FirstOrDefault(x => x.MasterGroupMappingSettingID == masterGroupMappingSetting.SettingID);
              if (setting == null)
              {
                if (!string.IsNullOrEmpty(masterGroupMappingSetting.Value))
                {
                  masterGroupMapping.MasterGroupMappingSettingValues.Add(new MasterGroupMappingSettingValue
                  {
                    MasterGroupMappingSettingID = masterGroupMappingSetting.SettingID,
                    Value = masterGroupMappingSetting.Value
                  });
                }
              }
              else
              {
                setting.Value = masterGroupMappingSetting.Value;
              }
            }

            //if (masterGroupMapping.ConnectorID.HasValue && masterGroupMapping.ConnectorID.Value > 0 &&
            //  masterGroupMapping.Connector.ConnectorSystemID.HasValue &&
            //  masterGroupMapping.Connector.ConnectorSystemID.Value == 6)//wpos connectors
            //{
            //  if (listOfParams.WposConnectorMappingSettings.Count() > 0)
            //  {
            //    foreach (var wposSetting in listOfParams.WposConnectorMappingSettings)
            //    {
            //      var wposSettingFromDB = masterGroupMapping.MasterGroupMappingSettings.FirstOrDefault(x => x.Code.Equals(wposSetting.Code));

            //      wposSettingFromDB.Value = wposSetting.SettingValue;
            //    }
            //  }
            //}

            unit.Save();
            return Success("Successfully updated Connector Mapping settings");
          }
          catch
          {
            return Failure("Failed to update Connector Mapping settings!");
          }
        }
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult SetSourceMasterGroupMapping(int MasterGroupMappingID, int ProductGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (MasterGroupMappingID < 0 || ProductGroupMappingID < 0)
            return Failure("Failed to set Source Master Group Mapping");
          else
          {
            MasterGroupMapping productGroupMapping = unit
              .Service<MasterGroupMapping>()
              .Get(x => x.MasterGroupMappingID == ProductGroupMappingID);

            if (productGroupMapping == null)
            {
              return Failure("Failed to set Source Master Group Mapping. Product Group Mapping ID does not exist.");
            }
            else
            {
              MasterGroupMapping masterGroupMapping = unit
                .Service<MasterGroupMapping>()
                .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

              if (masterGroupMapping == null)
              {
                return Failure("Failed to set Source Master Group Mapping. Master Group Mapping ID does not exist.");
              }
              else
              {
                productGroupMapping.SourceMasterGroupMappingID = MasterGroupMappingID;
              }
            }
          }
          unit.Save();

          return Success("Successfully set source master group mapping");
        }
        catch (Exception ex)
        {
          return Failure("Failed to set Source Master Group Mapping. " + ex.Message);
        }
      }
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetConnector(int connectorID)
    {
      var connector = GetObject<Connector>(c => c.ConnectorID == connectorID);
      string parentConnectorName = null;
      if (connector.ParentConnectorID.HasValue && connector.ParentConnectorID.Value > 0)
      {
        parentConnectorName = connector.ParentConnector.Name;
      }

      return Json(new
      {
        success = true,
        data = new
        {
          connector.Name,
          connector.ParentConnectorID,
          ParentConnectorName = parentConnectorName
        }
      });
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetMasterGroupMappingImage(int MasterGroupMappingID, string type, int? connectorID, int? languageID)
    {
      MasterGroupMappingMediaType mediaType;
      if (Enum.TryParse<MasterGroupMappingMediaType>(type, out mediaType))
      {
        using (var unit = GetUnitOfWork())
        {
          var masterGroupMappingMedias = unit.Service<MasterGroupMappingMedia>().GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID && x.ImageTypeID == (int)mediaType);
          var connectorLanguageImage = masterGroupMappingMedias.FirstOrDefault(x => ((!x.ConnectorID.HasValue && !connectorID.HasValue) || (x.ConnectorID == connectorID)) &&
                                                                                    ((!x.LanguageID.HasValue) && !languageID.HasValue || (x.LanguageID == languageID)));

          var results = new ArrayList();
          var prodPath = ConfigurationManager.AppSettings["FTPMasterGroupMappingMediaPath"];
          if (prodPath == null) return Failure("MasterGroupMapping media base path not configured");


          if (connectorLanguageImage != null && !string.IsNullOrEmpty(connectorLanguageImage.ImagePath))
          {
            results.Add(new { ImagePath = Path.Combine(prodPath, connectorLanguageImage.ImagePath) });
          }

          return Json(new { results });
        }
      }
      return Failure("Couldn't find media type: " + type);
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UploadImage(int MasterGroupMappingID, string type, int? connectorID, int? languageID)
    {
      using (var unit = GetUnitOfWork())
      {
        //TODO move to service
        var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
        if (masterGroupMapping != null)
        {
          MasterGroupMappingMediaType mediaType;
          if (Enum.TryParse<MasterGroupMappingMediaType>(type, out mediaType))
          {
            var path = ConfigurationManager.AppSettings["FTPMediaDirectory"];
            var prodPath = ConfigurationManager.AppSettings["FTPMasterGroupMappingMediaPath"];

            var innerPath = Path.Combine(path, prodPath);

            if (!Directory.Exists(innerPath))
              Directory.CreateDirectory(innerPath);

            var File = Request.Files.Get("file");
            if (File != null)
            {
              var filename = string.Format("{0}_{1}{2}{3}", MasterGroupMappingID
                , DateTime.Now.ToString("yyyyMMddHHmmssff")
                , (mediaType == MasterGroupMappingMediaType.Thumbnail) ? "_thumb" : string.Empty
                , Path.GetExtension(File.FileName));

              var fullPath = Path.Combine(innerPath, filename);

              var uploadDefaultImage = (!languageID.HasValue && !connectorID.HasValue);

              MasterGroupMappingMedia masterGroupMappingMedia;
              if (uploadDefaultImage)
              {
                masterGroupMappingMedia = masterGroupMapping.MasterGroupMappingMedias.FirstOrDefault(x => !x.LanguageID.HasValue
                                                                                                       && !x.ConnectorID.HasValue
                                                                                                       && x.ImageTypeID == (int)mediaType);
              }
              else
              {
                masterGroupMappingMedia = masterGroupMapping.MasterGroupMappingMedias.FirstOrDefault(x => x.LanguageID == languageID
                                                                                                             && x.ConnectorID == connectorID
                                                                                                             && x.ImageTypeID == (int)mediaType);
              }

              if (masterGroupMappingMedia != null)
              {
                var actualPath = Path.Combine(path, masterGroupMappingMedia.ImagePath);
                if (System.IO.File.Exists(actualPath))
                  System.IO.File.Delete(actualPath);

                masterGroupMappingMedia.ImagePath = filename;
              }
              else
              {
                masterGroupMappingMedia = new MasterGroupMappingMedia()
                {
                  MasterGroupMappingID = MasterGroupMappingID,
                  ImageTypeID = (int)mediaType,
                  LanguageID = languageID,
                  ConnectorID = connectorID,
                  ImagePath = filename
                };
                unit.Service<MasterGroupMappingMedia>().Create(masterGroupMappingMedia);
              }

              using (var stream = File.InputStream)
              {
                //Download file
                byte[] buff = new byte[stream.Length];
                stream.Read(buff, 0, (int)stream.Length);
                System.IO.File.WriteAllBytes(fullPath, buff);
              }
            }
            unit.Save();
          }
          else
          {
            return Failure("Unknown media type " + type);
          }
        }
        else
        {
          return Failure("Mastergroupmapping not found");
        }
      }
      return Success("Sucessfully uploaded file");
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult DeleteMasterGroupMappingImage(int MasterGroupMappingID, string type, int? connectorID, int? languageID)
    {
      using (var unit = GetUnitOfWork())
      {
        MasterGroupMappingMediaType mediaType;
        if (Enum.TryParse<MasterGroupMappingMediaType>(type, out mediaType))
        {
          var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
          var masterGroupMappingMedia = masterGroupMapping.MasterGroupMappingMedias.FirstOrDefault(x => x.ImageTypeID == (int)mediaType
                                                                                                     && x.ConnectorID == connectorID
                                                                                                     && x.LanguageID == languageID);

          if (masterGroupMappingMedia != null)
          {
            //delete image from drive
            var path = ConfigurationManager.AppSettings["FTPMediaDirectory"];
            var fileName = masterGroupMappingMedia.ImagePath;

            var file = Path.Combine(path, fileName);

            if (System.IO.File.Exists(file))
              System.IO.File.Delete(file);

            unit.Service<MasterGroupMappingMedia>().Delete(masterGroupMappingMedia);
            unit.Save();
            return Success("Successfully removed image");
          }
        }
        return Failure("Something whent wrong deleting the image.");
      }
    }

    // todo: security
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfConnectorMappingProducts(int MasterGroupMappingID, int? ConnectorID)
    {
      int _defaultLanguage;

      if (!int.TryParse(ConfigurationManager.AppSettings["DefaultLanguageID"], out _defaultLanguage))
        throw new Exception("DefaultLanguage is not set in Config");

      var connectorID = Client.User.ConnectorID ?? ConnectorID;
      if (!connectorID.HasValue)
        return Failure("Connector must be supplied");

      return List(unit => (from c in unit.Service<ContentProductGroup>().GetAll(c => c.MasterGroupMappingID == MasterGroupMappingID)
                           let desc = c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID)
                           let desc2 = c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == _defaultLanguage)
                           where c.ConnectorID == connectorID
                           select new
                           {
                             c.ProductID,
                             ProductName = desc.ProductName ?? desc2.ProductName,
                             ShortDescription = desc.ShortContentDescription ?? desc2.ShortContentDescription,
                             LongDescription = desc.LongContentDescription ?? desc2.LongContentDescription,
                             VendorItemNumber = c.Product.VendorItemNumber,
                             IsConfigurable = c.Product.IsConfigurable,
                             c.IsExported,
                             c.MasterGroupMappingID,
                             c.ContentProductGroupID
                           }).AsQueryable());

    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult UpdateConnectorMappingProducts(int id, bool? IsExported)
    {
      using (var unit = GetUnitOfWork())
      {
        var record = unit.Service<ContentProductGroup>().Get(x => x.ContentProductGroupID == id);
        if (record != null)
        {
          if (IsExported.HasValue)
          {
            record.IsExported = IsExported.Value;

            //quick fix for setting products for different child connectors
            try
            {
              var childrenCPGs = unit.Service<ContentProductGroup>().GetAll(c => c.MasterGroupMappingID == record.MasterGroupMappingID && c.ProductID == record.ProductID && c.ConnectorID != record.ConnectorID);
              foreach (var chilCpg in childrenCPGs)
              {
                chilCpg.IsExported = IsExported.Value;
              }
            }
            catch { }
          }

          unit.Save();
          return Success("Succesfully updated product");
        }
        return Failure("Product not found");
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult SetProductExportFlag(int ContentProductGroupID)
    {
      return Update<ContentProductGroup>(x => x.ContentProductGroupID == ContentProductGroupID);
    }

    [RequiresAuthentication(Functionalities.GetMasterGroupMappingDescription)]
    public ActionResult GetMasterGroupMappingDescriptions(int MasterGroupMappingID)
    {
      return List(unit => (from l in unit.Service<Language>().GetAll()
                           join p in unit.Service<MasterGroupMappingDescription>().GetAll().Where(c => c.MasterGroupMappingID == MasterGroupMappingID) on l.LanguageID equals p.LanguageID into temp
                           from tr in temp.DefaultIfEmpty()
                           select new
                           {
                             l.LanguageID,
                             Language = l.Name,
                             tr.Description,
                             MasterGroupMappingID = (tr == null ? MasterGroupMappingID : tr.MasterGroupMappingID)
                           }));
    }

    [ValidateInput(false)]
    [RequiresAuthentication(Functionalities.SetMasterGroupMappingDescription)]
    public ActionResult SetMasterGroupMappingDescriptions(int _LanguageID, int MasterGroupMappingID, string Description)
    {
      using (var unit = GetUnitOfWork())
      {
        var masterGroupMappingDescription = unit.Service<MasterGroupMappingDescription>().Get(x => x.LanguageID == _LanguageID && x.MasterGroupMappingID == MasterGroupMappingID);

        if (masterGroupMappingDescription == null)
        {
          unit.Service<MasterGroupMappingDescription>().Create(new MasterGroupMappingDescription
          {
            Description = Description,
            LanguageID = _LanguageID,
            MasterGroupMappingID = MasterGroupMappingID
          });
        }
        else
        {
          masterGroupMappingDescription.Description = Description;
        }

        unit.Save();
      }
      return Success("Successfully updated description");
    }

    #region Menu "SEO Texts"
    [RequiresAuthentication(Functionalities.ManageSeoTexts)]
    public ActionResult GetSeoTexts(int masterGroupMappingID, int? languageID, int? ConnectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        if (languageID == null)
          languageID = Client.User.LanguageID;


        var seoTexts = unit.Service<SeoTexts>().GetAll(x => x.MasterGroupMappingID == masterGroupMappingID && x.LanguageID == languageID && x.ConnectorID == ConnectorID);

        return Json(new
        {
          success = true,
          data = new
          {
            Description = seoTexts.Where(x => x.DescriptionType == (int)SeoDescriptionTypes.Description).Select(x => x.Description).FirstOrDefault(),
            Description2 = seoTexts.Where(x => x.DescriptionType == (int)SeoDescriptionTypes.Description2).Select(x => x.Description).FirstOrDefault(),
            Description3 = seoTexts.Where(x => x.DescriptionType == (int)SeoDescriptionTypes.Description3).Select(x => x.Description).FirstOrDefault(),
            Meta_title = seoTexts.Where(x => x.DescriptionType == (int)SeoDescriptionTypes.Meta_title).Select(x => x.Description).FirstOrDefault(),
            Meta_description = seoTexts.Where(x => x.DescriptionType == (int)SeoDescriptionTypes.Meta_description).Select(x => x.Description).FirstOrDefault()
          }
        });
      }
    }

    [ValidateInput(false)]
    [RequiresAuthentication(Functionalities.ManageSeoTexts)]
    public ActionResult UpdateSeoTexts(SeoText seoText)
    {
      using (var unit = GetUnitOfWork())
      {

        var repo = unit.Service<SeoTexts>();

        foreach (var field in seoText.GetType().GetProperties())
        {
          var value = field.GetValue(seoText, null);
          var name = field.Name;

          if (!Enum.IsDefined(typeof(SeoDescriptionTypes), name)) continue;

          var descriptionType = (int)Enum.Parse(typeof(SeoDescriptionTypes), name);

          var seoTextInDb = repo.Get(x => x.MasterGroupMappingID == seoText.MasterGroupMappingID && x.DescriptionType == descriptionType && x.LanguageID == seoText.LanguageID && x.ConnectorID == seoText.ConnectorID);
          if (seoTextInDb != null)
          {
            seoTextInDb.Description = value.ToString();
          }
          else if (value != null)
          {
            repo.Create(new SeoTexts
            {
              MasterGroupMappingID = seoText.MasterGroupMappingID,
              Description = value.ToString(),
              DescriptionType = descriptionType,
              LanguageID = seoText.LanguageID,
              ConnectorID = seoText.ConnectorID
            });
          }
        }

        unit.Save();
      }

      return Success("Successfully updated seo texts. ");
    }

    [RequiresAuthentication(Functionalities.ManageSeoTexts)]
    public ActionResult Create()
    {
      return Create<SeoTexts>();
    }

    #endregion

    #region Menu "Custom Labels"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetCustomLabels(int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        List<MasterGroupMappingCustomLabelModel> labelsToReturn = new System.Collections.Generic.List<MasterGroupMappingCustomLabelModel>();
        var allLabelsForMapping = unit.Service<MasterGroupMappingCustomLabel>().GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID);

        //first: add null connectors
        var languages = unit.Service<Language>().GetAll();

        foreach (var lan in languages)
        {
          var labelForNullConnector = allLabelsForMapping.FirstOrDefault(x => x.LanguageID == lan.LanguageID && x.ConnectorID == null);

          labelsToReturn.Add(new MasterGroupMappingCustomLabelModel()
          {
            LanguageID = lan.LanguageID,
            Language = lan.Name,
            CustomLabel = labelForNullConnector != null ? labelForNullConnector.CustomLabel : "",
            MasterGroupMappingID = labelForNullConnector != null ? MasterGroupMappingID : 0,
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

            labelsToReturn.Add(new MasterGroupMappingCustomLabelModel()
            {
              LanguageID = connectoLanguage.LanguageID,
              Language = connectoLanguage.Language.Name,
              CustomLabel = labelForUserConnector != null ? labelForUserConnector.CustomLabel : "",
              MasterGroupMappingID = labelForUserConnector != null ? MasterGroupMappingID : 0,
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
    public ActionResult SetCustomLabel(int _LanguageID, int _ConnectorID, int MasterGroupMappingID, string CustomLabel)
    {
      bool isNullConnector = _ConnectorID == -1;

      if (_LanguageID == -1)
      {
        return Success("Start updating custom labels");
      }
      if (string.IsNullOrEmpty(CustomLabel))
      {
        try
        {
          using (var unit = GetUnitOfWork())
          {
            if (isNullConnector)
              unit.Service<MasterGroupMappingCustomLabel>().Delete(c => c.MasterGroupMappingID == MasterGroupMappingID && c.LanguageID == _LanguageID
                && c.ConnectorID == null);
            else
              unit.Service<MasterGroupMappingCustomLabel>().Delete(c => c.MasterGroupMappingID == MasterGroupMappingID && c.LanguageID == _LanguageID
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

            var nameG = new MasterGroupMappingCustomLabel();

            if (isNullConnector)
              nameG = unit.Service<MasterGroupMappingCustomLabel>().Get(c => c.MasterGroupMappingID == MasterGroupMappingID && c.LanguageID == _LanguageID
                && c.ConnectorID == null);
            else
              nameG = unit.Service<MasterGroupMappingCustomLabel>().Get(c => c.MasterGroupMappingID == MasterGroupMappingID && c.LanguageID == _LanguageID
                && c.ConnectorID == _ConnectorID);

            if (nameG == null)
            {
              nameG = new MasterGroupMappingCustomLabel();
              nameG.MasterGroupMappingID = MasterGroupMappingID;
              nameG.LanguageID = _LanguageID;
              nameG.CustomLabel = CustomLabel;
              nameG.ConnectorID = isNullConnector ? (int?)null : _ConnectorID;
              unit.Service<MasterGroupMappingCustomLabel>().Create(nameG);
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

    #endregion

    #region Menu "Add Products To Connector Mapping"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult AddProductsToConnectorMapping(string SaveParamsJson)
    {
      var SaveParamsModel = new
      {
        MasterGroupMappingID = 0,
        ConnectorID = 0,
        ProductIDs = new[] { 0 }
      };
      var listOfParams = JsonConvert.DeserializeAnonymousType(SaveParamsJson, SaveParamsModel);

      List<ContentProductGroup> groups = new List<ContentProductGroup>();

      using (var unit = GetUnitOfWork())
      {
        var connectors = unit.Service<Connector>().GetAll(c => c.ConnectorID == listOfParams.ConnectorID
          || (c.ParentConnectorID.HasValue && c.ParentConnectorID.Value == listOfParams.ConnectorID))
          .Select(x => x.ConnectorID).ToList();

        MasterGroupMapping masterGroupMapping = unit
          .Service<MasterGroupMapping>()
          .Get(x => x.MasterGroupMappingID == listOfParams.MasterGroupMappingID);

        if (masterGroupMapping == null)
        {
          return Failure("Failed to add products to Product Group. Product Group ID does not exist!");
        }
        else
        {
          if (masterGroupMapping.ConnectorID == listOfParams.ConnectorID)
          {
            ConstructContentProductGroupHierarchy(groups, listOfParams.ProductIDs, listOfParams.MasterGroupMappingID, unit);

            var prodIds = groups.Select(x => x.ProductID).ToList();
            var distinctedprodids = prodIds.Distinct();

            foreach (var cpg in groups)
            {
              var masterGroupMappingProduct = unit.Service<MasterGroupMappingProduct>().Get(x => x.MasterGroupMappingID == listOfParams.MasterGroupMappingID && x.ProductID == cpg.ProductID);

              if (masterGroupMappingProduct == null)
              {
                masterGroupMappingProduct = new MasterGroupMappingProduct()
               {
                 MasterGroupMappingID = listOfParams.MasterGroupMappingID,
                 ProductID = cpg.ProductID,
                 IsCustom = true
               };

                unit.Service<MasterGroupMappingProduct>().Create(masterGroupMappingProduct);
              }

              foreach (var connectorID in connectors)
              {

                var currentContentProductGroup = unit
                  .Service<ContentProductGroup>()
                  .Get(x => x.ConnectorID == connectorID && x.MasterGroupMappingID == listOfParams.MasterGroupMappingID && x.ProductID == cpg.ProductID);

                //Check if also in content table because of foreign key constraint for this connector
                var productContent = unit.Service<Content>().Get(c => c.ProductID == cpg.ProductID && c.ConnectorID == connectorID);

                if (currentContentProductGroup == null && productContent != null)
                {
                  ContentProductGroup contentProductGroup = new ContentProductGroup()
                  {
                    ConnectorID = connectorID,
                    IsCustom = true,
                    MasterGroupMappingID = listOfParams.MasterGroupMappingID,
                    ProductID = cpg.ProductID,
                    ProductGroupMappingID = 933 //TODO: REPLACE AFTER MIGRATION
                  };
                  unit.Service<ContentProductGroup>().Create(contentProductGroup);
                }
                else
                {
                  if (currentContentProductGroup != null && !currentContentProductGroup.IsCustom)
                  {
                    currentContentProductGroup.IsCustom = true;
                  }
                }
              }
            }


            unit.Save();
            return Success("Successfully mapped products to product group");
          }
          else
          {
            return Failure("Failed to add products to Product Group. Data corrupt");
          }
        }
      }
    }

    private void ConstructContentProductGroupHierarchy(List<ContentProductGroup> contentProductGroupList, int[] productIDs, int masterGroupMappingID, IServiceUnitOfWork unit)
    {

      foreach (var productID in productIDs)
      {
        var product = unit.Service<Product>().Get(c => c.ProductID == productID);
        if (product == null) throw new Exception(string.Format("Product {0} not found", productID));

        //add selected product to the set
        contentProductGroupList.Add(new ContentProductGroup()
        {
          ProductID = product.ProductID,
          IsCustom = true,
          MasterGroupMappingID = masterGroupMappingID
        });

        //if product is toplevel product, add all the colorlevels including childs to the set
        if (product.ParentProductID == null)
        {
          var colorLevel = product.ChildProducts;

          contentProductGroupList.AddRange(from colorLevelProduct in colorLevel
                                           select new ContentProductGroup()
                                           {
                                             ProductID = colorLevelProduct.ProductID,
                                             IsCustom = true,
                                             MasterGroupMappingID = masterGroupMappingID
                                           });

          contentProductGroupList.AddRange(from colorLevelProduct in colorLevel
                                           from simpleProduct in colorLevelProduct.ChildProducts
                                           select new ContentProductGroup()
                                           {
                                             ProductID = simpleProduct.ProductID,
                                             IsCustom = true,
                                             MasterGroupMappingID = masterGroupMappingID
                                           });
        }
        else
        {
          //get all child related products and push them to the set
          var children = product.RelatedProductsSource.Where(c => c.IsConfigured).Select(c => c.RelatedProductID).Distinct().ToList();

          contentProductGroupList.AddRange(from rp in children
                                           select new ContentProductGroup()
                                           {
                                             ProductID = rp,
                                             IsCustom = true,
                                             MasterGroupMappingID = masterGroupMappingID
                                           });
        }
      }
    }

    #endregion

    #region "Import Product Group Mappings from connector"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult ImportProductGroupMappings(string SaveParamsJson)
    {
      var SaveParamsModel = new
      {
        connectorID = 0,
        importTo = "",
        newProductGroupName = ""
      };
      var listOfParams = JsonConvert.DeserializeAnonymousType(SaveParamsJson, SaveParamsModel);

      using (var unit = GetUnitOfWork())
      {
        if (listOfParams.connectorID > 0)
        {
          int parentID = 0;
          if (listOfParams.importTo == "new")
          {
            string productGroupName = listOfParams.newProductGroupName;
            if (String.IsNullOrWhiteSpace(listOfParams.newProductGroupName))
            {
              var connector = unit
                .Service<Connector>()
                .Get(x => x.ConnectorID == listOfParams.connectorID);
              if (connector == null)
              {
                return Failure("Faild to import the product group mapping. Connector is unkown.");
              }
              else
              {
                productGroupName = "Product Group Mappings From " + connector.Name;
              }
            }
            MasterGroupMapping newMasterGroupMapping = new MasterGroupMapping()
            {
              ProductGroupID = -1, // tijdelijk 
              ConnectorID = listOfParams.connectorID,
              FlattenHierarchy = false,
              FilterByParentGroup = false
            };
            unit.Service<MasterGroupMapping>().Create(newMasterGroupMapping);
            unit.Save();

            MasterGroupMappingLanguage newMasterGroupMappingLanguage = new MasterGroupMappingLanguage()
            {
              LanguageID = Client.User.LanguageID,
              MasterGroupMappingID = newMasterGroupMapping.MasterGroupMappingID,
              Name = productGroupName
            };
            unit.Service<MasterGroupMappingLanguage>().Create(newMasterGroupMappingLanguage);
            unit.Save();
            parentID = newMasterGroupMapping.MasterGroupMappingID;
          }
          ImportProductGroupMappings(parentID, 0, listOfParams.connectorID, unit);

          return Success("Successfully imported the product group mappings");
        }
        else
        {
          return Failure("Faild to import the product group mapping. Connector is unkown.");
        }
      }
    }
    #endregion

    #region Tabblad "Group Mapping"
    [RequiresAuthentication(Functionalities.GetContentProductGroupMapping)]
    public ActionResult GetListOfContentProductGroupByProductGroupMappingAndConnector(int ProductGroupMappingID, int ConnectorID)
    {
      return List(unit => (
            from contentProductGroup in unit
              .Service<ContentProductGroup>()
              .GetAll(x => x.ConnectorID == ConnectorID && x.ProductGroupMappingID == ProductGroupMappingID)

            join content in unit
              .Service<Content>()
              .GetAll(x => x.ConnectorID == ConnectorID) on contentProductGroup.ProductID equals content.ProductID

            let productName = contentProductGroup.Product != null ?
              contentProductGroup.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
              contentProductGroup.Product.ProductDescriptions.FirstOrDefault() : null

            select new
            {
              ProductName = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) : contentProductGroup.Product != null ? contentProductGroup.Product.VendorItemNumber : string.Empty,
              contentProductGroup.ProductID,
              ConnectorID,
              Connector = contentProductGroup.Connector.Name,
              ShortDescription = content.ShortDescription,
              LongDescription = content.LongDescription,
              VendorItemNumber = contentProductGroup.Product.VendorItemNumber
            }
          ));
    }

    #endregion

    #region Tabblad "Connector Publication Rule"
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfMasterGroupMappingName()
    {
      return List(unit => unit
        .Service<MasterGroupMapping>()
        .GetAll(x => x.ConnectorID == null)
        .ToList()
        .Select(x =>
        {
          int masterGroupMappingID = x.MasterGroupMappingID;
          var sourceMasterGroupMappingPadName = x
            .MasterGroupMappingLanguages
            .Where(c => c.LanguageID == Client.User.LanguageID)
            .Select(c => c.Name)
            .FirstOrDefault();

          bool hasParent = true;
          var MasterGroupMappingPadName = "";
          do
          {
            var masterGroupmapping = unit.Service<MasterGroupMapping>()
              .Get(m => m.MasterGroupMappingID == masterGroupMappingID);

            var tempMasterGroupMappingPadName = masterGroupmapping
              .MasterGroupMappingLanguages
              .Where(c => c.LanguageID == Client.User.LanguageID)
              .Select(c => c.Name)
              .FirstOrDefault();

            MasterGroupMappingPadName = (tempMasterGroupMappingPadName == null ? "Master Group Mapping Name Empty!" : tempMasterGroupMappingPadName) + MasterGroupMappingPadName;

            if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
            {
              masterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
              MasterGroupMappingPadName = "->" + MasterGroupMappingPadName;
            }
            else
            {
              hasParent = false;
            }
          } while (hasParent);

          return new
          {
            MasterGroupMappingID = x.MasterGroupMappingID,
            MasterGroupMappingName = sourceMasterGroupMappingPadName == null ? "Master Group Mapping Name Empty!" : sourceMasterGroupMappingPadName,
            MasterGroupMappingPad = MasterGroupMappingPadName
          };
        })
        .OrderBy(o => o.MasterGroupMappingPad)
        .AsQueryable()
      );
    }

    #endregion

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfSourceMasterGroupMappingProducts(int MasterGroupMappingID)
    {
      if (MasterGroupMappingID > 0)
      {
        using (var unit = GetUnitOfWork())
        {
          MasterGroupMapping masterGroupMapping = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
          if (masterGroupMapping != null)
          {
            return List(unitOfWord => (
                from p in unitOfWord.Service<MasterGroupMappingProduct>().GetAll(x => x.MasterGroupMappingID == masterGroupMapping.SourceMasterGroupMappingID)
                join c in unitOfWord.Service<Content>().GetAll(x => x.ConnectorID == masterGroupMapping.ConnectorID.Value) on p.ProductID equals c.ProductID
                select new
                {
                  p.ProductID,
                  ProductName = p.Product.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.ProductName).FirstOrDefault(),
                  p.Product.VendorItemNumber,
                  c.Product.IsConfigurable,
                  ShortDescription = p.Product.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.ShortContentDescription).FirstOrDefault(),
                  LongDescription = p.Product.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.LongContentDescription).FirstOrDefault()
                }
              )
            );
          }
          else
          {
            return Failure("Incorrect Master Group Mapping ID.");
          }
        }
      }
      else
      {
        return Failure("Incorrect Master Group Mapping ID.");
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfProducts(int ConnectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        return List(unitOfWord => (
          from p in unitOfWord.Service<Product>().GetAll()
          join c in unitOfWord.Service<Content>().GetAll(x => x.ConnectorID == ConnectorID) on p.ProductID equals c.ProductID
          select new
          {
            p.ProductID,
            ProductName = p.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.ProductName).FirstOrDefault(),
            p.VendorItemNumber,
            p.IsConfigurable,
            ShortDescription = p.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.ShortContentDescription).FirstOrDefault(),
            LongDescription = p.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).Select(l => l.LongContentDescription).FirstOrDefault()
          }
        )
        );
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetListOfcustomProducts(int MasterGroupMappingID)
    {
      if (MasterGroupMappingID > 0)
      {
        var customProductsQuery = string.Format(@"
                  SELECT mp.ProductID
                  	,cpg.IsExported
                  	,pd.ProductName
                  	,pd.ShortContentDescription
                  	,pd.LongContentDescription
                  	,p.VendorItemNumber
                    ,cpg.ContentProductGroupID
                    ,mp.MasterGroupMappingID
                    ,p.IsConfigurable
                  FROM MasterGRoupMappingProduct mp
                  INNER JOIN ContentProductGroup cpg ON cpg.ProductID = mp.ProductID
                  	AND cpg.MasterGroupMappingID = mp.MasterGroupMappingID
                  INNER JOIN Product p ON p.ProductID = mp.ProductID
                  LEFT JOIN ProductDescription pd ON pd.ProductID = mp.ProductID
                  	AND pd.LanguageID = {0}
                  WHERE mp.MasterGroupMappingID = {1} AND mp.IsCustom = 1",
          Client.User.LanguageID, MasterGroupMappingID);

        return List(unitOfWord => unitOfWord
          .ExecuteStoreQuery<ConnectorMappingProducts>(customProductsQuery).GroupBy(x => x.ProductID)
          .Select(x => new
          {
            ProductID = x.Key,
            IsExported = x.Select(y => y.IsExported).FirstOrDefault(),
            ProductName = x.Select(y => y.ProductName).FirstOrDefault(d => !string.IsNullOrEmpty(d)),
            VendorItemNumber = x.Select(y => y.VendorItemNumber).FirstOrDefault(d => !string.IsNullOrEmpty(d)),
            ShortDescription = x.Select(y => y.ShortContentDescription).FirstOrDefault(d => !string.IsNullOrEmpty(d)),
            LongDescription = x.Select(y => y.LongContentDescription).FirstOrDefault(d => !string.IsNullOrEmpty(d)),
            IsConfigurable = x.Select(y => y.IsConfigurable).FirstOrDefault()
          }).ToList().AsQueryable());
      }
      else
      {
        return Failure("Incorrect Master Group Mapping ID.");
      }
    }

    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult RemoveCustomProduct(int MasterGroupMappingID, int ProductID)
    {
      using (var unit = GetUnitOfWork())
      {
        if (MasterGroupMappingID > 0 && ProductID > 0)
        {
          MasterGroupMapping masterGroupMapping = unit
            .Service<MasterGroupMapping>()
            .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

          if (masterGroupMapping != null)
          {
            var masterGroupMappingProduct = unit.Service<MasterGroupMappingProduct>().Get(x => x.ProductID == ProductID && x.MasterGroupMappingID == MasterGroupMappingID);
            var contentProductGroup = unit.Service<ContentProductGroup>().Get(x => x.ProductID == ProductID && x.MasterGroupMappingID == MasterGroupMappingID);

            if (masterGroupMappingProduct != null)
            {
              unit.Service<MasterGroupMappingProduct>().Delete(masterGroupMappingProduct);
            }

            if (contentProductGroup != null)
            {
              unit.Service<ContentProductGroup>().Delete(contentProductGroup);
            }

            unit.Save();
            return Success("Successfully removed custom product from product group");
          }
          else
          {
            return Failure("Failed to remove custom product from Product Group. Master Group Mapping does not exist!");
          }
        }
        else
        {
          return Failure("Failed to remove custom product from Product Group. Data corrupt");
        }
      }
    }


    #endregion

    #region private functions
    private IEnumerable<int> StringToIntList(string str)
    {
      if (String.IsNullOrEmpty(str))
        yield break;

      foreach (var s in str.Split(','))
      {
        int num;
        if (int.TryParse(s, out num))
          yield return num;
      }
    }
    private List<TreeNodeModel> FindChildrenByMasterGroupMappingID(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      List<TreeNodeModel> listOfMasterGroupMappings = new List<TreeNodeModel>();
      var masterGroupmapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

      if (masterGroupmapping != null && masterGroupmapping.MasterGroupMappingChildren.Count > 0)
      {
        masterGroupmapping.MasterGroupMappingChildren.ToList().ForEach(x =>
        {
          List<TreeNodeModel> tempListOfMasterGroupMappings = FindChildrenByMasterGroupMappingID(x.MasterGroupMappingID, unit);
          listOfMasterGroupMappings.AddRange(tempListOfMasterGroupMappings);
        });
      }

      var masterGroupMappingName = masterGroupmapping
        .MasterGroupMappingLanguages
        .Where(x => x.LanguageID == Client.User.LanguageID)
        .Select(x => x.Name)
        .FirstOrDefault();

      if (masterGroupMappingName == null)
      {
        masterGroupMappingName = "MasterGroupMapping NAME EMPTY!";
      }

      listOfMasterGroupMappings.ForEach(x =>
        x.MasterGroupMappingPad = masterGroupMappingName + "->" + x.MasterGroupMappingPad
      );

      listOfMasterGroupMappings.Add(new TreeNodeModel { MasterGroupMappingID = masterGroupmapping.MasterGroupMappingID, MasterGroupMappingPad = masterGroupMappingName });

      return listOfMasterGroupMappings;
    }
    private ArrayList GetMasterGroupMappingRelatedObjects(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      ArrayList listOfRelatedObjects = new ArrayList();

      listOfRelatedObjects.AddRange(GetRelatedCrossReferences(MasterGroupMappingID, unit));
      listOfRelatedObjects.AddRange(GetRelatedSelectors(MasterGroupMappingID, unit));
      listOfRelatedObjects.AddRange(GetRelatedConnectorProductGroupMapping(MasterGroupMappingID, unit));
      listOfRelatedObjects.AddRange(GetRelatedChildProductGroupMapping(MasterGroupMappingID));

      return listOfRelatedObjects;
    }
    private Boolean IsMasterGroupMappingRelated(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      if (HasMasterGroupMappingMatchWithProductGroupMapping(MasterGroupMappingID, unit))
      {
        return true;
      }

      if (IsMasterGroupMappingCrossed(MasterGroupMappingID, unit))
      {
        return true;
      }
      return false;
    }
    private Boolean HasMasterGroupMappingMatchWithProductGroupMapping(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      bool childHasRelation = false;
      var masterGroupMapping = unit
        .Service<MasterGroupMapping>()
        .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
      if (masterGroupMapping.ProductGroupMappings.Count > 0)
      {
        return true;
      }
      masterGroupMapping.MasterGroupMappingChildren.ForEach(x =>
      {
        if (HasMasterGroupMappingMatchWithProductGroupMapping(x.MasterGroupMappingID, unit))
        {
          childHasRelation = true;
        }
      });
      if (childHasRelation)
        return true;
      return false;
    }

    private Boolean IsMasterGroupMappingCrossed(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      bool childHasRelation = false;
      var masterGroupMapping = unit
        .Service<MasterGroupMapping>()
        .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
      if (masterGroupMapping.MasterGroupMappings.Count > 0)
      {
        return true;
      }
      masterGroupMapping.MasterGroupMappingChildren.ForEach(x =>
      {
        if (IsMasterGroupMappingCrossed(x.MasterGroupMappingID, unit))
        {
          childHasRelation = true;
        }
      });
      if (childHasRelation)
        return true;
      return false;
    }

    /// <summary>
    /// To check products in the MasterGroupMapping wizard, all control rules are set in the ApplicationController.InitializeProductControls
    /// </summary>
    /// <param name="ProductID"></param>
    /// <param name="MasterGroupMappingID"></param>
    /// <returns>Boolean if check succeed</returns>
    private Boolean ProductControl(int ProductID, int MasterGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        var productControles = unit
          .Service<ProductControl>()
          .GetAll(x => x.IsActive == true);
        var masterGroupMapping = unit.Service<MasterGroupMapping>()
          .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

        var currentProductStep = GetAttributeCheckStep(ProductID, unit);

        Boolean ProductImageApproved = true;
        Boolean ProductAttributeApproved = true;
        Boolean ProductMatch = true;
        Boolean ProductPrimaryCheck = true;

        // Product Image Controle
        if (productControles.Where(x => x.ProductControlID == 1).Select(x => x.IsActive).FirstOrDefault())
        {
          ProductImageApproved = ProductControlProductImage(unit, ProductID);
        }

        // Product Attribute Controle
        if (productControles.Where(x => x.ProductControlID == 2).Select(x => x.IsActive).FirstOrDefault())
        {
          ProductAttributeApproved = ProductControlProductAttributes(masterGroupMapping, unit, ProductID);
        }

        // Product Match Controle
        if (productControles.Where(x => x.ProductControlID == 3).Select(x => x.IsActive).FirstOrDefault())
        {
          ProductMatch = ProductControlProductMatch(masterGroupMapping, unit, ProductID);
        }

        // Product Set Primary Controle
        if (productControles.Where(x => x.ProductControlID == 4).Select(x => x.IsActive).FirstOrDefault())
        {
          if (currentProductStep.Value != "Step 1 - Product")
            ProductPrimaryCheck = ProductControlPrimary(masterGroupMapping, unit, ProductID);
        }

        if (ProductImageApproved && ProductAttributeApproved & ProductMatch && ProductPrimaryCheck)
          return true;
        return false;
      }
    }

    private Boolean ProductControlProductImage(IServiceUnitOfWork unit, int productID)
    {
      return unit.Scope.Repository<ProductMedia>().GetAll(pm => pm.TypeID == 1 && pm.ProductID == productID).Count() > 0 ? true : false;
    }

    private Boolean ProductControlProductAttributes(MasterGroupMapping masterGroupMapping, IServiceUnitOfWork unit, int productID)
    {
      var masterGroupMappingAttributes = masterGroupMapping
        .ProductAttributeMetaDatas
        .Select(x => x.AttributeID);

      foreach (int AttributeID in masterGroupMappingAttributes)
      {
        var ProductAttribute = unit.Scope.Repository<ProductAttributeValue>().GetAll(x => x.AttributeID == AttributeID && x.ProductID == productID).Count() > 0 ? true : false;
        if (!ProductAttribute)
        {
          return false;
        }
      }
      return true;
    }

    private Boolean ProductControlProductMatch(MasterGroupMapping masterGroupMapping, IServiceUnitOfWork unit, int productID)
    {
      var newMatchstatus = (int)MatchStatuses.New;
      var productMatch = unit.Scope.Repository<ProductMatch>().GetSingle(x => x.ProductID == productID);
      if (productMatch != null)
      {
        var matchID = productMatch.ProductMatchID;

        if (unit.Scope.Repository<ProductMatch>().GetAll(x => x.ProductMatchID == matchID).Any(x => x.MatchStatus == newMatchstatus))
          return false;
        else
          return true;
      }

      return true;
    }

    private Boolean ProductControlPrimary(MasterGroupMapping masterGroupMapping, IServiceUnitOfWork unit, int productID)
    {
      var productMatch = unit.Scope.Repository<ProductMatch>().GetSingle(x => x.ProductID == productID);
      if (productMatch != null)
      {
        var matchID = productMatch.ProductMatchID;

        if (unit.Scope.Repository<ProductMatch>().GetAll(x => x.ProductMatchID == matchID).Any(x => x.Primary))
          return true;
        else
          return false;
      }

      return true;
    }

    private int countProductsInMasterGroupMapping(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
      int count = 0;
      if (masterGroupMapping.MasterGroupMappingChildren.Count > 0)
      {
        masterGroupMapping.MasterGroupMappingChildren.ToList().ForEach(x =>
        {
          count += countProductsInMasterGroupMapping(x.MasterGroupMappingID, unit);
        });
      }
      if (masterGroupMapping.MasterGroupMappingProducts == null)
      {
        return 0;
      }
      count += masterGroupMapping.MasterGroupMappingProducts.Count();
      return count;
    }
    private int countNotControledProductsInMasterGroupMapping(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      var masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);
      int count = 0;
      if (masterGroupMapping.MasterGroupMappingChildren.Count > 0)
      {
        masterGroupMapping.MasterGroupMappingChildren.ToList().ForEach(x =>
        {
          count += countNotControledProductsInMasterGroupMapping(x.MasterGroupMappingID, unit);
        });
      }
      if (masterGroupMapping.MasterGroupMappingProducts == null)
      {
        return 0;
      }
      count += masterGroupMapping.MasterGroupMappingProducts.Where(x => x.IsApproved == false).Count();
      return count;
    }
    private void CopyNodes(int MasterGroupMappingID, int ParentMasterGroupMappingID, bool CopyProducts, bool CopyAttributes, bool copyCrossReferences, IServiceUnitOfWork unit)
    {
      MasterGroupMapping masterGroupMapping = unit.Service<MasterGroupMapping>().Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

      MasterGroupMapping newMasterGroupMapping = new MasterGroupMapping()
      {
        ProductGroupID = masterGroupMapping.ProductGroupID,
        Score = masterGroupMapping.Score,
        ConnectorID = masterGroupMapping.ConnectorID,
        FlattenHierarchy = masterGroupMapping.FlattenHierarchy,
        FilterByParentGroup = masterGroupMapping.FilterByParentGroup,
        SourceMasterGroupMappingID = masterGroupMapping.SourceMasterGroupMappingID
      };

      if (ParentMasterGroupMappingID != -1)
      {
        newMasterGroupMapping.ParentMasterGroupMappingID = ParentMasterGroupMappingID;
      }

      masterGroupMapping.MasterGroupMappingLanguages.ForEach(x =>
      {
        MasterGroupMappingLanguage masterGroupMappingLanguage = new MasterGroupMappingLanguage()
        {
          MasterGroupMappingID = newMasterGroupMapping.MasterGroupMappingID,
          LanguageID = x.LanguageID,
          Name = x.Name
        };
        unit.Service<MasterGroupMappingLanguage>().Create(masterGroupMappingLanguage);
      });

      if (CopyProducts)
      {
        newMasterGroupMapping.ProductGroupVendors = new List<ProductGroupVendor>();
        newMasterGroupMapping.MasterGroupMappingProducts = new List<MasterGroupMappingProduct>();
        masterGroupMapping.ProductGroupVendors.ToList().ForEach(x =>
        {
          newMasterGroupMapping.ProductGroupVendors.Add(x);
        });

        masterGroupMapping.MasterGroupMappingProducts.ToList().ForEach(x =>
        {
          MasterGroupMappingProduct masterGroupMappingProduct = new MasterGroupMappingProduct()
          {
            ProductID = x.ProductID,
            IsApproved = false
          };
          newMasterGroupMapping.MasterGroupMappingProducts.Add(masterGroupMappingProduct);
        });
      }

      if (CopyAttributes)
      {
        newMasterGroupMapping.ProductAttributeMetaDatas = new List<ProductAttributeMetaData>();
        masterGroupMapping.ProductAttributeMetaDatas.ToList().ForEach(x =>
        {
          newMasterGroupMapping.ProductAttributeMetaDatas.Add(x);
        });
      }

      if (copyCrossReferences)
      {
        newMasterGroupMapping.MasterGroupMappingCrossReferences = new List<MasterGroupMapping>();
        masterGroupMapping.MasterGroupMappingCrossReferences.ToList().ForEach(x =>
        {
          newMasterGroupMapping.MasterGroupMappingCrossReferences.Add(x);
        });
      }

      unit.Service<MasterGroupMapping>().Create(newMasterGroupMapping);

      unit.Save();

      var child = unit.Service<MasterGroupMapping>().GetAll(x => x.ParentMasterGroupMappingID == MasterGroupMappingID).ToList();

      if (child.Count > 0)
      {
        child.ForEach(x =>
        {
          CopyNodes(x.MasterGroupMappingID, newMasterGroupMapping.MasterGroupMappingID, CopyProducts, CopyAttributes, copyCrossReferences, unit);
        });
      }
    }
    private void CopyMasterGroupMappingToConnectorMapping(int MasterGroupMappingID, int ParentConnectorMappingID, int ConnectorID, bool copyChildren, IServiceUnitOfWork unit)
    {
      // Get master group mapping 
      MasterGroupMapping masterGroupMapping = unit
        .Service<MasterGroupMapping>()
        .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

      if (masterGroupMapping != null)
      {
        // TODO: dit is nodig zolang er een relatie is tussen ProductGroup en MasterGroupMapping. Dit moet uiteindelijk weg.
        var productGroup = unit
          .Service<ProductGroup>()
          .GetAll()
          .First();
        //

        MasterGroupMapping connectorMapping = new MasterGroupMapping()
        {
          ProductGroupID = productGroup.ProductGroupID,
          Score = masterGroupMapping.Score,
          ConnectorID = ConnectorID,
          SourceMasterGroupMappingID = MasterGroupMappingID
        };

        if (ParentConnectorMappingID > 0)
        {
          connectorMapping.ParentMasterGroupMappingID = ParentConnectorMappingID;
        }
        unit.Service<MasterGroupMapping>().Create(connectorMapping);
        unit.Save();

        masterGroupMapping.MasterGroupMappingLanguages.ForEach(x =>
        {
          MasterGroupMappingLanguage masterGroupMappingLanguage = new MasterGroupMappingLanguage()
          {
            MasterGroupMappingID = connectorMapping.MasterGroupMappingID,
            LanguageID = x.LanguageID,
            Name = x.Name
          };
          unit.Service<MasterGroupMappingLanguage>().Create(masterGroupMappingLanguage);
        });
        unit.Save();

        if (copyChildren)
        {
          var child = unit.Service<MasterGroupMapping>().GetAll(x => x.ParentMasterGroupMappingID == MasterGroupMappingID).ToList();
          if (child.Count > 0)
          {
            child.ForEach(x =>
            {
              CopyMasterGroupMappingToConnectorMapping(x.MasterGroupMappingID, connectorMapping.MasterGroupMappingID, ConnectorID, copyChildren, unit);
            });
          }
        }
      }
    }
    private void DeleteHierarchy(int masterGroupMappingID, IServiceUnitOfWork unit)
    {
      var child = unit.Service<MasterGroupMapping>().GetAll(x => x.ParentMasterGroupMappingID.HasValue && x.ParentMasterGroupMappingID.Value == masterGroupMappingID).ToList();

      if (child.Count > 0)
      {
        child.ForEach(x =>
        {
          DeleteHierarchy(x.MasterGroupMappingID, unit);
        });
      }
      //UnassignedAllUsersFromMasterGroupMapping(masterGroupMappingID, unit);
      //UnMatchAllProductGroupVendorsFromMasterGroupMapping(masterGroupMappingID, unit);
      //UnMatchAllVendorProductsFromMasterGroupMapping(masterGroupMappingID, unit);
      //DeleteAllAttributesOfMasterGroupMapping(masterGroupMappingID, unit);
      //DeleteAllCrossReferencesOfMasterGroupMapping(masterGroupMappingID, unit);
      //DeleteMasterGroupMappingLanguage(masterGroupMappingID, unit);
      (unit.Service<MasterGroupMapping>()).Delete(c => c.MasterGroupMappingID == masterGroupMappingID);
    }
    private void DeleteProductGroupMappingHierarchy(int productGroupMappingID, IServiceUnitOfWork unit)
    {
      var child = unit
        .Service<ProductGroupMapping>()
        .GetAll(x => x.ParentProductGroupMappingID.HasValue && x.ParentProductGroupMappingID.Value == productGroupMappingID)
        .ToList();

      if (child.Count > 0)
      {
        child.ForEach(x =>
        {
          DeleteProductGroupMappingHierarchy(x.ProductGroupMappingID, unit);
        });
      }
      (unit.Service<ProductGroupMapping>()).Delete(c => c.ProductGroupMappingID == productGroupMappingID);
    }

    private void DeleteRelatedAttributes(int MasterGroupMappingID, int CrossReferenceID, IServiceUnitOfWork unit)
    {
      var relatedAttributes = unit
        .Service<MasterGroupMappingRelatedAttribute>()
        .GetAll(x => x.MasterGroupMappingID == CrossReferenceID && x.ParentRelatedAttribute.MasterGroupMappingID == MasterGroupMappingID);

      relatedAttributes.ForEach(x =>
      {
        unit.Service<MasterGroupMappingRelatedAttribute>().Delete(x);
      });

      unit.Save();

      relatedAttributes = unit
        .Service<MasterGroupMappingRelatedAttribute>()
        .GetAll(x => x.MasterGroupMappingID == MasterGroupMappingID && x.CrossReferenceRelatedAttribute.Count == 0);

      relatedAttributes.ForEach(x =>
      {
        unit.Service<MasterGroupMappingRelatedAttribute>().Delete(x);
      });

    }

    private void AssignUserTo(int masterGroupMappingID, int userID, IServiceUnitOfWork unit)
    {
      var childMasterGroupMappingHierarchy = GetListOfMasterGroupMappingHierarchy(masterGroupMappingID);

      var masterGroupMappingIDs = childMasterGroupMappingHierarchy.Select(x => x.MasterGroupMappingID).ToList();

      AssignUserToMasterGroupMapping(masterGroupMappingIDs, userID, unit);

      VerifyMasterGroupMappingAssigneeRole(unit);
    }

    private void VerifyMasterGroupMappingAssigneeRole(IServiceUnitOfWork unit)
    {
      var createRoleIfNotExistsQuery = string.Format(@"
        IF NOT EXISTS (
        		SELECT RoleID
        		FROM ROLE
        		WHERE rolename = '{0}'
        		)
        	INSERT INTO [Role] (
        		RoleName
        		,IsHidden
        		)
        	VALUES (
        		'{0}'
        		,0
        		)
      ", MasterGroupMappingConstants.MasterGroupMappingAssigneeRoleName);

      unit.ExecuteStoreCommand(createRoleIfNotExistsQuery);

      var functionalityNames = new[]
          {
            Functionalities.MasterGroupMappingViewVendorProducts,
            Functionalities.MasterGroupMappingViewVendorProductGroups
          }.Select(x => "('" + x.ToString() + "')").ToArray();

      var setFunctionalitiesQuery = string.Format(@"
        DECLARE @RoleID INT = (
		         SELECT RoleID
		         FROM [Role]
		         WHERE rolename = '{1}'
		    )
      
      	MERGE FunctionalityRole AS Target
      	USING (
      		SELECT FunctionalityName
      		FROM (
      			VALUES {0}
      			) V(FunctionalityName)
      		) AS Source
      		ON Target.FunctionalityName = Source.FunctionalityName AND Target.RoleID = @RoleID
      	WHEN NOT MATCHED
      		THEN
      			INSERT (
      				RoleID
      				,FunctionalityName
      				)
      			VALUES (
      				@RoleID
      				,Source.FunctionalityName
      				);
        ", string.Join(",", functionalityNames), MasterGroupMappingConstants.MasterGroupMappingAssigneeRoleName);

      unit.ExecuteStoreCommand(setFunctionalitiesQuery);
    }

    private void AssignUserToMasterGroupMapping(List<int> masterGroupMappingIDs, int userID, IServiceUnitOfWork unit)
    {
      var assignUserToMasterGroupMappingQuery = string.Format(@"
            MERGE INTO MasterGroupMappingUser AS Target
            USING (
            	SELECT MasterGroupMappingID
            		,{0} AS UserID
            	FROM MasterGroupMapping
            	WHERE MasterGroupMappingID IN ({1})
            	) AS Source
            	ON Target.MasterGroupMappingID = Source.MasterGroupMappingID AND Target.UserID = Source.UserID
            WHEN NOT MATCHED
            	THEN
            		INSERT (
            			MasterGroupMappingID
            			,UserID
            			)
            		VALUES (
            			Source.MasterGroupMappingID
            			,Source.UserID
            			);"
      , userID, string.Join<int>(",", masterGroupMappingIDs));

      unit.ExecuteStoreCommand(assignUserToMasterGroupMappingQuery);
    }

    private void ImportProductGroupMappings(int toProductGroupID, int fromParentProductGroupMappingID, int connectorID, IServiceUnitOfWork unit)
    {
      List<ProductGroupMapping> listOfProductGroupMappings = new List<ProductGroupMapping>();

      if (fromParentProductGroupMappingID > 0)
      {
        listOfProductGroupMappings = unit
          .Service<ProductGroupMapping>()
          .GetAll(x => x.ParentProductGroupMappingID == fromParentProductGroupMappingID && x.ConnectorID == connectorID)
          .ToList();
      }
      else
      {
        listOfProductGroupMappings = unit
          .Service<ProductGroupMapping>()
          .GetAll(x => x.ParentProductGroupMappingID == null && x.ConnectorID == connectorID)
          .Where(x => x.CustomProductGroupLabel == null)
          .ToList();
      }

      listOfProductGroupMappings.ForEach(productGroupMapping =>
      {
        MasterGroupMapping newMasterGroupMapping = new MasterGroupMapping()
        {
          ProductGroupID = productGroupMapping.ProductGroupID,
          ConnectorID = connectorID,
          FlattenHierarchy = productGroupMapping.FlattenHierarchy,
          FilterByParentGroup = productGroupMapping.FilterByParentGroup,
          Score = productGroupMapping.Score
        };
        if (toProductGroupID > 0)
        {
          newMasterGroupMapping.ParentMasterGroupMappingID = toProductGroupID;
        }
        unit.Service<MasterGroupMapping>().Create(newMasterGroupMapping);
        unit.Save();

        if (productGroupMapping.CustomProductGroupLabel == null)
        {
          var languages = productGroupMapping
            .ProductGroup
            .ProductGroupLanguages
            .ToList();
          languages.ForEach(language =>
          {
            MasterGroupMappingLanguage newMasterGroupMappingLanguage = new MasterGroupMappingLanguage()
            {
              LanguageID = language.LanguageID,
              MasterGroupMappingID = newMasterGroupMapping.MasterGroupMappingID,
              Name = language.Name
            };
            unit.Service<MasterGroupMappingLanguage>().Create(newMasterGroupMappingLanguage);
          });
        }
        else
        {
          var languages = unit
            .Service<Language>()
            .GetAll();

          languages.ForEach(language =>
          {
            MasterGroupMappingLanguage newMasterGroupMappingLanguage = new MasterGroupMappingLanguage()
            {
              LanguageID = language.LanguageID,
              MasterGroupMappingID = newMasterGroupMapping.MasterGroupMappingID,
              Name = productGroupMapping.CustomProductGroupLabel
            };
            unit.Service<MasterGroupMappingLanguage>().Create(newMasterGroupMappingLanguage);
          });
        }
        unit.Save();

        //productGroupMapping.ChildMappings.ForEach(childProductGroup => {
        //  ImportProductGroupMappings(newMasterGroupMapping.MasterGroupMappingID, productGroupMapping.ProductGroupMappingID, connectorID, unit);
        //});
      });
    }
    private string GetMasterGroupMappingPath(int MasterGroupMappingID, int LanguageID, IServiceUnitOfWork unit)
    {
      bool hasParent = true;
      var MasterGroupMappingPathName = "";
      do
      {
        var masterGroupmapping = unit.Service<MasterGroupMapping>()
          .Get(m => m.MasterGroupMappingID == MasterGroupMappingID);

        var tempMasterGroupMappingPadName = "MasterGroupMapping NAME EMPTY!";

        var tempMasterGroupMapping = masterGroupmapping
          .MasterGroupMappingLanguages
          .FirstOrDefault(c => c.LanguageID == LanguageID);

        if (tempMasterGroupMapping != null) tempMasterGroupMappingPadName = tempMasterGroupMapping.Name;

        MasterGroupMappingPathName = tempMasterGroupMappingPadName + MasterGroupMappingPathName;

        if (masterGroupmapping.ParentMasterGroupMappingID.HasValue)
        {
          MasterGroupMappingID = masterGroupmapping.MasterGroupMappingParent.MasterGroupMappingID;
          MasterGroupMappingPathName = "->" + MasterGroupMappingPathName;
        }
        else
        {
          hasParent = false;
        }
      } while (hasParent);
      return MasterGroupMappingPathName;
    }

    private ArrayList GetAllCrossMasterGroupMappings(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      ArrayList listOfCrossReference = new ArrayList();

      var masterGroupMapping = unit
        .Service<MasterGroupMapping>()
        .Get(x => x.MasterGroupMappingID == MasterGroupMappingID);

      var masterGroupMappingName = masterGroupMapping.MasterGroupMappingLanguages.Where(x => x.LanguageID == Client.User.LanguageID).Select(x => x.Name).FirstOrDefault();
      var masterGroupMappingPath = GetMasterGroupMappingPath(MasterGroupMappingID, Client.User.LanguageID, unit);

      masterGroupMapping.MasterGroupMappings.ForEach(x =>
      {
        var crossReferenceName = masterGroupMapping.MasterGroupMappingLanguages.Where(l => l.LanguageID == Client.User.LanguageID).Select(n => n.Name).FirstOrDefault();
        var item = new
        {
          MasterGroupMappingName = masterGroupMappingName,
          MasterGroupMappingInfo = new
          {
            MasterGroupMappingUsedBy = "Cross Reference (" + crossReferenceName + ")",
            MasterGroupMappingpath = masterGroupMappingPath,
            MasterGroupMappingUsedPath = GetMasterGroupMappingPath(x.MasterGroupMappingID, Client.User.LanguageID, unit)
          }
        };

        listOfCrossReference.Add(item);
      });

      masterGroupMapping.MasterGroupMappingChildren.ForEach(x =>
      {
        listOfCrossReference.AddRange(GetAllCrossMasterGroupMappings(x.MasterGroupMappingID, unit));
      });
      return listOfCrossReference;
    }
    private ArrayList GetCrossReferences(List<MasterGroupMappingModel> listOfMasterGroupMappings, List<MasterGroupMappingModel> listOfSelectedMasterGroupMappings, IServiceUnitOfWork unit)
    {
      ArrayList listOfRelatedObjects = new ArrayList();

      List<int> listOfIDs = listOfSelectedMasterGroupMappings.Select(x => x.MasterGroupMappingID).ToList();
      List<MasterGroupMapping> listOfCrossReferences = unit
        .Service<MasterGroupMapping>()
        .GetAll(x => x.MasterGroupMappings.Any(y => listOfIDs.Contains(y.MasterGroupMappingID)))
        .ToList();

      //var masterGroupMappingPath = GetMasterGroupMappingPath(MasterGroupMappingID, Client.User.LanguageID, unit);
      listOfCrossReferences.ForEach(x =>
      {
        //var crossReferenceName = x.MasterGroupMappingLanguages.Where(l => l.LanguageID == Client.User.LanguageID).Select(n => n.Name).FirstOrDefault();
        var item = new
        {
          MasterGroupMappingName = listOfMasterGroupMappings.Where(m => m.MasterGroupMappingID == x.MasterGroupMappingID).Select(n => n.MasterGroupMappingName),
          MasterGroupMappingInfo = new
          {
            MasterGroupMappingUsedBy = "Cross Reference To (" + listOfMasterGroupMappings.Where(m => m.MasterGroupMappingID == x.MasterGroupMappingID).Select(n => n.MasterGroupMappingName) + ")",
            //MasterGroupMappingpath = masterGroupMappingPath,
            MasterGroupMappingUsedPath = GetMasterGroupMappingPath(x.MasterGroupMappingID, Client.User.LanguageID, unit)
          }
        };

        listOfRelatedObjects.Add(item);
      });
      return listOfRelatedObjects;
    }

    private ArrayList GetRelatedCrossReferences(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      ArrayList listOfRelatedObjects = new ArrayList();
      var item = new
      {
        ErrorMessage = "Related Cross References",
        Info = new List<string>()
      };

      string sqlQuery = string.Format(@"
        WITH masterGroupMappingChildren
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.MasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN masterGroupMappingChildren mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        SELECT m.*, mcr.CrossReferenceID as CrossReferenceID, ISNULL(ml.NAME, 'MasterGroupMapping Name Empty') as MasterGroupMappingName
        FROM MasterGroupMapping m
        INNER JOIN masterGroupMappingChildren mc ON m.MasterGroupMappingID = mc.MasterGroupMappingID
        LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
	        AND ml.LanguageID = {1}
        INNER JOIN MasterGroupMappingCrossReference mcr ON m.MasterGroupMappingID = mcr.MasterGroupMappingID  
      ", MasterGroupMappingID, Client.User.LanguageID);

      List<MasterGroupMappingCrossReferenceModel> listOfCrossReferences = unit
        .ExecuteStoreQuery<MasterGroupMappingCrossReferenceModel>(sqlQuery)
        .ToList();

      sqlQuery = string.Format(@"
        WITH masterGroupMappingChildren
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.MasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN masterGroupMappingChildren mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        SELECT m.*, mcr.MasterGroupMappingID as CrossReferenceID, ISNULL(ml.NAME, 'MasterGroupMapping Name Empty') as MasterGroupMappingName
        FROM MasterGroupMapping m
        INNER JOIN masterGroupMappingChildren mc ON m.MasterGroupMappingID = mc.MasterGroupMappingID
        LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
	        AND ml.LanguageID = {1}
        INNER JOIN MasterGroupMappingCrossReference mcr ON m.MasterGroupMappingID = mcr.CrossReferenceID  
      ", MasterGroupMappingID, Client.User.LanguageID);

      List<MasterGroupMappingCrossReferenceModel> listOfRelatedCrossReferences = unit
        .ExecuteStoreQuery<MasterGroupMappingCrossReferenceModel>(sqlQuery)
        .ToList();

      List<MasterGroupMappingModel> listOfMasterGroupMappingPaths = GetListOfMasterGroupMappingPaths();

      listOfCrossReferences.ForEach(masterGroupMapping =>
      {
        MasterGroupMappingModel crossReference = listOfMasterGroupMappingPaths.Where(x => x.MasterGroupMappingID == masterGroupMapping.CrossReferenceID).FirstOrDefault();
        item.Info.Add(string.Format("'{0}' is crossed to '{1}' (Path: {2})", masterGroupMapping.MasterGroupMappingName, crossReference.MasterGroupMappingName, crossReference.MasterGroupMappingPath));
      });

      listOfRelatedCrossReferences.ForEach(masterGroupMapping =>
      {
        MasterGroupMappingModel crossReference = listOfMasterGroupMappingPaths.Where(x => x.MasterGroupMappingID == masterGroupMapping.CrossReferenceID).FirstOrDefault();
        item.Info.Add(string.Format("'{1}' is crossed to '{0}' (Path: {2})", masterGroupMapping.MasterGroupMappingName, crossReference.MasterGroupMappingName, crossReference.MasterGroupMappingPath));
      });

      if (item.Info.Count > 0)
      {
        listOfRelatedObjects.Add(item);
      }
      return listOfRelatedObjects;
    }
    private ArrayList GetRelatedSelectors(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      ArrayList listOfRelatedObjects = new ArrayList();
      var item = new
      {
        ErrorMessage = "Related Selectors",
        Info = new List<string>()
      };

      string sqlQuery = string.Format(@"
        WITH masterGroupMappingChildren
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.MasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN masterGroupMappingChildren mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        SELECT m.*
	        ,ISNULL(ml.NAME, 'MasterGroupMapping Name Empty') AS MasterGroupMappingName
	        ,s.NAME as SelectorName
        FROM MasterGroupMapping m
        INNER JOIN masterGroupMappingChildren mc ON m.MasterGroupMappingID = mc.MasterGroupMappingID
        LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
	        AND ml.LanguageID = {1}
        INNER JOIN Selector s ON m.MasterGroupMappingID = s.MasterGroupMappingID
      ", MasterGroupMappingID, Client.User.LanguageID);

      List<MasterGroupMappingSelectorModel> listOfSelectors = unit
        .ExecuteStoreQuery<MasterGroupMappingSelectorModel>(sqlQuery)
        .ToList();

      listOfSelectors.ForEach(masterGroupMapping =>
      {
        item.Info.Add(string.Format("'{0}' has selector '{1}'", masterGroupMapping.MasterGroupMappingName, masterGroupMapping.SelectorName));
      });

      if (item.Info.Count > 0)
      {
        listOfRelatedObjects.Add(item);
      }
      return listOfRelatedObjects;
    }
    private ArrayList GetRelatedConnectorProductGroupMapping(int MasterGroupMappingID, IServiceUnitOfWork unit)
    {
      ArrayList listOfRelatedObjects = new ArrayList();
      var item = new
      {
        ErrorMessage = "Related Product Group Mappings in Connector Mapping",
        Info = new List<string>()
      };

      string sqlQuery = string.Format(@"
        WITH masterGroupMappingChildren
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.MasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN masterGroupMappingChildren mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        SELECT m.*
	        ,ISNULL(ml.NAME, 'MasterGroupMapping Name Empty') AS MasterGroupMappingName
        FROM MasterGroupMapping m
        INNER JOIN masterGroupMappingChildren mc ON m.SourceMasterGroupMappingID = mc.MasterGroupMappingID
        LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
	        AND ml.LanguageID = {1}
        order by m.SourceMasterGroupMappingID
      ", MasterGroupMappingID, Client.User.LanguageID);

      List<MasterGroupMappingModel> listOfProductGroupMappings = unit
        .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
        .ToList();

      List<MasterGroupMappingModel> listOfMasterGroupMappingPaths = GetListOfMasterGroupMappingAndProductGroupMappingPaths();
      List<Connector> listOfConnectors = unit
        .Service<Connector>()
        .GetAll()
        .ToList();

      listOfProductGroupMappings.ForEach(masterGroupMapping =>
      {
        Connector connector = listOfConnectors.Where(x => x.ConnectorID == masterGroupMapping.ConnectorID).FirstOrDefault();
        MasterGroupMappingModel productGroupMapping = listOfMasterGroupMappingPaths.Where(x => x.MasterGroupMappingID == masterGroupMapping.MasterGroupMappingID).FirstOrDefault();

        item.Info.Add(string.Format("'{0}' is used in connector '{1}' (Path:{2})", masterGroupMapping.MasterGroupMappingName, connector.Name, productGroupMapping.MasterGroupMappingPath));
      });

      if (item.Info.Count > 0)
      {
        listOfRelatedObjects.Add(item);
      }
      return listOfRelatedObjects;
    }
    private ArrayList GetRelatedChildProductGroupMapping(int MasterGroupMappingID)
    {
      ArrayList listOfRelatedObjects = new ArrayList();
      var item = new
      {
        ErrorMessage = "Related Source Product Group Mappings in Connector Mapping",
        Info = new List<string>()
      };

      string sqlQuery = string.Format(@"
        WITH masterGroupMappingChildren
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.MasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN masterGroupMappingChildren mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        SELECT m.*
	        ,ISNULL(ml.NAME, 'MasterGroupMapping Name Empty') AS MasterGroupMappingName
        FROM MasterGroupMapping m
        INNER JOIN masterGroupMappingChildren mc ON m.SourceProductGroupMappingID = mc.MasterGroupMappingID
        LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
	        AND ml.LanguageID = {1}
        order by m.SourceMasterGroupMappingID
      ", MasterGroupMappingID, Client.User.LanguageID);
      using (var unit = GetUnitOfWork())
      {
        List<MasterGroupMappingModel> listOfSourceProductGroupMappings = unit
          .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
          .ToList();

        if (listOfSourceProductGroupMappings.Count > 0)
        {
          List<MasterGroupMappingModel> listOfMasterGroupMappingPaths = GetListOfMasterGroupMappingAndProductGroupMappingPaths();
          List<Connector> listOfConnectors = unit
            .Service<Connector>()
            .GetAll()
            .ToList();

          listOfSourceProductGroupMappings.ForEach(sourceProductGroupMapping =>
          {
            Connector connector = listOfConnectors.Where(x => x.ConnectorID == sourceProductGroupMapping.ConnectorID).FirstOrDefault();

            string productGroupMappingName = listOfMasterGroupMappingPaths
              .Where(x => x.MasterGroupMappingID == sourceProductGroupMapping.SourceProductGroupMappingID)
              .Select(x => x.MasterGroupMappingName)
              .FirstOrDefault();

            string sourceProductGroupMappingPath = listOfMasterGroupMappingPaths
              .Where(x => x.MasterGroupMappingID == sourceProductGroupMapping.MasterGroupMappingID)
              .Select(x => x.MasterGroupMappingPath)
              .FirstOrDefault();

            item.Info.Add(string.Format("'{0}' has child Product Group Mapping in connector '{1}' (Path:{2})",
              productGroupMappingName, connector.Name, sourceProductGroupMappingPath));
          });

          if (item.Info.Count > 0)
          {
            listOfRelatedObjects.Add(item);
          }
        }
      }
      return listOfRelatedObjects;
    }

    #endregion

    #region MasterGroupMapping Repository
    private List<MasterGroupMappingModel> GetListOfChildMasterGroupMappingsForMasterGroupMappingTree(int ParentMasterGroupMappingID, bool showProductCounter, int? ConnectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var canSeeUnassigned = Client.User.HasFunctionality(Functionalities.MasterGroupMappingViewUnassigned);

        var filterQuery = new StringBuilder();

        string sqlQuery = string.Empty;

        if (ParentMasterGroupMappingID > 0)
        {
          filterQuery.AppendFormat("ParentMasterGroupMappingID = {0}", ParentMasterGroupMappingID);
        }
        else
        {
          filterQuery.AppendFormat("ParentMasterGroupMappingID IS NULL");
        }

        if (ConnectorID.HasValue && ConnectorID.Value > 0)
        {
          sqlQuery = @";

          with customLabels as
          (

	          select * from 
	          (
		          select *,
			          row_number() over (partition by mastergroupmappingid order by connectorid desc, languageid desc) as rank_id
		          from mastergroupmappingcustomlabel  
		          where languageid = {1} and (connectorid is null or connectorid = " + ConnectorID.Value + @")
	          ) a
	          where a.rank_id = 1
          )

          SELECT m.*, ISNULL(mmCl.CustomLabel, ISNULL(ml.NAME, 'MasterGroupMapping NAME EMPTY!')) AS MasterGroupMappingName
          FROM MasterGroupMapping m
          {2}
          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
            AND ml.LanguageID = {1}

          LEFT JOIN customLabels mmCl on mmCl.MasterGroupMappingID = m.MasterGroupMappingID
	        

          WHERE {0}";

          filterQuery.AppendFormat(" AND m.ConnectorID = {0}", ConnectorID.Value);
        }
        else
        {
          sqlQuery = @"
          SELECT m.*, ISNULL(ml.NAME, 'MasterGroupMapping NAME EMPTY!') AS MasterGroupMappingName
          FROM MasterGroupMapping m
          {2}
          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
            AND ml.LanguageID = {1}          
          WHERE {0}";

          filterQuery.AppendFormat(" AND m.ConnectorID IS NULL AND m.MasterGroupMappingID > 0");
        }

        var assignedJoin = !(canSeeUnassigned || ConnectorID.HasValue)
          ? string.Format("inner join MasterGroupMappingUser mgmu on m.MasterGroupMappingID = mgmu.MasterGroupMappingID and mgmu.UserID = {0}", Client.User.UserID)
          : string.Empty;

        sqlQuery = string.Format(sqlQuery
          , filterQuery.ToString(),
          Client.User.LanguageID,
          assignedJoin
          );

        if (showProductCounter)
        {
          sqlQuery = string.Format(@"
          WITH ActiveAssortment AS ( 
                    SELECT MIN(VendorAssortmentID) AS VendorAssortmentID,
                           COUNT(1) AS CountVendorItemNumbers
                     FROM   dbo.VendorAssortment va
                     WHERE  IsActive = 1
                     GROUP  BY ProductID
                   ),
               ActiveProducts AS ( 
                    SELECT p.ProductID,
                           VendorItemNumber,
                           ShortDescription,
                           CountVendorItemNumbers,
                           p.IsBlocked,
                           NAME AS BrandName
                     FROM  dbo.Product p
                     INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
                     INNER JOIN dbo.Brand b ON p.BrandID = b.BrandID
                     INNER JOIN ActiveAssortment aa ON va.VendorAssortmentID = aa.VendorAssortmentID
                   ),
               highestMasterGroupMappings AS (
	                   SELECT RootID = m.MasterGroupMappingID,
                            m.MasterGroupMappingID
	                   FROM MasterGroupMapping m
	                   WHERE {0}
	                    
                     UNION ALL
	
	                   SELECT hm.RootID,
		                        m.MasterGroupMappingID
	                   FROM highestMasterGroupMappings hm
	                   INNER JOIN MasterGroupMapping m ON m.ParentMasterGroupMappingID = hm.MasterGroupMappingID
                  ),
	             productsCount as(
					           SELECT
					                  mgp.mastergroupmappingid,
					                  MIN(mgp.productid) as productid,
					                  MIN(convert(int,isapproved)) isapproved
					           FROM mastergroupmappingproduct mgp
                     INNER JOIN ActiveProducts ap on ap.productid=mgp.productid
					           INNER JOIN productmatch pm on mgp.productid = pm.productid and ismatched = 1
					           WHERE IsProductMapped = 1
					           GROUP BY mgp.mastergroupmappingid, pm.ProductID

					           UNION

                     SELECT mgp.mastergroupmappingid,
					                  mgp.productid,
					                  isapproved
					           FROM mastergroupmappingproduct mgp
                     INNER JOIN ActiveProducts ap on ap.productid=mgp.productid
					           LEFT JOIN productmatch pm on mgp.productid = pm.productid
					           WHERE pm.productmatchid is null and IsProductMapped = 1
					           UNION
					           SELECT mgp.mastergroupmappingid,
					                  mgp.productid,
					                  isapproved
					           FROM mastergroupmappingproduct mgp
                     INNER JOIN ActiveProducts ap on ap.productid=mgp.productid
					           INNER JOIN productmatch pm on mgp.productid = pm.productid and ismatched = 0
					           WHERE IsProductMapped = 1
	                ),
	             countProductsPerMasterGroupMapping AS (			
	                   SELECT mp.MasterGroupMappingID,
		                        COUNT(1) AS AmountProducts
	                   FROM highestMasterGroupMappings hm
	                   INNER JOIN productsCount mp ON hm.MasterGroupMappingID = mp.MasterGroupMappingID
	                   GROUP BY mp.MasterGroupMappingID
	                ),
	             countProductsByHighestMasterGroupMapping AS (
	                   SELECT MasterGroupMappingID = hm.RootID,
		                        countProducts = SUM(cm.AmountProducts)
	                   FROM highestMasterGroupMappings hm
	                   LEFT JOIN countProductsPerMasterGroupMapping cm ON hm.MasterGroupMappingID = cm.MasterGroupMappingID
	                   GROUP BY hm.RootID
	                ),
	             countRejectedProductsPerMasterGroupMapping AS (				          
	                   SELECT mp.MasterGroupMappingID,
		                        COUNT(1) AS AmountProducts
	                   FROM highestMasterGroupMappings hm
	                   INNER JOIN productsCount mp ON hm.MasterGroupMappingID = mp.MasterGroupMappingID
	                   WHERE mp.IsApproved = 0
	                   GROUP BY mp.MasterGroupMappingID
	                ),
               countRejenctedProductsByHighestMasterGroupMapping AS (
	                   SELECT MasterGroupMappingID = hm.RootID,
		                        countProducts = SUM(cm.AmountProducts)
	                   FROM highestMasterGroupMappings hm
	                   LEFT JOIN countRejectedProductsPerMasterGroupMapping cm ON hm.MasterGroupMappingID = cm.MasterGroupMappingID
	                   GROUP BY hm.RootID
	                ),
	             processCounting AS (
	                   SELECT hm.MasterGroupMappingID,
		                        cp.countProducts AS CountProducts,
		                        crp.countProducts AS CountRejectedProducts
	                   FROM highestMasterGroupMappings hm
	                   INNER JOIN countProductsByHighestMasterGroupMapping cp ON hm.MasterGroupMappingID = cp.MasterGroupMappingID
	                   INNER JOIN countRejenctedProductsByHighestMasterGroupMapping crp ON hm.MasterGroupMappingID = crp.MasterGroupMappingID
	                )
          SELECT m.*,
	            ISNULL(pc.CountProducts, 0) CountProducts,
	            ISNULL(pc.CountRejectedProducts, 0) CountNotApprovedProducts,
	            ISNULL(ml.NAME, 'MasterGroupMapping NAME EMPTY!') AS MasterGroupMappingName             
          FROM MasterGroupMapping m
          {2}
          INNER JOIN processCounting pc ON m.MasterGroupMappingID = pc.MasterGroupMappingID
          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID AND ml.LanguageID = {1}",
           filterQuery.ToString(),
           Client.User.LanguageID,
           assignedJoin);
        }

        List<MasterGroupMappingModel> listOfMasterGroupMappings = unit
          .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
          .OrderByDescending(x => x.Score)
          .ToList();

        return listOfMasterGroupMappings;
      }

    }

    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingHierarchy()
    {
      return GetListOfMasterGroupMappingHierarchy(0);
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingHierarchy(int MasterGroupMappingID)
    {
      return GetListOfMasterGroupMappingHierarchy(MasterGroupMappingID, null);
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingHierarchy(int MasterGroupMappingID, int? ConnectorID)
    {
      if (ConnectorID.HasValue && ConnectorID.Value > 0)
      {
        return GetListOfMasterGroupMappingHierarchy(MasterGroupMappingID, ConnectorID.Value, null);
      }
      else
      {
        return GetListOfMasterGroupMappingHierarchy(MasterGroupMappingID, null, null);
      }
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingHierarchy(int MasterGroupMappingID, int? ConnectorID, int? LanguageID)
    {
      using (var unit = GetUnitOfWork())
      {
        var isCurrentUserAdmin = Client.User.CurrentRole.RoleID == 1;

        var filterQuery = new StringBuilder();

        if (MasterGroupMappingID > 0)
        {
          filterQuery.AppendFormat("MasterGroupMappingID = {0}", MasterGroupMappingID);
        }
        else
        {
          filterQuery.AppendFormat("ParentMasterGroupMappingID IS NULL");
        }

        if (ConnectorID.HasValue && ConnectorID.Value > 0)
        {
          filterQuery.AppendFormat(" AND ConnectorID = {0}", ConnectorID.Value);
        }
        else
        {
          filterQuery.AppendFormat(" AND ConnectorID IS NULL AND m.MasterGroupMappingID > 0");
        }

        var filterAssignedUsersQuery = isCurrentUserAdmin
          ? string.Empty
          : string.Format("inner join MasterGroupMappingUser mgmu on m.MasterGroupMappingID = mgmu.MasterGroupMappingID and mgmu.UserID = {0}", Client.User.UserID);
        var queryFormat = @"
          WITH masterGroupMappingHierarchy
          AS (
	          SELECT m.MasterGroupMappingID
	          FROM MasterGroupMapping m
	          WHERE {0}
	
	          UNION ALL
	
	          SELECT m.MasterGroupMappingID
	          FROM MasterGroupMapping m
	          INNER JOIN masterGroupMappingHierarchy mh ON m.ParentMasterGroupMappingID = mh.MasterGroupMappingID
	          )
          SELECT m.*, ml.Name AS MasterGroupMappingName
          FROM MasterGroupMapping m
          INNER JOIN masterGroupMappingHierarchy mh ON m.MasterGroupMappingID = mh.MasterGroupMappingID
          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID AND ml.LanguageID = {1}
        "
        + filterAssignedUsersQuery;

        var languageId = (LanguageID.HasValue && (LanguageID.Value >= 0 && LanguageID.Value <= 5))
                           ? LanguageID.Value
                           : Client.User.LanguageID;

        var sqlQuery = string.Format(
          queryFormat,
          filterQuery,
          languageId
         );

        var listOfMasterGroupMappings = unit
          .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
          .OrderBy(x => x.Score)
          .ToList();

        return listOfMasterGroupMappings;
      }

    }

    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingPaths()
    {
      return GetListOfMasterGroupMappingPaths(null);
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingPaths(int LanguageID)
    {
      return GetListOfMasterGroupMappingPaths(null, LanguageID);
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingPaths(int? connectorID)
    {
      return GetListOfMasterGroupMappingPaths((connectorID.HasValue && connectorID.Value > 0) ? connectorID.Value : 0, Client.User.LanguageID);
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingPaths(int? connectorID, int languageID)
    {
      var filterQuery = new StringBuilder();
      if (connectorID.HasValue && connectorID.Value > 0)
      {
        filterQuery.AppendFormat(" ConnectorID = {0}", connectorID.Value);
      }
      else
      {
        filterQuery.AppendFormat(" ConnectorID IS NULL AND m.MasterGroupMappingID > 0");
      }
      using (var unit = GetUnitOfWork())
      {
        var sqlQuery = string.Format(@"
          WITH MasterGroupMappingNames
          AS (
	          SELECT m.MasterGroupMappingID
		          ,isnull(ml.NAME, 'MasterGroupMappin Name Empty') AS MasterGroupMappingName
	          FROM MasterGroupMapping m
	          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
		          AND ml.LanguageID = {0}
	          WHERE {1}
	          )
	          ,MasterGroupMappingPaths
          AS (
	          SELECT m.MasterGroupMappingID
		          ,m.ParentMasterGroupMappingID
		          ,CAST(mn.MasterGroupMappingName AS VARCHAR(max)) AS 'Path'
	          FROM MasterGroupMapping m
	          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
	          WHERE m.MasterGroupMappingID > 0
		          AND m.ParentMasterGroupMappingID IS NULL
	
	          UNION ALL
	
	          SELECT m.MasterGroupMappingID
		          ,m.ParentMasterGroupMappingID
		          ,CAST(cte.Path + ' -> ' + CAST(mn.MasterGroupMappingName AS VARCHAR(50)) AS VARCHAR(max)) AS 'Path'
	          FROM MasterGroupMapping m
	          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
	          INNER JOIN MasterGroupMappingPaths cte ON m.ParentMasterGroupMappingID = cte.MasterGroupMappingID
	          )
          SELECT m.*
	          ,mn.MasterGroupMappingName
	          ,cte.Path as MasterGroupMappingPath
          FROM MasterGroupMapping m
          INNER JOIN MasterGroupMappingPaths cte ON m.MasterGroupMappingID = cte.MasterGroupMappingID
          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
        ", languageID, filterQuery);

        List<MasterGroupMappingModel> listOfMasterGroupMappingPaths = unit
          .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
          .ToList();
        return listOfMasterGroupMappingPaths;
      }
    }

    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingAndProductGroupMappingPaths()
    {
      return GetListOfMasterGroupMappingAndProductGroupMappingPaths(Client.User.LanguageID);
    }
    private List<MasterGroupMappingModel> GetListOfMasterGroupMappingAndProductGroupMappingPaths(int LanguageID)
    {
      using (var unit = GetUnitOfWork())
      {
        string sqlQuery = string.Format(@"
          WITH MasterGroupMappingNames
          AS (
	          SELECT m.MasterGroupMappingID
		          ,isnull(ml.NAME, 'MasterGroupMappin Name Empty') AS MasterGroupMappingName
	          FROM MasterGroupMapping m
	          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
		          AND ml.LanguageID = {0}
	          WHERE m.MasterGroupMappingID > 0
	          )
	          ,MasterGroupMappingPaths
          AS (
	          SELECT m.MasterGroupMappingID
		          ,m.ParentMasterGroupMappingID
		          ,CAST(mn.MasterGroupMappingName AS VARCHAR(max)) AS 'Path'
	          FROM MasterGroupMapping m
	          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
	          WHERE m.MasterGroupMappingID > 0
		          AND m.ParentMasterGroupMappingID IS NULL
	
	          UNION ALL
	
	          SELECT m.MasterGroupMappingID
		          ,m.ParentMasterGroupMappingID
		          ,CAST(cte.Path + ' -> ' + CAST(mn.MasterGroupMappingName AS VARCHAR(50)) AS VARCHAR(max)) AS 'Path'
	          FROM MasterGroupMapping m
	          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
	          INNER JOIN MasterGroupMappingPaths cte ON m.ParentMasterGroupMappingID = cte.MasterGroupMappingID
	          )
          SELECT m.*
	          ,mn.MasterGroupMappingName
	          ,cte.Path as MasterGroupMappingPath
          FROM MasterGroupMapping m
          INNER JOIN MasterGroupMappingPaths cte ON m.MasterGroupMappingID = cte.MasterGroupMappingID
          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
        ", LanguageID);

        List<MasterGroupMappingModel> listOfMasterGroupMappingPaths = unit
          .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
          .ToList();
        return listOfMasterGroupMappingPaths;
      }
    }

    private MasterGroupMappingModel GetMasterGroupMappingPath(int MasterGroupMappingID)
    {
      return GetMasterGroupMappingPath(MasterGroupMappingID, Client.User.LanguageID);
    }
    private MasterGroupMappingModel GetMasterGroupMappingPath(int MasterGroupMappingID, int LanguageID)
    {
      using (var unit = GetUnitOfWork())
      {
        string sqlQuery = string.Format(@"
          WITH MasterGroupMappingNames
          AS (
	          SELECT m.MasterGroupMappingID
		          ,isnull(ml.NAME, 'MasterGroupMappin Name Empty') AS MasterGroupMappingName
	          FROM MasterGroupMapping m
	          LEFT JOIN MasterGroupMappingLanguage ml ON m.MasterGroupMappingID = ml.MasterGroupMappingID
		          AND ml.LanguageID = {1}
	          WHERE m.MasterGroupMappingID > 0
	          )
	          ,MasterGroupMappingPaths
          AS (
	          SELECT m.MasterGroupMappingID
		          ,m.ParentMasterGroupMappingID
		          ,CAST(mn.MasterGroupMappingName AS VARCHAR(max)) AS 'Path'
	          FROM MasterGroupMapping m
	          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
	          WHERE m.MasterGroupMappingID > 0
		          AND m.ParentMasterGroupMappingID IS NULL
	
	          UNION ALL
	
	          SELECT m.MasterGroupMappingID
		          ,m.ParentMasterGroupMappingID
		          ,CAST(cte.Path + ' -> ' + CAST(mn.MasterGroupMappingName AS VARCHAR(50)) AS VARCHAR(max)) AS 'Path'
	          FROM MasterGroupMapping m
	          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
	          INNER JOIN MasterGroupMappingPaths cte ON m.ParentMasterGroupMappingID = cte.MasterGroupMappingID
	          )
          SELECT m.*
	          ,mn.MasterGroupMappingName
	          ,cte.Path as MasterGroupMappingPath
          FROM MasterGroupMapping m
          INNER JOIN MasterGroupMappingPaths cte ON m.MasterGroupMappingID = cte.MasterGroupMappingID
          INNER JOIN MasterGroupMappingNames mn ON m.MasterGroupMappingID = mn.MasterGroupMappingID
          WHERE m.MasterGroupMappingID = {0}
        ", MasterGroupMappingID, LanguageID);

        MasterGroupMappingModel masterGroupMappingPath = unit
          .ExecuteStoreQuery<MasterGroupMappingModel>(sqlQuery)
          .FirstOrDefault();
        return masterGroupMappingPath;
      }
    }
    #endregion
  }
  #region ModelView
  public class TreeNodeModel
  {
    public int MasterGroupMappingID { get; set; }
    public string MasterGroupMappingPad { get; set; }
  }
  public class ListOfGroupNameResult
  {
    public bool IsMasterGroupMapping { get; set; }
    public int ID { get; set; }
    public string GroupName { get; set; }
    public List<string> LanguageTranslation { get; set; }
  }
  public class ListOfMasterGroupMappingLanguages
  {
    public int MasterGroupMappingID { get; set; }
    public string Name { get; set; }
    public int? LanguageID { get; set; }
    public string LanguageName { get; set; }
    public int? Matched { get; set; }
  }
  public class MatchedMasterGroupMappingProduct
  {
    public int ProductID { get; set; }
    public int? MasterGroupMappingID { get; set; }
  }
  public class MasterGroupMappingUnMatchItem
  {
    public int ProductID { get; set; }
    public int[] MasterGroupMappingIDs { get; set; }
  }
  public class MatchedMasterGroupMappingVendorProductGroup
  {
    public int MasterGroupMappingID { get; set; }
    public int[] ListOfVendorProductGroupIDs { get; set; }
  }
  public class MatchedVendorProductsPerGroupFoMasterGroupMapping
  {
    public int VendorProductGroupID { get; set; }
    public int[] ListOfVendorProductIDs { get; set; }
  }
  public class MasterGroupMappingPriceTagModel
  {
    public int MasterGroupMappingID { get; set; }

    public string Name { get; set; }

    public Dictionary<string, bool> PriceTags { get; set; }
  }
  public class MasterGroupMappingModel : MasterGroupMapping
  {
    public string MasterGroupMappingName { get; set; }
    public string MasterGroupMappingPath { get; set; }
    public Int32 CountProducts { get; set; }
    public Int32 CountNotApprovedProducts { get; set; }
  }
  //public class RelatedMasterGroupMappingModel : RelatedMasterGroupMapping
  //{
  //  public string MasterGroupMappingName { get; set; }
  //  public string RelatedMasterGroupMappingName { get; set; }
  //  public string RelatedType { get; set; }
  //}
  public class MasterGroupMappingCrossReferenceModel : MasterGroupMapping
  {
    public Int32 CrossReferenceID { get; set; }
    public string MasterGroupMappingName { get; set; }
  }
  public class MasterGroupMappingSelectorModel : MasterGroupMapping
  {
    public string MasterGroupMappingName { get; set; }
    public string SelectorName { get; set; }
  }
  internal class MasterGroupmappingFilter
  {
    public int ProductID { get; set; }
    public int MasterGroupMappingID { get; set; }
    public string Name { get; set; }
    public string VendorItemNumber { get; set; }
    public bool isApproved { get; set; }
  }
  internal class MasterGroupMappingProducAttributeName
  {
    public int AttributeID { get; set; }
    public string Name { get; set; }
    public string GroupName { get; set; }
    public string VendorName { get; set; }
  }
  internal class MasterGroupMappingProductMatch
  {
    public int sourceproductid { get; set; }
    public bool IsMatched { get; set; }
    public int ProductID { get; set; }
    public string VendorItemNumber { get; set; }
    public string VendorName { get; set; }
    public int MatchStatus { get; set; }
    public string Brand { get; set; }
    public int? MatchPercentage { get; set; }
    public bool Primary { get; set; }
    public string ShortDescription { get; set; }
    public int VendorID { get; set; }
  }
  internal class MasterGroupMappingProductMatchAttribute
  {
    public int ProductID { get; set; }
    public int AttributeID { get; set; }
    public int AttributeValueID { get; set; }
    public int? MaxLength { get; set; }
    public string Value { get; set; }
    public string Name { get; set; }
  }
  internal class ProductMatchWizardItems
  {
    public int ProductID { get; set; }
    public bool IsMatch { get; set; }
  }
  internal class UpdateConnectorMappingSetting
  {
    public Int32 MasterGroupMappingID { get; set; }
    public bool FilterByParentGroup { get; set; }
    public bool FlattenHierachy { get; set; }
    public Int32? ExportID { get; set; }
    public bool FollowSystemSettings { get; set; }
    public bool DisableInNavigation { get; set; }
    public bool DisableInProductGroup { get; set; }
    public bool ListView { get; set; }
    public Int32? PageLayoutID { get; set; }
    public List<WposConnectorMappingSetting> WposConnectorMappingSettings { get; set; }
    public List<ConnectorGroupSetting> ConnectorGroupSettings { get; set; }
    public List<GenericSetting> GenericSettings { get; set; }
  }
  internal class WposConnectorMappingSetting
  {
    public string Code { get; set; }
    public string SettingValue { get; set; }
  }
  internal class ConnectorGroupSetting
  {
    public int ConnectorID { get; set; }
    public bool IsActive { get; set; }
  }
  internal class ProductAttributeValueModel
  {
    public Int32 ProductID { get; set; }
    public string ShortDescription { get; set; }
    public string BrandName { get; set; }
    public string VendorItemNumber { get; set; }
    public Int32? AttributeValueID { get; set; }
    public Int32? AttributeID { get; set; }
    public string Value { get; set; }
  }
  public class SeoText
  {
    public string Description { get; set; }
    public string Description2 { get; set; }
    public string Description3 { get; set; }
    public string Meta_title { get; set; }
    public string Meta_description { get; set; }
    public int MasterGroupMappingID { get; set; }
    public int LanguageID { get; set; }
    public int ConnectorID { get; set; }
  }
  public class ConnectorMappingProducts
  {
    public int ContentProductGroupID { get; set; }
    public int ProductID { get; set; }
    public int MasterGroupMappingID { get; set; }
    public string ShortContentDescription { get; set; }
    public string LongContentDescription { get; set; }
    public string VendorItemNumber { get; set; }
    public string ProductName { get; set; }
    public bool IsExported { get; set; }
    public bool IsConfigurable { get; set; }
  }
  public class MasterGroupMappingCustomLabelModel
  {
    public int MasterGroupMappingID { get; set; }

    public int LanguageID { get; set; }

    public string Language { get; set; }

    public string CustomLabel { get; set; }

    public int? ConnectorID { get; set; }

    public string ConnectorName { get; set; }
  }

  public class GenericSetting
  {
    public int SettingID { get; set; }
    public string Value { get; set; }
  }
  #endregion
}

