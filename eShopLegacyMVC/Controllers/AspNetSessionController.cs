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
            string sessionData = HttpContext.Session.GetString("DemoItem");
            if (!string.IsNullOrEmpty(sessionData))
            {
                model = JsonConvert.DeserializeObject<SessionDemoModel>(sessionData);
            }
            return View(model);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            string sessionData = JsonConvert.SerializeObject(demoModel);
            HttpContext.Session.SetString("DemoItem", sessionData);
            return View(demoModel);
        }
    }
}