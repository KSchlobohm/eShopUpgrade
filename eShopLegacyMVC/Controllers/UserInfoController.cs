using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;


namespace eShopLegacyMVC.Controllers
{
    public class UserInfoController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}