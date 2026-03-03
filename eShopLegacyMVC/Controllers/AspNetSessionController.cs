using eShopLegacy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace eShopLegacyMVC.Controllers
{
    public class AspNetSessionController : Controller
    {
        // GET: AspNetCoreSession
        public ActionResult Index()
        {
            var json = HttpContext.Session.GetString("DemoItem");
            var model = json != null ? JsonSerializer.Deserialize<SessionDemoModel>(json) : null;
            return View(model);
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
