using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Web.Security
{
    public static class IdentityServerMiddleware
    {
        public static void RegisterIdentityServer(this IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddSigningCredential(new X509Certificate2(@"Security\SecurityCertificate.pfx", "SecurityCertificate"))
                .AddInMemoryIdentityResources(Configuration.IdentityResources)
                .AddTestUsers(Configuration.Users)
                .AddInMemoryClients(Configuration.Clients)
                 .AddInMemoryApiResources(Configuration.ApiResources());
        }
    }
}
