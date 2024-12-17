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
            var model = HttpContext.Session.GetString("DemoItem");
            return View(model != null ? JsonSerializer.Deserialize<SessionDemoModel>(model) : null);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            HttpContext.Session.SetString("DemoItem", JsonSerializer.Serialize(demoModel));
            return View(demoModel);
        }
    }
}
