using Database.Base.Interface;
using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using Database.Base.Service.Infrastructure;

namespace DataService.EntityFramework.Base.Service
{
    public class SysRoleSysUserInfoService : RepositoryBase<SysRoleSysUserInfo>, SysRoleSysUserInfoInterface
    {
        public SysRoleSysUserInfoService(IDatabaseFactory databaseFactory, CurrentUserInterface userInfo) 
            : base(databaseFactory, userInfo)
        {
        }
    }
}
