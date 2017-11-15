using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Database.Base
{
    public class SysApplicationDb : DbContext , IApplicationDb
    {
        public SysApplicationDb(DbContextOptions option) : base(option)
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

        EntityEntry IApplicationDb.Entry(object entity)
        {
            return base.Entry(entity);
        }

        EntityEntry<TEntity> IApplicationDb.Entry<TEntity>(TEntity entity)
        {
            return base.Entry(entity);
        }
        
    }
}
