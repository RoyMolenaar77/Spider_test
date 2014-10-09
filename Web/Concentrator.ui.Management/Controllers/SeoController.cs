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

namespace Concentrator.ui.Management.Controllers
{
  public class SeoController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductGroupMapping)]
    public ActionResult GetSeoTexts(int ProductGroupMappingID, int? languageID)
    {
      using (var unit = GetUnitOfWork())
      {
        if (languageID == null)
          languageID = Client.User.LanguageID;

        var seoTexts = unit.Service<SeoTexts>().GetAll(x => x.ProductGroupMappingID == ProductGroupMappingID && x.LanguageID == languageID);

        string description1, description2, description3, pageTitle, metaDescription;
        description1 = description2 = description3 = pageTitle = metaDescription = null;

        if (seoTexts.FirstOrDefault() != null)
        {
          description1 = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Description).Description;
          description2 = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Description2).Description;
          description3 = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Description3).Description;
          pageTitle = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Meta_title).Description;
          metaDescription = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Meta_description).Description;
        }

        return Json(new
        {
          success = true,
          data = new
          {
            description1 = description1,
            description2 = description2,
            description3 = description3,
            pageTitle = pageTitle,
            metaDescription = metaDescription
          }
        });
      }
    }

    [ValidateInput(false)]
    [RequiresAuthentication(Functionalities.UpdateProductGroupMapping)]
    public ActionResult UpdateSeoTexts(string description1, string description2, string description3, string pageTitle, string metaDescription, int productGroupMappingID, int languageID)
    {
      using (var unit = GetUnitOfWork())
      {

        var repo = unit.Service<SeoTexts>();
        var seoTexts = repo.GetAll(x => x.ProductGroupMappingID == productGroupMappingID && x.LanguageID == languageID);

        var desc1 = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Description);
        if (desc1 != null)
          desc1.Description = description1;
        else
          repo.Create(new SeoTexts { ProductGroupMappingID = productGroupMappingID, LanguageID = languageID, DescriptionType = (int)SeoDescriptionTypes.Description, Description = description1 });

        var desc2 = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Description2);
        if (desc2 != null)
          desc2.Description = description2;
        else
          repo.Create(new SeoTexts { ProductGroupMappingID = productGroupMappingID, LanguageID = languageID, DescriptionType = (int)SeoDescriptionTypes.Description2, Description = description2 });

        var desc3 = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Description3);
        if (desc3 != null)
          desc3.Description = description3;
        else
          repo.Create(new SeoTexts { ProductGroupMappingID = productGroupMappingID, LanguageID = languageID, DescriptionType = (int)SeoDescriptionTypes.Description3, Description = description3 });

        var paget = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Meta_title);
        if (paget != null)
          paget.Description = pageTitle;
        else
          repo.Create(new SeoTexts { ProductGroupMappingID = productGroupMappingID, LanguageID = languageID, DescriptionType = (int)SeoDescriptionTypes.Meta_title, Description = pageTitle });

        var metaDesc = seoTexts.FirstOrDefault(x => x.DescriptionType == (int)SeoDescriptionTypes.Meta_description);
        if (metaDesc != null)
          metaDesc.Description = metaDescription;
        else
          repo.Create(new SeoTexts { ProductGroupMappingID = productGroupMappingID, LanguageID = languageID, DescriptionType = (int)SeoDescriptionTypes.Meta_description, Description = metaDescription });

        unit.Save();
      }


      return Success("Successfully updated seo texts. ");
    }

    [RequiresAuthentication(Functionalities.CreateProductGroupMapping)]
    public ActionResult Create()
    {
      return Create<SeoTexts>();
    }
    
  }
}
