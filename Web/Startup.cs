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

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDB>(option =>
            {
                option.UseSqlServer("Data Source=localhost; Initial Catalog=DotNetCore; User ID=sa;Password=abc!123;Trusted_Connection=True");
            });
            services.AddTransient<CurrentUserInterface, CurrentUser>();
            services.AddServices<ApplicationDB>();//批量注入

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMvc(route => {
                route.RegisterAllArea();
                route.RegisterRoutes();
            });
            app.RegisterStaticFilePath(Configuration);
            

            SFID.WorkerID = long.Parse(ConfigurationManager.AppSettings("MachineId"));
        }
    }
}
