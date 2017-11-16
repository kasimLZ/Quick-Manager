using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Database.Base.Interface;
using Database.Base.Model;
using Database.Base.Interface.Infrastructure;

namespace Web.Areas.Desktop.Controllers
{
    [Area("Desktop")]
    public class IndexController : Controller
    {
        private readonly SysUserInfoInterface _iSysUser;

        public IndexController(SysUserInfoInterface sysUserInfo)
        {
            _iSysUser = sysUserInfo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Read()
        {
            return Content(_iSysUser.GetAll(a => a.Login.Equals("admin")).First().UserName);
        }

        public IActionResult test()
        {

            return View();
        }


        public IActionResult Write()
        {
            var u = new SysUserInfo
            {
                Password = "1"
            };
            u.Login = u.RealName = u.UserName = u.Id.ToString();

            _iSysUser.Save(null, u);
            //_iSysUser.Commit();
            return Content(u.Id.ToString());
        }
    }
}