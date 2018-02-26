using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KttLogService
{
    static class ApplicationSettings
    {
        public static DateTime LastSync;
        public static string Command;
        public static string AttIp;
        public static string AttPort;
        public static string KttUri;
        private static Configuration _config;
        static ApplicationSettings()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            DateTime.TryParse(_config.AppSettings.Settings["lastSync"].Value, out LastSync);
            Command = _config.AppSettings.Settings["Command"].Value; 
            AttIp = _config.AppSettings.Settings["attip"].Value; 
            AttPort = _config.AppSettings.Settings["attport"].Value;
            KttUri = _config.AppSettings.Settings["ktturi"].Value;
            //_config.AppSettings.Settings["ktturi"].Value = "http://www.korentec.co.il/kttdebug1";
            //_config.Save(ConfigurationSaveMode.Modified);
        }

        public static void Save(string key, string value)
        {
            _config.AppSettings.Settings[key].Value = value;
            _config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
