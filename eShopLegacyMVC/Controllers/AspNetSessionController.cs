using eShopLegacy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace eShopLegacyMVC.Controllers
{
    public class AspNetSessionController : Controller
    {
        // GET: AspNetCoreSession
        public IActionResult Index()
        {
            var modelJson = HttpContext.Session.GetString("DemoItem");
            var model = modelJson != null ? JsonConvert.DeserializeObject<SessionDemoModel>(modelJson) : null;
            return View(model);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SessionDemoModel demoModel)
        {
            HttpContext.Session.SetString("DemoItem", JsonConvert.SerializeObject(demoModel));
            return View(demoModel);
        }
    }
}