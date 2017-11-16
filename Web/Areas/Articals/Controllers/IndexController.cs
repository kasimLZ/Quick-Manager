using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Articals.Controllers
{
    [Area("Articals")]
    public class IndexController : Controller
    {

        public IActionResult Index()
        {
            return Content("aaa");
        }

        public IActionResult Error()
        {
            return Content("Error");
        }
    }
}