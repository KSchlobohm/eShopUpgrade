using eShopLegacy.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace eShopLegacyMVC.Controllers
{
    public class AspNetSessionController : Controller
    {
        // GET: AspNetCoreSession
        public ActionResult Index()
        {
            var modelJson = HttpContext.Session.GetString("DemoItem");
            var model = string.IsNullOrWhiteSpace(modelJson)
                ? new SessionDemoModel()
                : JsonSerializer.Deserialize<SessionDemoModel>(modelJson);

            return View(model);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            var modelJson = JsonSerializer.Serialize(demoModel);
            HttpContext.Session.SetString("DemoItem", modelJson);
            return View(demoModel);
        }
    }
}