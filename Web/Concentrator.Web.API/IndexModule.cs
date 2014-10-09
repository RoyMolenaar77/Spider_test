using System.Web.UI.WebControls;

namespace Concentrator.Web.API
{
  using Nancy;

  public class IndexModule : NancyModule
  {
    public IndexModule()
    {
      Get["/"] = parameters =>
      {
        return View["index"];
      };
    }
  }
}