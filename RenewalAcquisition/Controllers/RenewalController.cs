using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace RenewalAcquisition.Controllers
{
    public class RenewalController: Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReadExcel()
        {
            ImportRenewals.Business.ImportCsv importer = new ImportRenewals.Business.ImportCsv();
            ImportRenewals.Models.Response resp = null;

            string vendor = "Cisco";
            string region = "USA";

            if (ModelState.IsValid)
            {
                try
                {
                    System.Configuration.AppSettingsReader appReader = new System.Configuration.AppSettingsReader();
                    string filePath = (string)appReader.GetValue("TempPath", typeof(string));
                    filePath += vendor + ".csv";//Fixo por enquanto, refatorar
                    bool async = Convert.ToBoolean(Request.Params["Async"].ToString());
                    string email = Request.Params["Email"].ToString();

                    resp = importer.ReadFile(filePath, "USA", async,email);

                }
                catch (Exception e)
                {
                    return Json(new { Success = false, Message = e.Message, MessageType = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Success = resp.Success, Message = resp.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckFile()
        {
            string vendor = "Cisco";
            string region = "USA";

            ImportRenewals.Business.ImportCsv importer = new ImportRenewals.Business.ImportCsv();
            ImportRenewals.Models.Response resp = null;
            if (ModelState.IsValid)
            {
                try
                {
                    string filePath = null;
                    //Get the file
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase hpf = Request.Files[fileName];
                        if (hpf.ContentLength == 0)
                        {
                            continue;
                        }

                        filePath = Util.CopyFile(hpf, vendor);
                        
                        break;
                    }
                    resp = importer.CheckFile(filePath);

                }
                catch (Exception e)
                {
                    return Json(new { Success = false, Message = e.Message, MessageType = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Success = resp.Success, Samples = resp.Sample, Message = resp.Message, MessageType = resp.TypeOfMessage }, JsonRequestBehavior.AllowGet);

        }
    }
}