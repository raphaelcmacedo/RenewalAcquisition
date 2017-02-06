using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImportRenewals.Controllers
{
    public class RenewalsController:BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReadExcel(Byte[] file,string fileExtension)
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}