using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Products;

namespace Concentrator.ui.Management.Controllers
{
  public class ThumbnailController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetThumbnail)]
    public ActionResult GetList()
    {
      return List(unit => (from t in unit.Service<ThumbnailGenerator>().GetAll()
                           select new
                           {
                             t.ThumbnailGeneratorID,
                             t.Width,
                             t.Height,
                             t.Resolution,
                             t.Description
                           }));
    }

    [RequiresAuthentication(Functionalities.UpdateThumbnail)]
    public ActionResult Update(int id)
    {
      return Update<ThumbnailGenerator>(x => x.ThumbnailGeneratorID == id);
    }

    [RequiresAuthentication(Functionalities.GetThumbnail)]
    public ActionResult GetThumbnails(int thumbnailGeneratorID)
    {
      return List(unit => (from i in unit.Service<ProductMedia>().GetAll()
                           //where i.ThumbnailGenerators.Any(x => x.ThumbnailGeneratorID == thumbnailGeneratorID)
                           select new
                           {
                            i.MediaID,
                            i.MediaPath,
                            i.FileName,
                            i.Description,
                            OriginalResolution = i.Resolution,
                            OriginalSize = i.Size
                           }));
    }
  }
}
