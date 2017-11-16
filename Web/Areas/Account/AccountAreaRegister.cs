using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace Web.Areas.Account
{
    public class AccountAreaRegister : IAreaRegister
    {
        public string AreaName
        {
            get { return "Account"; }
        }

        public void RegisterArea(IRouteBuilder route)
        {
            route.MapRoute(
                 name: "Login",
                 template: "Login",
                 defaults: new { area = "Account", controller = "Login", action = "Index" }
                 );
        }
    }
}
