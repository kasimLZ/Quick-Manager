using Database.Base.Interface;
using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using Database.Base.Service.Infrastructure;
using System.Linq;

namespace Database.Base.Service
{
    public class SysUserInfoService : RepositoryBase<SysUserInfo>, SysUserInfoInterface
    {
        public SysUserInfoService(IDatabaseFactory databaseFactory, CurrentUserInterface userInfo)
            : base(databaseFactory, userInfo)
        {
        }

        public SysUserInfo GetUser(string UserName, string Password)
        {
            return base.GetAll(a => a.Login == UserName && a.Password == Password).FirstOrDefault();
        }
    }
}
