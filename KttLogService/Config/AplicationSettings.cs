using System;
using System.Configuration;

namespace KTT.Config
{
    static class ApplicationSettings
    {
        private static Configuration _config;
        static ApplicationSettings()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
                
        public static string ATT_IP
        {
            get { return ((SyncConfig)_config.GetSection("syncconfig")).ATT_IP; }
            set { ((SyncConfig)_config.GetSection("syncconfig")).ATT_IP = value; }
        }

        public static int ATT_Port
        {
            get { return ((SyncConfig)_config.GetSection("syncconfig")).ATT_Port; }
            set { ((SyncConfig)_config.GetSection("syncconfig")).ATT_Port = value; }
        }

        public static Uri KTT_URI
        {
            get { return ((SyncConfig)_config.GetSection("syncconfig")).KTT_URI; }
            set { ((SyncConfig)_config.GetSection("syncconfig")).KTT_URI = value; }
        }

        public static void Save()
        {
            //_config.AppSettings.Settings[key].Value = value;
            _config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
