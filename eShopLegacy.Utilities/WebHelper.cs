#if NETFRAMEWORK
using System.Web;
#else
using System;
#endif

namespace eShopLegacy.Utilities
{
    public class WebHelper
    {
#if NETFRAMEWORK
        public static string UserAgent => HttpContext.Current.Request.UserAgent;
#else
        // On .NET 10, System.Web.HttpContext.Current doesn't exist.
        // Call SetUserAgentAccessor() during app startup (M5) to wire this up.
        private static Func<string> _userAgentAccessor;

        public static void SetUserAgentAccessor(Func<string> accessor)
        {
            _userAgentAccessor = accessor;
        }

        public static string UserAgent => _userAgentAccessor?.Invoke() ?? string.Empty;
#endif
    }
}