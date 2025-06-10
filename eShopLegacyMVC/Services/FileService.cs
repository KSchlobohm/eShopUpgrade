using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace eShopLegacyMVC.Services
{
    public class FileService
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_NEWCREDENTIALS = 9;

        private readonly FileServiceConfiguration configuration;
        private readonly BlobContainerClient containerClient;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        public FileService(FileServiceConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            if (configuration.UseCredentialBasedAuth && !string.IsNullOrEmpty(configuration.BlobStorageUri))
            {
                // Use DefaultAzureCredential for authentication (managed identity, environment variables, etc.)
                var credential = new DefaultAzureCredential();
                var blobServiceClient = new BlobServiceClient(new Uri(configuration.BlobStorageUri), credential);
                containerClient = blobServiceClient.GetBlobContainerClient(configuration.BlobContainerName);
                
                // Create the container if it doesn't exist
                containerClient.CreateIfNotExists(PublicAccessType.None);
            }
            else if (!string.IsNullOrEmpty(configuration.BlobStorageConnectionString))
            {
                var blobServiceClient = new BlobServiceClient(configuration.BlobStorageConnectionString);
                containerClient = blobServiceClient.GetBlobContainerClient(configuration.BlobContainerName);
                
                // Create the container if it doesn't exist
                containerClient.CreateIfNotExists(PublicAccessType.None);
            }
        }        public static FileService Create() =>
            new FileService(new FileServiceConfiguration
            {
                BasePath = ConfigurationManager.AppSettings["Files:BasePath"],
                ServiceAccountUsername = ConfigurationManager.AppSettings["Files:ServiceAccountUsername"],
                ServiceAccountDomain = ConfigurationManager.AppSettings["Files:ServiceAccountDomain"],
                ServiceAccountPassword = ConfigurationManager.AppSettings["Files:ServiceAccountPassword"],
                BlobStorageConnectionString = ConfigurationManager.AppSettings["Files:BlobStorageConnectionString"],
                BlobContainerName = ConfigurationManager.AppSettings["Files:BlobContainerName"],
                BlobStorageUri = ConfigurationManager.AppSettings["Files:BlobStorageUri"],
                UseCredentialBasedAuth = ConfigurationManager.AppSettings["Files:UseCredentialBasedAuth"] == "true"
            });

        public IEnumerable<string> ListFiles()
        {
            if (containerClient != null)
            {
                var blobs = containerClient.GetBlobs();
                return blobs.Select(b => b.Name).ToList();
            }
            else
            {
                var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                    ? WindowsIdentity.GetCurrent().Token
                    : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

                using (var impersonationContext = WindowsIdentity.Impersonate(authToken))
                {
                    return Directory.GetFiles(configuration.BasePath).Select(Path.GetFileName);
                }
            }
        }

        public byte[] DownloadFile(string filename)
        {
            if (containerClient != null)
            {
                var blobClient = containerClient.GetBlobClient(filename);
                using (var memoryStream = new MemoryStream())
                {
                    blobClient.DownloadTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            else
            {
                var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                    ? WindowsIdentity.GetCurrent().Token
                    : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

                using (var impersonationContext = WindowsIdentity.Impersonate(authToken))
                {
                    var path = Path.Combine(configuration.BasePath, filename);
                    return File.ReadAllBytes(path);
                }
            }
        }

        public void UploadFile(HttpFileCollectionBase files)
        {
            if (containerClient != null)
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var filename = Path.GetFileName(file.FileName);
                    var blobClient = containerClient.GetBlobClient(filename);

                    using (var stream = file.InputStream)
                    {
                        blobClient.Upload(stream, true);
                    }
                }
            }
            else
            {
                var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                    ? WindowsIdentity.GetCurrent().Token
                    : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

                using (var impersonationContext = WindowsIdentity.Impersonate(authToken))
                {
                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var filename = Path.GetFileName(file.FileName);
                        var path = Path.Combine(configuration.BasePath, filename);

                        using (var fs = File.Create(path))
                        {
                            file.InputStream.CopyTo(fs);
                        }
                    }
                }
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