using eShopLegacyMVC.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;





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

        [ResponseCache(Duration = int.MaxValue, VaryByQueryKeys = new[] { "filename" })]
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

            // Convert IFormFileCollection to a format the legacy code can use
            var files = new List<HttpPostedFileBase>();
            foreach (var file in Request.Form.Files)
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                files.Add(new FormFileWrapper(file, memoryStream));
            }

            var fileCollection = new HttpFileCollectionWrapper(files);
            fileService.UploadFile(fileCollection);
            return RedirectToAction("Index");
        }

        // Wrapper class for HttpPostedFileBase
        private class FormFileWrapper : HttpPostedFileBase
        {
            private readonly IFormFile _file;
            private readonly MemoryStream _stream;

            public FormFileWrapper(IFormFile file, MemoryStream stream)
            {
                _file = file;
                _stream = stream;
            }

            public override string FileName => _file.FileName;
            public override string ContentType => _file.ContentType;
            public override int ContentLength => (int)_file.Length;
            public override Stream InputStream => _stream;
        }

        // Wrapper class for HttpFileCollectionBase
        private class HttpFileCollectionWrapper : HttpFileCollectionBase
        {
            private readonly List<HttpPostedFileBase> _files;

            public HttpFileCollectionWrapper(List<HttpPostedFileBase> files)
            {
                _files = files;
            }

            public override int Count => _files.Count;
            public override HttpPostedFileBase this[int index] => _files[index];
            public override HttpPostedFileBase this[string name] => _files.FirstOrDefault();
        }
    }
}