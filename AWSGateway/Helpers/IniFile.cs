using System.Text;
using System.Runtime.InteropServices;

namespace AWSGateway.Helpers
{
    public class IniFile
    {
        // Windows API pise rezultat u StringBuilder
        private const int MaxValueLength = 255;

        private string _path;

        // .NET nema .ini - kernel32.dll
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString
            (string section, string key, string value, string filePath);


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString
            (string section,string key, string defaultValue, StringBuilder returnVal, int size, string filePath);

        public IniFile(string iniPath)
        {
            _path = iniPath;
        }

        // cita vrijednost
        public string Read(string section, string key, string defaultValue = "")
        {
            StringBuilder result = new StringBuilder(MaxValueLength);

            GetPrivateProfileString(section, key, defaultValue, result, MaxValueLength, _path);

            return result.ToString();
        }

        // pise vrijednost
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _path);
        }

        public bool FileExists()
        {
            return File.Exists(_path);
        }
    }
}