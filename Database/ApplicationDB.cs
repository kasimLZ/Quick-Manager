using Database.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class ApplicationDB : SysApplicationDb<ApplicationDB>
    {
        public ApplicationDB(DbContextOptions<ApplicationDB> options) : base(options)
        {
        }
    }

    public class ApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDB>
    {
        public ApplicationDB CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDB>();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                     .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                     .AddJsonFile("appsettings.json").Build();

            builder.UseSqlServer(configuration.GetConnectionString("DevelopmentConnection"));
            return new ApplicationDB(builder.Options);
        }

    }

}
