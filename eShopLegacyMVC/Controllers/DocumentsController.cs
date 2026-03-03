using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace eShopLegacyMVC.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly FileService _fileService;

        public DocumentsController(FileService fileService)
        {
            _fileService = fileService;
        }

        // GET: Files
        public ActionResult Index()
        {
            var files = _fileService.ListFiles();
            return View(files);
        }

        [ResponseCache(Duration = int.MaxValue, VaryByQueryKeys = new[] { "filename" })]
        public FileResult Download(string filename)
        {
            var file = _fileService.DownloadFile(filename);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            FileContentResult fc = new FileContentResult(file, contentType);
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
            _fileService.UploadFile(Request.Form.Files);
            return RedirectToAction("Index");
        }
    }
}