using eShopLegacy.Utilities;
using eShopLegacyMVC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private ICatalogService _service;

        public BrandsController(ICatalogService service)
        {
            _service = service;
        }

        [HttpGet]

        // GET api/<controller>
        public IEnumerable<Models.CatalogBrand> Get()
        {
            var brands = _service.GetCatalogBrands();
            return brands;
        }

        [HttpGet("{id?}")]

        // GET api/<controller>/5
        public IActionResult Get(int id)
        {
            var brands = _service.GetCatalogBrands();
            var brand = brands.FirstOrDefault(x => x.Id == id);
            if (brand == null) return NotFound();

            return Ok(brand);
        }

        [HttpDelete("{id?}")]
        // DELETE api/<controller>/5
        public IActionResult Delete(int id)
        {
            var brandToDelete = _service.GetCatalogBrands().FirstOrDefault(x => x.Id == id);
            if (brandToDelete == null)
            {
                return NotFound();
            }

            // demo only - don't actually delete
            return Ok();
        }
    }
}