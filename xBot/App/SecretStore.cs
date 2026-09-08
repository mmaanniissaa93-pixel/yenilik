using System;
using System.Security.Cryptography;
using System.Text;

namespace xBot.App
{
    /// <summary>
    /// Protects persisted secrets for the current Windows user.
    /// Legacy plaintext values are accepted on read and encrypted on the next save.
    /// </summary>
    public static class SecretStore
    {
        private const string Prefix = "enc:";

        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] clearBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] protectedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(protectedBytes);
        }

        public static string Unprotect(string storedValue)
        {
            if (string.IsNullOrEmpty(storedValue))
                return string.Empty;

            if (!storedValue.StartsWith(Prefix, StringComparison.Ordinal))
                return storedValue;

            try
            {
                byte[] protectedBytes = Convert.FromBase64String(storedValue.Substring(Prefix.Length));
                byte[] clearBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(clearBytes);
            }
            catch (CryptographicException)
            {
                return string.Empty;
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }
    }
}
