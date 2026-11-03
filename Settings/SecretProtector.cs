using System;
using System.Security.Cryptography;
using System.Text;

namespace Settings
{
    // Windows can encrypt for the current user. On macOS and Linux the value stays as it is, see PrivateSettingsFile.
    internal static class SecretProtector
    {
        public static string Protect(string secret)
        {
            if (secret.Length == 0 || !OperatingSystem.IsWindows())
            {
                return secret;
            }

            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(secret), null, DataProtectionScope.CurrentUser));
        }

        // Empty when it can't be decrypted, e.g. the file comes from another user or computer. It has to be entered again then.
        public static string Unprotect(string stored)
        {
            if (stored.Length == 0 || !OperatingSystem.IsWindows())
            {
                return stored;
            }

            try
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(stored), null, DataProtectionScope.CurrentUser));
            }
            catch (FormatException)
            {
                return "";
            }
            catch (CryptographicException)
            {
                return "";
            }
        }
    }
}
