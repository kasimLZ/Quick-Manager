using Database.Base.Interface;
using Database.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Security
{
    public interface IAuthRepository
    {
        SysUserInfo GetUserById(string id);
        SysUserInfo GetUserByUsername(string username);
        bool ValidatePassword(string username, string plainTextPassword);
    }

    public class AuthRepository : IAuthRepository
    {
        private readonly SysUserInfoInterface _iSysUserInfoService;

        public AuthRepository(SysUserInfoInterface sysUserInfoInterface)
        {
            _iSysUserInfoService = sysUserInfoInterface;
        }

        public SysUserInfo GetUserById(string id)
        {
            long Uid = 0;
            if (long.TryParse(id, out Uid))
            {
                return _iSysUserInfoService.GetById(Uid);
            }
            else
            {
                return null;
            }
           
        }

        public SysUserInfo GetUserByUsername(string username)
        {
            return _iSysUserInfoService.GetAll(a => a.Login.Equals(username)).FirstOrDefault();
        }

        public bool ValidatePassword(string username, string plainTextPassword)
        {
            var user = GetUserByUsername(username);
            if (user == null) return false;
            if (String.Equals(plainTextPassword, user.Password)) return true;
            return false;
        }
    }
}
