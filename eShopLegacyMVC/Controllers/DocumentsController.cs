using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace eShopLegacyMVC.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;
        private readonly IConfiguration _configuration;

        public DocumentsController(IConfiguration configuration)
        {
            _contentTypeProvider = new FileExtensionContentTypeProvider();
            _configuration = configuration;
        }

        // GET: Files
        public ActionResult Index()
        {
            var files = FileService.Create(_configuration).ListFiles();
            return View(files);
        }

        [ResponseCache(VaryByQueryKeys = new[] { "filename" }, Duration = int.MaxValue)]
        public FileResult Download(string filename)
        {
            var fileService = FileService.Create(_configuration);
            var file = fileService.DownloadFile(filename);

            if (!_contentTypeProvider.TryGetContentType(filename, out string contentType))
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
            var fileService = FileService.Create(_configuration);
            fileService.UploadFile(Request.Form.Files);
            return RedirectToAction("Index");
        }
    }
}