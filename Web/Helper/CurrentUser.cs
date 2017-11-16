using Database.Base.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Base.Model;

namespace Web.Helper
{
    public class CurrentUser : CurrentUserInterface
    {
        public bool IsLogin => throw new NotImplementedException();

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public void SetLoginInfo(SysUserInfo UserInfo, bool createPersistentCookie)
        {
            throw new NotImplementedException();
        }
    }
}
