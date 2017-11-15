using System;
using Database.Base.Interface;
using Database.Base.Model;
using Database.Base.Service.Infrastructure;
using System.Linq.Expressions;
using System.Linq;
using System.Collections;
using Database.Base.Interface.Infrastructure;

namespace DataService.EntityFramework.Base.Service
{
    public class SysRoleService : RepositoryBase<SysRole>, SysRoleInterface
    {
        public SysRoleService(IDatabaseFactory databaseFactory, CurrentUserInterface userInfo)
            : base(databaseFactory, userInfo)
        {
        }
    }
}
