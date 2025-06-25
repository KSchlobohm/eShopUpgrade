using eShopLegacyMVC.Services;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace eShopLegacyMVC.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly FileService fileService;

        public DocumentsController(FileService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out var contentType))
            {
                contentType = "application/octet-stream"; // Fallback for unknown types
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
        public async Task<ActionResult> UploadDocument(List<IFormFile> files)
        {
            await fileService.UploadFileAsync(files);
            return RedirectToAction("Index");
        }
    }
}