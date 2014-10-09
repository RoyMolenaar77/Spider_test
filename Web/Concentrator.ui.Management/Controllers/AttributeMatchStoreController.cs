using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
    public class AttributeMatchStoreController : BaseController
    {
      [RequiresAuthentication(Functionalities.UpdateAttributeMatchStore)]
      public ActionResult Update(int AttributeStoreID)
      {
        return Update<AttributeMatchStore>(c => c.AttributeStoreID == AttributeStoreID);
      }
    }
}
