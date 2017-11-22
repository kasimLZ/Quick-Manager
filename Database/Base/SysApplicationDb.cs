using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Database.Base
{
    public class SysApplicationDb<T> : DbContext, IApplicationDB
        where T : DbContext
    {
        public SysApplicationDb(DbContextOptions<ApplicationDB> option) : base(option)
        {
        }
        
        #region Base Database Register
        public DbSet<SysUserInfo> SysUserInfos { get; set; }
        public DbSet<SysArea> SysAreas { get; set; }
        public DbSet<SysAction> SysActions { get; set; }
        public DbSet<SysController> SysControllers { get; set; }
        public DbSet<SysControllerSysAction> SysControllerSysActions { get; set; }
        public DbSet<SysRole> SysRoles { get; set; }
        public DbSet<SysRoleSysControllerSysAction> SysRoleSysControllerSysAction { get; set; }
        public DbSet<SysRoleSysUserInfo> SysRoleSysUserInfo { get; set; }

        #endregion

        public virtual int Commit()
        {
            return base.SaveChanges();
        }

        public virtual Task<int> CommitAsync()
        {
            return base.SaveChangesAsync();
        }

        EntityEntry IApplicationDB.Entry(object entity)
        {
            return base.Entry(entity);
        }

        EntityEntry<TEntity> IApplicationDB.Entry<TEntity>(TEntity entity)
        {
            return base.Entry(entity);
        }
       
    }
}
