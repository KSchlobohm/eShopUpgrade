using Microsoft.AspNetCore.Http;

namespace eShopLegacy.Utilities
{
    public class WebHelper
    {
        public static string GetUserAgent(HttpContext context) => context.Request.Headers["User-Agent"];
    }
}