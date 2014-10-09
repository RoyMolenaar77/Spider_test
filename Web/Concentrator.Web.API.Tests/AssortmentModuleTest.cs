using System;

using Xunit;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Testing;

namespace Concentrator.Web.API.Tests
{
  public class AssortmentModuleTest
  {
    private const String AssortmentPath = "http://localhost/Concentrator.Web.API/Assortment";
    private const String Prefix = "Assortment Module Test: ";

    private Browser CreateBrowser()
    {
      return new Browser(bootstrapper =>
      {
      });
    }

    [Fact(DisplayName = Prefix + "Get complete assortment")]
    public void GetCompleteAssortment()
    {
      var result = CreateBrowser().Get(AssortmentPath, context =>
      {
        //context.Query
      });

      Assert.Equal(result.StatusCode, HttpStatusCode.OK);
    }
  }
}
