using System;
using System.Web.Mvc;

using System.Linq;
using System.Collections.Generic;

using Concentrator.Objects;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Faq;

namespace Concentrator.Administration.Controllers
{
  public class FaqController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetFaq)]
    public ActionResult GetListByProduct(int productID)
    {

      return List(unit => from f in unit.Service<FaqTranslation>().GetAll()
                          join p in unit.Service<FaqProduct>().GetAll(x => x.ProductID == productID) on f.FaqID equals p.FaqID into prodfaqs
                          from faq in prodfaqs
                          select new
                          {
                            f.FaqID,
                            f.Question,
                            f.LanguageID,
                            f.Faq.Mandatory,
                            faq.Answer,
                            LastModificationTime = f.LastModificationTime
                          });

    }

    [RequiresAuthentication(Functionalities.GetFaq)]
    public ActionResult GetList(int? productID)
    {
      if (productID.HasValue)
      {
        return List(unit => from f in unit.Service<Faq>().GetAll()
                            //join p in unit.Service<FaqProduct>().GetAll() on productID equals p.ProductID into prodfaqs
                            //from faq in prodfaqs
                            where !f.FaqProducts.Any(c => c.ProductID == productID.Value)
                            from ft in f.FaqTranslations
                            select new
                            {
                              ft.FaqID,
                              ft.Question,
                              ft.LanguageID,
                              f.Mandatory,
                              //faq.Answer,
                              CreationTime = ft.CreationTime.ToLocalTime(),
                              LastModificationTime = ft.LastModificationTime.ToNullOrLocal()
                            });
      }
      return List(unit => from f in unit.Service<FaqTranslation>().GetAll()
                          select new
                                          {
                                            f.FaqID,
                                            f.Question,
                                            f.LanguageID,
                                            f.Faq.Mandatory,
                                            CreationTime = f.CreationTime.ToLocalTime(),
                                            LastModificationTime = f.LastModificationTime.ToNullOrLocal()
                                          });

    }

    [RequiresAuthentication(Functionalities.UpdateFaq)]
    public ActionResult UpdateForProduct(int id, int productID, string Answer)
    {
      var success = true;
      var message = "Faq successfully updated";
      using (var unit = GetUnitOfWork())
      {
        var UserID = Concentrator.Objects.Web.Client.User.UserID;

        var FaqProduct = unit.Service<FaqProduct>().Get(x => x.FaqID == id && x.ProductID == productID);

        FaqProduct.Answer = Answer;
        FaqProduct.LastModificationTime = DateTime.Now;
        FaqProduct.LastModifiedBy = UserID;

        try
        {
          unit.Save();
        }

        catch (Exception ex)
        {
          success = false;
          message = "Failed to update faq" + ex.Message;
        }

        return Json(new
        {
          success,
          message
        });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateFaq)]
    public ActionResult AddForProduct(int faqID, int productID)
    {
      var success = true;
      var message = "Faq successfully added";
      using (var unit = GetUnitOfWork())
      {
        var UserID = Concentrator.Objects.Web.Client.User.UserID;

        var FaqTranslation = unit.Service<FaqTranslation>().Get(x => x.FaqID == faqID);

        var FaqProduct = new FaqProduct
        {
          FaqID = faqID,
          ProductID = productID,
          CreatedBy = UserID,
          CreationTime = DateTime.Now.ToUniversalTime(),
          LastModifiedBy = UserID,
          LastModificationTime = DateTime.Now.ToUniversalTime(),
          LanguageID = FaqTranslation.LanguageID
        };

        unit.Service<FaqProduct>().Create(FaqProduct);

        try
        {
          unit.Save();
        }

        catch (Exception ex)
        {
          success = false;
          message = "Failed to add faq" + ex.Message;
        }

        return Json(new
        {
          success,
          message
        });
      }
    }

    [RequiresAuthentication(Functionalities.CreateFaq)]
    public ActionResult Create(int LanguageID, string Question, string Mandatory)
    {
      bool isMandatory = false;

      if (Mandatory != null)
      {
        isMandatory = true;
      }
      var success = true;
      var message = "Faq successfully created";
      using (var unit = GetUnitOfWork())
      {
        var UserID = Concentrator.Objects.Web.Client.User.UserID;

        var Faq = new Faq
        {
          Mandatory = isMandatory,
          CreatedBy = UserID,
          CreationTime = DateTime.Now,
          LastModifiedBy = UserID,
          LastModificationTime = DateTime.Now

        };

        var FaqTranslation = new FaqTranslation
        {
          Faq = Faq,
          LanguageID = LanguageID,
          Question = Question,
          CreatedBy = UserID,
          CreationTime = DateTime.Now.ToUniversalTime(),
          LastModifiedBy = UserID,
          LastModificationTime = DateTime.Now.ToUniversalTime()

        };

        unit.Service<Faq>().Create(Faq);
        unit.Service<FaqTranslation>().Create(FaqTranslation);

        try
        {
          unit.Save();
        }

        catch (Exception ex)
        {
          success = false;
          message = "Faq creation failed" + ex.Message;
        }

        return Json(new
        {
          success,
          message
        });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateFaq)]
    public ActionResult Update(int id, bool? Mandatory, int? LanguageID, string Question)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var UserID = Concentrator.Objects.Web.Client.User.UserID;

          var FaqTranslationToUpdate = unit.Service<FaqTranslation>().Get(x => x.FaqID == id);

          if (Mandatory.HasValue) { FaqTranslationToUpdate.Faq.Mandatory = Mandatory; }
          if (LanguageID.HasValue) { FaqTranslationToUpdate.LanguageID = LanguageID.Value; }
          if (Question != null) { FaqTranslationToUpdate.Question = Question; }
          FaqTranslationToUpdate.LastModifiedBy = UserID;
          FaqTranslationToUpdate.LastModificationTime = DateTime.Now;

          unit.Save();

          return Success("Succesfully updated faq transaction");
        }
        catch (Exception ex)
        {
          return Failure(string.Format("Failed to update faq transaction, reason: {0}", ex.Message));
        }
      }
    }

    [RequiresAuthentication(Functionalities.DeleteFaq)]
    public ActionResult Delete(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var FaqTransToDelete = unit.Service<FaqTranslation>().Get(x => x.FaqID == id);
          if (FaqTransToDelete != null)
          {
            unit.Service<FaqTranslation>().Delete(FaqTransToDelete);
            unit.Service<Faq>().Delete(FaqTransToDelete.Faq);
            unit.Save();
          }
          return Success("Succesfully deleted faq transaction");
        }
        catch (Exception ex)
        {
          return Failure(string.Format("Failed to delete faq transaction, reason: {0}", ex.Message));
        }
      }
    }

    [RequiresAuthentication(Functionalities.DeleteFaq)]
    public ActionResult DeleteForProduct(int productID, int id)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var FaqProductToDelete = unit.Service<FaqProduct>().Get(x => x.ProductID == productID && x.FaqID == id);
          if (FaqProductToDelete != null)
          {
            unit.Service<FaqProduct>().Delete(FaqProductToDelete);
            unit.Save();
          }
          return Success("Succesfully deleted faq");
        }
        catch (Exception ex)
        {
          return Failure(string.Format("Failed to delete faq, reason: {0}", ex.Message));
        }
      }
    }

  }
}
