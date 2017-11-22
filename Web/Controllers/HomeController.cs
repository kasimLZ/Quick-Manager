using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Database.Base.Interface;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly CurrentUserInterface _CurrentUser;
        private readonly SysControllerInterface _sysController;

        public HomeController(CurrentUserInterface iCurrentUser, SysControllerInterface isysController)
        {
            _CurrentUser = iCurrentUser;
            _sysController = isysController;
        }
        

        public IActionResult Index()
        {
            //var userinfo = _CurrentUser.UserInfo;
            var model = _sysController.GetAll(
                a =>
                    a.Display && a.Enabled && (a.SysAreaId == null || (a.SysArea.Display && a.SysArea.Enabled)) &&
                    a.SysControllerSysActions.Any(
                        b =>
                            b.SysRoleSysControllerSysActions.Any(
                                c =>
                                    c.SysRole.SysRoleSysUsers.Any(
                                        d => d.SysUserId.Equals(1L))
                                        ))

                ).ToList();

            return View(model);
        }
    }
}
