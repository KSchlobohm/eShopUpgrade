using eShopLegacy.Utilities;
using eShopLegacyMVC.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;


namespace eShopLegacyMVC.Controllers.WebApi
{
[ApiController]    public class FilesController :ControllerBase    {
        private ICatalogService _service;

        public FilesController(ICatalogService service)
        {
            _service = service;
        }

        // GET api/<controller>
        public HttpResponseMessage Get()
        {
            var brands = _service.GetCatalogBrands()
                .Select(b => new BrandDTO
                {
                    Id = b.Id,
                    Brand = b.Brand
                }).ToList();
            var serializer = new Serializing();
            var serializedData = serializer.SerializeBinary(brands);
            var byteArray = Encoding.UTF8.GetBytes(serializedData);
            var memoryStream = new MemoryStream(byteArray);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memoryStream)
            };

            return response;
        }

        [Serializable]
        public class BrandDTO
        {
            public int Id { get; set; }
            public string Brand { get; set; }
        }
    }
}