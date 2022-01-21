using GoogleDriveAPI_v03.Models;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace GoogleDriveAPI_v03.Controllers
{
    public class RESTController : Controller
    {
        [HttpGet]
        public ActionResult GetGoogleDriveFiles()
        {
            return View(FilesDriveRepository.GetDriveFiles());
        }

        [HttpPost]
        public ActionResult DeleteFile(FilesFromDrive file)
        {
            FilesDriveRepository.DeleteFile(file);
            return RedirectToAction("GetGoogleDriveFiles");
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            FilesDriveRepository.FileUpload(file);
            return RedirectToAction("GetGoogleDriveFiles");
        }

        public void DownloadFile(string id)
        {
            string FilePath = FilesDriveRepository.DownloadGoogleFile(id);

            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(FilePath)));
            Response.End();
            Response.Flush();
        }
    }
}