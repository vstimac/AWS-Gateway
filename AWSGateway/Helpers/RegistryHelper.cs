using Microsoft.Win32;
using System.Diagnostics;

namespace AWSGateway.Helpers
{
    // HKEY_CURRENT_USER\SOFTWARE\AWSGateway

    public static class RegistryHelper
    {
        private const string RegistryPath = @"SOFTWARE\AWSGateway";

        // pise tekstualnu vrijednost
        public static void WriteString(string key, string value)
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (regKey != null)
                    {
                        regKey.SetValue(key, value, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod pisanja u registar: " + ex.Message);
            }
        }

        // parsirati zbog teksta
        public static int ReadInt(string key, int defaultValue = 0)
        {
            string stringValue = ReadString(key, defaultValue.ToString());
            int result;

            if (int.TryParse(stringValue, out result))
            {
                return result;
            }

            return defaultValue;
        }

        // cita tekstualnu vrijednost
        public static string ReadString(string key, string defaultValue = "")
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (regKey != null)
                    {
                        object value = regKey.GetValue(key);

                        if (value != null)
                        {
                            return value.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod čitanja iz registra: " + ex.Message);
            }

            return defaultValue;
        }
    }
}