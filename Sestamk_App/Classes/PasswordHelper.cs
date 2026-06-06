using System.Security.Cryptography;

namespace Sestamk.Classes
{
    /// <summary>
    /// PBKDF2-based password hashing with per-user salt.
    /// Format: "{iterations}.{base64_salt}.{base64_hash}"
    /// Replaces the old SHA256-without-salt implementation.
    /// </summary>
    public static class PasswordHelper
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        /// <summary>
        /// Hash a password with a random salt using PBKDF2-SHA256.
        /// Returns a self-contained string: "100000.{salt}.{hash}"
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verify a password against a stored hash.
        /// Supports BOTH new PBKDF2 format AND legacy SHA256 (for migration).
        /// Uses constant-time comparison to prevent timing attacks.
        /// </summary>
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            // New PBKDF2 format: "100000.{salt}.{hash}"
            string[] parts = storedHash.Split('.');
            if (parts.Length == 3 && int.TryParse(parts[0], out int iterations))
            {
                try
                {
                    byte[] salt = Convert.FromBase64String(parts[1]);
                    byte[] expectedHash = Convert.FromBase64String(parts[2]);
                    byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                        enteredPassword,
                        salt,
                        iterations,
                        HashAlgorithmName.SHA256,
                        expectedHash.Length);

                    return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
                }
                catch
                {
                    return false;
                }
            }

            // Legacy: 64-char hex = old SHA256 hash (no salt)
            if (storedHash.Length == 64 && IsHexString(storedHash))
            {
                string legacyHash = LegacySha256(enteredPassword);
                return storedHash.Equals(legacyHash, StringComparison.OrdinalIgnoreCase);
            }

            // Fallback: plaintext comparison (for users not yet migrated)
            return storedHash == enteredPassword;
        }

        /// <summary>
        /// Check if a stored hash is in the old format and needs migration.
        /// Call this after successful login to auto-upgrade.
        /// </summary>
        public static bool NeedsMigration(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return true;

            string[] parts = storedHash.Split('.');
            if (parts.Length == 3 && int.TryParse(parts[0], out _))
                return false; // Already PBKDF2

            return true; // SHA256 hex or plaintext
        }

        private static string LegacySha256(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static bool IsHexString(string s)
        {
            foreach (char c in s)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            return true;
        }
    }
}
