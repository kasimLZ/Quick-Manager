using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Database;
using Microsoft.EntityFrameworkCore;
using Database.Base.Interface;
using Database.Base.Infrastructure;
using Common.DataTool;
using Common.Configuration;
using Web.Helper;
using Web.Security;
using Microsoft.EntityFrameworkCore.Design;

namespace Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public string connectionString { get { return Configuration.GetConnectionString(HostingEnvironment.IsDevelopment() ? "DevelopmentConnection" : "DefaultConnection"); } }

        public Startup(IConfiguration configuration, IHostingEnvironment host)
        {
            //throw new Exception();
            Configuration = configuration;
            HostingEnvironment = host;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDB>(option =>{option.UseSqlServer(connectionString); });


            services.AddTransient<CurrentUserInterface, CurrentUser>();
            services.ServicesRegisterFactory<ApplicationDB>();//批量注入

            services.RegisterIdentityServer();

            services.AddApplicationInsightsTelemetry(Configuration);

            //Register a global custom identity filter
            services.AddMvc(option =>{ option.Filters.Add(new AuthorizeRoleFilter()); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseIdentityServer();

            app.UseMvc(route => {
                route.RegisterAllArea();
                route.RegisterRoutes();
            });
            app.RegisterStaticFilePath(Configuration);
            

            SFID.WorkerID = long.Parse(ConfigurationManager.AppSettings("MachineId"));
        }


    }

   
}
