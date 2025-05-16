using eShopLegacyMVC.Services;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace eShopLegacyMVC.Controllers
{
    public class DocumentsController : Controller
    {
        // GET: Files
        public ActionResult Index()
        {
            var files = FileService.Create().ListFiles();
            return View(files);
        }

        [ResponseCache(VaryByQueryKeys = new[] { "filename" }, Duration = int.MaxValue)]
        public FileResult Download(string filename)
        {
            var fileService = FileService.Create();
            var file = fileService.DownloadFile(filename);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out string contentType))
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
        public async Task<ActionResult> UploadDocument()
        {
            var fileService = FileService.Create();

            // Create a wrapper or adapter to convert IFormFileCollection to what FileService expects
            var files = Request.Form.Files;
            var httpFileCollectionBase = new HttpFileCollectionWrapper(files);

            fileService.UploadFile(httpFileCollectionBase);
            return RedirectToAction("Index");
        }

        // Wrapper class to adapt IFormFileCollection to HttpFileCollectionBase
        private class HttpFileCollectionWrapper : HttpFileCollectionBase
        {
            private readonly IFormFileCollection _files;

            public HttpFileCollectionWrapper(IFormFileCollection files)
            {
                _files = files;
            }

            public override int Count => _files.Count;

            public override HttpPostedFileBase Get(string name)
            {
                var file = _files.FirstOrDefault(f => f.Name == name);
                return file != null ? new HttpPostedFileWrapper(file) : null;
            }

            public override HttpPostedFileBase this[string name] => Get(name);

            public override HttpPostedFileBase this[int index] =>
                index < _files.Count ? new HttpPostedFileWrapper(_files[index]) : null;

            public override string[] AllKeys => _files.Select(f => f.Name).ToArray();
        }

        // Wrapper class to adapt IFormFile to HttpPostedFileBase
        private class HttpPostedFileWrapper : HttpPostedFileBase
        {
            private readonly IFormFile _file;

            public HttpPostedFileWrapper(IFormFile file)
            {
                _file = file;
            }

            public override int ContentLength => (int)_file.Length;

            public override string ContentType => _file.ContentType;

            public override string FileName => Path.GetFileName(_file.FileName);

            public override Stream InputStream => _file.OpenReadStream();

            public override void SaveAs(string filename)
            {
using (var stream = new FileStream(filename, FileMode.Create))
                {
                    _file.CopyTo(stream);
                }
            }
        }
    }
}