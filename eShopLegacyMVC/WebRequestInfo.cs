using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace eShopLegacyMVC
{
    public class WebRequestInfo
    {
        public string Url { get; }
        public string UserAgent { get; }

        public WebRequestInfo(HttpContext context)
        {
            UserAgent = context.Request.Headers["User-Agent"];
            Url = context.Request.GetDisplayUrl();
        }

        public override string ToString() => "{Url}, {UserAgent}";
    }
}