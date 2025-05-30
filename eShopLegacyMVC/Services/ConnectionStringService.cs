using System;
using System.Configuration;

namespace eShopLegacyMVC.Services
{
    public interface IConnectionStringService
    {
        string GetConnectionString(string name);
    }

    public class ConnectionStringService : IConnectionStringService
    {
        private readonly IKeyVaultService _keyVaultService;

        public ConnectionStringService(IKeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService;
        }

        public string GetConnectionString(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Connection string name cannot be null or empty", nameof(name));
            }

            // First try to get from configuration (backward compatibility)
            var connectionString = ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            // If not found in config and Key Vault service is available, get from Key Vault
            if (_keyVaultService != null)
            {
                try
                {
                    var secretName = $"connectionstring-{name.ToLowerInvariant()}";
                    return _keyVaultService.GetSecret(secretName);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to retrieve connection string '{name}' from Key Vault", ex);
                }
            }

            throw new InvalidOperationException($"Connection string '{name}' not found in configuration or Key Vault");
        }

        public static ConnectionStringService Create()
        {
            return new ConnectionStringService(new KeyVaultService());
        }
    }
}
