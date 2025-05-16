using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;

namespace eShopLegacyMVC.Services
{
    public class FileService
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_NEWCREDENTIALS = 9;

        private readonly FileServiceConfiguration configuration;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        public FileService(FileServiceConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static FileService Create()
        {
// Get configuration from appsettings.json using Microsoft.Extensions.Configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return new FileService(new FileServiceConfiguration
            {
                BasePath = configuration["Files:BasePath"],
                ServiceAccountUsername = configuration["Files:ServiceAccountUsername"],
                ServiceAccountDomain = configuration["Files:ServiceAccountDomain"],
                ServiceAccountPassword = configuration["Files:ServiceAccountPassword"]
            });
        }

        public IEnumerable<string> ListFiles()
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

using (var safeTokenHandle = new SafeAccessTokenHandle(authToken))
            {
                return WindowsIdentity.RunImpersonated(safeTokenHandle, () =>
                {
                    return Directory.GetFiles(configuration.BasePath).Select(Path.GetFileName);
                });
            }
        }

        public byte[] DownloadFile(string filename)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

using (var safeTokenHandle = new SafeAccessTokenHandle(authToken))
            {
                return WindowsIdentity.RunImpersonated(safeTokenHandle, () =>
                {
                    var path = Path.Combine(configuration.BasePath, filename);
                    return File.ReadAllBytes(path);
                });
            }
        }

        public void UploadFile(HttpFileCollectionBase files)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

using (var safeTokenHandle = new SafeAccessTokenHandle(authToken))
            {
                WindowsIdentity.RunImpersonated(safeTokenHandle, () =>
                {
                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var filename = Path.GetFileName(file.FileName);
                        var path = Path.Combine(configuration.BasePath, filename);

using (var fs = File.Create(path))
                        {
                            // TODO - Switch to CopyToAsync when upgrading to .NET 8
                            file.InputStream.CopyTo(fs);
                        }
                    }
                });
            }
        }

        private IntPtr GetAuthToken(string username, string domain, string password)
        {
            if (!LogonUser(username, domain, password, LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, out IntPtr authToken))
            {
                throw new InvalidOperationException($"Unable to get auth token for service account {username} in domain {domain}");
            }

            return authToken;
        }
    }
}