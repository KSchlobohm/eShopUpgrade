using eShopLegacy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace eShopLegacyMVC.Controllers
{
    public class AspNetSessionController : Controller
    {
        // GET: AspNetCoreSession
        public ActionResult Index()
        {
            SessionDemoModel model = null;
            var serializedModel = HttpContext.Session.GetString("DemoItem");
            if (!string.IsNullOrEmpty(serializedModel))
            {
                model = JsonConvert.DeserializeObject<SessionDemoModel>(serializedModel);
            }
            return View(model);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            HttpContext.Session.SetString("DemoItem", JsonConvert.SerializeObject(demoModel));
            return View(demoModel);
        }
    }
}