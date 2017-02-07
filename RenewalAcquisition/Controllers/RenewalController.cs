using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (ModelState.IsValid)
            {
                try
                {
                    byte[] file = null;
                   
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase hpf = Request.Files[fileName];
                        if (hpf.ContentLength == 0)
                        {
                            continue;
                        }
                        using (var reader = new BinaryReader(hpf.InputStream))
                        {
                            file = reader.ReadBytes(hpf.ContentLength);
                        }                       
                        break;
                    }
                    bool async = Convert.ToBoolean(Request.Params["Async"].ToString());
                    string email = Request.Params["Email"].ToString();

                    importer.ReadFile(file, "USA", async);

                }
                catch (Exception e)
                {
                    return Json(new { Success = false, Message = e.Message, MessageType = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Success = resp.Success, Samples = resp.Sample, Message = resp.Message, MessageType = resp.TypeOfMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckFile()
        {
            ImportRenewals.Business.ImportCsv importer = new ImportRenewals.Business.ImportCsv();
            ImportRenewals.Models.Response resp = null;
            if (ModelState.IsValid)
            {
                try
                {
                    byte[] file = null;
                    String fileExtension = string.Empty;
                    String fileN = string.Empty;
                    //Get the file
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase hpf = Request.Files[fileName];
                        if (hpf.ContentLength == 0)
                        {
                            continue;
                        }
                        using (var reader = new BinaryReader(hpf.InputStream))
                        {
                            file = reader.ReadBytes(hpf.ContentLength);
                        }


                        fileExtension = Path.GetExtension(hpf.FileName);
                        fileN = hpf.FileName;
                        break;
                    }
                    resp = importer.CheckFile(file, fileN, fileExtension);

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