using Database.Base.Model;
using System;

namespace Database.Base.Interface
{
    public interface CurrentUserInterface
    {
        //LoginInfo UserInfo { get; }

        bool IsLogin { get; }

        void SetLoginInfo(SysUserInfo UserInfo, bool createPersistentCookie);

        void Logout();
    }
}
