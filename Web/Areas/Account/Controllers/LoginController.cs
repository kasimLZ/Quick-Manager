using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Account.Controllers
{
    [Area("Account")]
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}