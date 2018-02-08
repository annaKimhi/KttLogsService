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
        public static DateTime lastSync;
        public static string Command;
        public static string attip;
        public static string attport;
        public static string ktturi;
        private static Configuration _config;
        static ApplicationSettings()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            lastSync = DateTime.Parse(_config.AppSettings.Settings["lastSync"].Value);
            Command = _config.AppSettings.Settings["Command"].Value; 
            attip = _config.AppSettings.Settings["attip"].Value; 
            attport = _config.AppSettings.Settings["attport"].Value;
            ktturi = _config.AppSettings.Settings["ktturi"].Value;
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
