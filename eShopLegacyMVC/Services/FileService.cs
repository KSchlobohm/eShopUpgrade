using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Win32.SafeHandles;

namespace eShopLegacyMVC.Services
{
    [SupportedOSPlatform("windows")]
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

        public IEnumerable<string> ListFiles()
        {
            return RunImpersonated(() =>
                Directory.GetFiles(configuration.BasePath).Select(Path.GetFileName).ToList()
            );
        }

        public byte[] DownloadFile(string filename)
        {
            return RunImpersonated(() =>
            {
                var path = Path.Combine(configuration.BasePath, filename);
                return File.ReadAllBytes(path);
            });
        }

        public void UploadFile(IFormFileCollection files)
        {
            RunImpersonated(() =>
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var filename = Path.GetFileName(file.FileName);
                    var path = Path.Combine(configuration.BasePath, filename);

                    using (var fs = File.Create(path))
                    {
                        file.CopyTo(fs);
                    }
                }
            });
        }

        private T RunImpersonated<T>(Func<T> action)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().AccessToken
                : new SafeAccessTokenHandle(GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword));

            return WindowsIdentity.RunImpersonated(authToken, action);
        }

        private void RunImpersonated(Action action)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().AccessToken
                : new SafeAccessTokenHandle(GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword));

            WindowsIdentity.RunImpersonated(authToken, action);
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