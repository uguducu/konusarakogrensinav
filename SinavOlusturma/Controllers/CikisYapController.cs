using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinavOlusturma.Controllers
{
    public class CikisYapController : Controller
    {
        public IActionResult Index()
        {
            HttpContext.Session.Remove("OturumAnahtari");

            return Redirect("/Login");
        }
    }
}
