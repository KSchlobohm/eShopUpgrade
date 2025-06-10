namespace eShopLegacyMVC.Services
{
    public class FileServiceConfiguration
    {
        public string BasePath { get; set; }
        public string ServiceAccountDomain { get; set; }
        public string ServiceAccountUsername { get; set; }
        public string ServiceAccountPassword { get; set; }
        public string BlobStorageConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        public string BlobStorageUri { get; set; }
        public bool UseCredentialBasedAuth { get; set; }
    }
}