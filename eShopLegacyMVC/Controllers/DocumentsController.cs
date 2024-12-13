using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Runtime.Versioning;

namespace eShopLegacyMVC.Controllers
{
    [SupportedOSPlatform("windows")]
    public class DocumentsController : Controller
    {
        private readonly FileService fileService;
        private readonly FileExtensionContentTypeProvider contentTypeProvider;

        public DocumentsController(FileService fileService)
        {
            this.fileService = fileService;
            this.contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        // GET: Files
        public ActionResult Index()
        {
            var files = fileService.ListFiles();
            return View(files);
        }

        [ResponseCache(VaryByQueryKeys = ["filename"], Duration = int.MaxValue)]
        public FileResult Download(string filename)
        {
            var file = fileService.DownloadFile(filename);
            var fc = new FileContentResult(file, contentTypeProvider.TryGetContentType(filename, out var contentType) ? contentType : "application/octet-stream");
            fc.FileDownloadName = filename;
            return fc;
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadDocument()
        {
            fileService.UploadFile(Request.Form.Files);
            return RedirectToAction("Index");
        }
    }
}