using Database.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class ApplicationDB : SysApplicationDb
    {
        public ApplicationDB(DbContextOptions<ApplicationDB> options) : base(options)
        {

        }
    }
}
