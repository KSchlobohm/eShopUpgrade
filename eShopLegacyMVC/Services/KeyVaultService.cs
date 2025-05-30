using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace eShopLegacyMVC.Services
{
    public interface IKeyVaultService
    {
        string GetSecret(string secretName);
        Task<string> GetSecretAsync(string secretName);
    }

    public class KeyVaultService : IKeyVaultService
    {
        private readonly SecretClient _secretClient;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        private readonly object _cacheLock = new object();

        public KeyVaultService()
        {
            var keyVaultUrl = ConfigurationManager.AppSettings["KeyVault:Url"];
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new InvalidOperationException("KeyVault:Url configuration is required");
            }

            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        public string GetSecret(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
            }

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(secretName, out var cachedValue))
                {
                    return cachedValue;
                }
            }

            try
            {
                var response = _secretClient.GetSecret(secretName);
                var secretValue = response.Value.Value;

                lock (_cacheLock)
                {
                    _cache[secretName] = secretValue;
                }

                return secretValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from Key Vault", ex);
            }
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
            }

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(secretName, out var cachedValue))
                {
                    return cachedValue;
                }
            }

            try
            {
                var response = await _secretClient.GetSecretAsync(secretName);
                var secretValue = response.Value.Value;

                lock (_cacheLock)
                {
                    _cache[secretName] = secretValue;
                }

                return secretValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from Key Vault", ex);
            }
        }
    }
}
