using Database.Base.Interface.Infrastructure;
using Database.Base.Model;

namespace Database.Base.Interface
{
    public interface SysUserInfoInterface : IRepository<SysUserInfo>
    {

        SysUserInfo GetUser(string UserName, string Password);
    }
}
