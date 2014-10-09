[assembly: WebActivator.PreApplicationStartMethod(typeof(Concentrator.ui.Management.App_Start.CombresHook), "PreStart")]
namespace Concentrator.ui.Management.App_Start {
	using System.Web.Routing;
	using Combres;
	
    public static class CombresHook {
        public static void PreStart() {
            RouteTable.Routes.AddCombresRoute("Combres");
        }
    }
}