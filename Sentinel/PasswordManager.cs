using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sentinel
{
    /// <summary>
    /// Manages the application's administrator password using PBKDF2 hashing.
    /// Stores credentials in a separate JSON file (not the settings file) that works
    /// cross-platform on Windows, Linux, and macOS.
    /// </summary>
    public class PasswordManager
    {
        private static readonly string CredentialFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ASCOM\\Sentinel",
            "sentinel-credentials.json");

        private static readonly Lock _fileLock = new();
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;
        private const int MaxPasswordHistory = 10;
        private const string DefaultPassword = "Sentinel2026!";

        private CredentialData _data;

        /// <summary>
        /// Server-side auth token store. Tokens are invalidated on app restart,
        /// requiring users to re-authenticate.
        /// </summary>
        private static readonly ConcurrentDictionary<string, DateTime> _validTokens = new();

        public PasswordManager()
        {
            _data = Load();
        }

        /// <summary>
        /// Returns true if the stored password must be changed before the user can proceed.
        /// </summary>
        public bool MustChangePassword
        {
            get { lock (_fileLock) return _data.MustChangePassword; }
        }

        /// <summary>
        /// Verifies a plaintext password against the stored hash.
        /// </summary>
        public bool VerifyPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            lock (_fileLock)
            {
                return VerifyHash(password, _data.PasswordHash, _data.Salt);
            }
        }

        /// <summary>
        /// Sets a new administrator password. Validates complexity, checks history, and stores the hash.
        /// </summary>
        /// <returns>Null on success, or an error message describing the failure.</returns>
        public string? SetPassword(string newPassword)
        {
            ArgumentNullException.ThrowIfNull(newPassword);
            lock (_fileLock)
            {
                string? complexityError = ValidateComplexity(newPassword);
                if (complexityError is not null)
                    return complexityError;

                if (IsInHistory(newPassword))
                    return "This password has been used recently. Please choose a different password.";

                byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
                byte[] hash = ComputeHash(newPassword, salt);

                // Add current password to history before replacing
                if (_data.PasswordHash.Length > 0)
                {
                    _data.PasswordHistory.Add(new PasswordHistoryEntry
                    {
                        Hash = _data.PasswordHash,
                        Salt = _data.Salt
                    });

                    while (_data.PasswordHistory.Count > MaxPasswordHistory)
                        _data.PasswordHistory.RemoveAt(0);
                }

                _data.PasswordHash = Convert.ToBase64String(hash);
                _data.Salt = Convert.ToBase64String(salt);
                _data.MustChangePassword = false;

                Save();
                return null;
            }
        }

        /// <summary>
        /// Validates that a password meets the complexity requirements:
        /// at least 8 characters, with uppercase, lowercase, digits, and punctuation.
        /// </summary>
        /// <returns>Null if valid, or an error message describing the failure.</returns>
        public static string? ValidateComplexity(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Password cannot be empty.";

            if (password.Length < 8)
                return "Password must be at least 8 characters long.";

            if (!password.Any(char.IsUpper))
                return "Password must contain at least one uppercase letter.";

            if (!password.Any(char.IsLower))
                return "Password must contain at least one lowercase letter.";

            if (!password.Any(char.IsDigit))
                return "Password must contain at least one number.";

            if (!password.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
                return "Password must contain at least one punctuation character.";

            return null;
        }

        /// <summary>
        /// Creates a session token for an authenticated browser tab.
        /// </summary>
        public static string CreateAuthToken()
        {
            string token = Guid.NewGuid().ToString("N");
            _validTokens[token] = DateTime.UtcNow;
            return token;
        }

        /// <summary>
        /// Validates that a session token is currently active.
        /// </summary>
        public static bool ValidateAuthToken(string? token)
        {
            return !string.IsNullOrEmpty(token) && _validTokens.ContainsKey(token);
        }

        /// <summary>
        /// Removes a session token (logout).
        /// </summary>
        public static void RevokeAuthToken(string? token)
        {
            if (!string.IsNullOrEmpty(token))
                _validTokens.TryRemove(token, out _);
        }

        #region Private helpers

        private bool IsInHistory(string password)
        {
            if (_data.PasswordHash.Length > 0 && VerifyHash(password, _data.PasswordHash, _data.Salt))
                return true;

            foreach (var entry in _data.PasswordHistory)
            {
                if (VerifyHash(password, entry.Hash, entry.Salt))
                    return true;
            }

            return false;
        }

        private static bool VerifyHash(string password, string storedHash, string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            byte[] hash = ComputeHash(password, salt);
            return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(storedHash));
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);
        }

        private CredentialData Load()
        {
            try
            {
                if (File.Exists(CredentialFilePath))
                {
                    string json = File.ReadAllText(CredentialFilePath);
                    var data = JsonSerializer.Deserialize<CredentialData>(json);
                    if (data is not null && data.PasswordHash.Length > 0)
                        return data;
                }
            }
            catch
            {
                // Fall through to create default credentials
            }

            // No valid credential file — create one with the default password
            var defaultData = new CredentialData { MustChangePassword = true };
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = ComputeHash(DefaultPassword, salt);
            defaultData.PasswordHash = Convert.ToBase64String(hash);
            defaultData.Salt = Convert.ToBase64String(salt);

            _data = defaultData;
            Save();
            return defaultData;
        }

        private void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(CredentialFilePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CredentialFilePath, json);
            }
            catch
            {
                // Credential save failed — log to console as logger may not be available
                Console.Error.WriteLine($"Warning: Unable to save credentials to {CredentialFilePath}");
            }
        }

        #endregion

        #region Data classes

        private sealed class CredentialData
        {
            public string PasswordHash { get; set; } = "";
            public string Salt { get; set; } = "";
            public bool MustChangePassword { get; set; } = true;
            public List<PasswordHistoryEntry> PasswordHistory { get; set; } = [];
        }

        private sealed class PasswordHistoryEntry
        {
            public string Hash { get; set; } = "";
            public string Salt { get; set; } = "";
        }

        #endregion
    }
}
