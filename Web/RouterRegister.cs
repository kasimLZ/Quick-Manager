using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Web.Helper;

namespace Web
{
    public static class RouteConfiger
    {
        public static void RegisterRoutes(this IRouteBuilder route)
        {
            route.MapRoute(
                 name: "default",
                 template: "{controller=Home}/{action=Index}/{id?}");

            route.MapRoute(
                 name: "AreaDefault",
                 template: "{area:exists}/{controller=Index}/{action=Index}/{id?}"
               );
        }

        public static void RegisterAllArea(this IRouteBuilder route)
        {
            var AreaRegs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAreaRegister))))
                .ToArray();
            foreach (Type reg in AreaRegs)
            {
                IAreaRegister register = (IAreaRegister)Activator.CreateInstance(reg);
                register.RegisterArea(route);
            }
        }

        public static void RegisterStaticFilePath(this IApplicationBuilder app, IConfiguration config)
        {
            foreach (var option in config.GetSection("StaticFilePath").GetChildren())
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), option.GetSection("Relative").Value)),
                    RequestPath = new PathString(option.GetSection("Request").Value)
                });
            }
        }
    }

   
}
