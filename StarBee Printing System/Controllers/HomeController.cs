using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StarBee_Printing_System.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Home()
        {
            if (Session["userEmail"] == null)
            {
                return RedirectToAction("LogIn", "Authentication");
            }

            return View();
        }

        public ActionResult Content()
        {
            return View();
        }
    }
}