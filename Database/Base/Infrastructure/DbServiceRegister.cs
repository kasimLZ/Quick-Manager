using Database.Base.Interface.Infrastructure;
using Database.Base.Service.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Database.Base.Infrastructure
{
    public static class DbServiceRegister
    {
        public static void ServicesRegisterFactory<TDbContext>(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseFactory, DatabaseFactory>();
            services.AddScoped(typeof(TDbContext).GetInterface("I" + typeof(TDbContext).Name), typeof(TDbContext));
            Assembly.GetAssembly(typeof(TDbContext)).GetTypes().Where(a => a.Name.EndsWith("Service")).ToList().ForEach(Service => {
                Service.GetInterfaces().Where(a => a.Name.EndsWith("Interface")).ToList().ForEach(Intface =>
                {
                    services.AddScoped(Intface, Service); 
                });
            });
        }
    }
}
