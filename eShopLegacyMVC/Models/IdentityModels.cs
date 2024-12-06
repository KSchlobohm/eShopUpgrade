using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace eShopLegacyMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
// Generate a new ClaimsIdentity using the user's claims
            var claims = await manager.GetClaimsAsync(this);
            var userIdentity = new ClaimsIdentity(claims, "ApplicationCookie");
            // Add custom user claims here if needed
            return userIdentity;
        }

        private int? _zipCode = null;

        public int? ZipCode
        {
            get
            {
                if (_zipCode is null)
                {
                    var uri = string.Format("http://10.0.0.42/UserLookup.svc/zipCode?id={0}", Id);
                    var req = HttpWebRequest.Create(uri) as HttpWebRequest;
                    req.Method = "GET";
                    req.ServicePoint.Expect100Continue = false;

                    var response = req.GetResponse();
                    var responseStream = response.GetResponseStream();
using (var reader = new StreamReader(responseStream))
                    {
                        var zipCode = reader.ReadToEnd();
                        _zipCode = int.Parse(zipCode);
                    }
                }
                return _zipCode;
            }
        }
    }
}