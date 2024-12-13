using eShopLegacyMVC.Models;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace eShopLegacyMVC.Services
{
    [SupportedOSPlatform("windows")]
    public class FileService
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_NEWCREDENTIALS = 9;

        private readonly FileSettings settings;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        public FileService(IOptions<FileSettings> options)

        {
            this.settings = options.Value;
            ArgumentException.ThrowIfNullOrEmpty(settings.BasePath, nameof(settings.BasePath));
        }

        public IEnumerable<string> ListFiles()
        {
            var authToken = string.IsNullOrEmpty(settings.ServiceAccountId)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(settings.ServiceAccountId, settings.ServiceAccountDomain, settings.ServiceAccountPassword);

            using var tokenHandle = new SafeAccessTokenHandle(authToken);
            return WindowsIdentity.RunImpersonated(tokenHandle, () =>
            {
                return Directory.GetFiles(settings.BasePath).Select(Path.GetFileName);
            });
        }

        public byte[] DownloadFile(string filename)
        {
            var authToken = string.IsNullOrEmpty(settings.ServiceAccountId) || string.IsNullOrEmpty(settings.ServiceAccountDomain) || string.IsNullOrEmpty(settings.ServiceAccountPassword)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(settings.ServiceAccountId, settings.ServiceAccountDomain, settings.ServiceAccountPassword);

            using var tokenHandle = new SafeAccessTokenHandle(authToken);
            return WindowsIdentity.RunImpersonated(tokenHandle, () =>
            {
                var path = Path.Combine(settings.BasePath, filename);
                return File.ReadAllBytes(path);
            });
        }


        public void UploadFile(IFormFileCollection files)
        {
            var authToken = string.IsNullOrEmpty(settings.ServiceAccountId)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(settings.ServiceAccountId, settings.ServiceAccountDomain, settings.ServiceAccountPassword);

            using var tokenHandle = new SafeAccessTokenHandle(authToken);
            WindowsIdentity.RunImpersonated(tokenHandle, () =>
            {

                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var filename = Path.GetFileName(file.FileName);
                    var path = Path.Combine(settings.BasePath, filename);

                    using (var fs = File.Create(path))
                    {
                        // TODO - Switch to CopyToAsync when upgrading to .NET 8
                        file.CopyTo(fs);
                    }
                }
            });
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