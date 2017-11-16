using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Web.Helper;

namespace Web.Areas.Desktop
{
    public class DesktopAreaRegister : IAreaRegister
    {
        public string AreaName
        {
            get { return "Desktop"; }
        }
        
        public void RegisterArea(IRouteBuilder route)
        {
            route.MapRoute(
                name: "text",
                template : "test",
                defaults: new { area = "Articals", controller = "Index", action = "Error" }
                );
        }
    }
}
