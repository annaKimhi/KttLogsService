using log4net;
using log4net.Config;
using System.Configuration;
using System.IO;

namespace KTT.Logging
{
    public class Logger
    {
        private static readonly Logger _instance;

        static Logger()
        {
            _instance = new Logger();
        }

        public readonly ILog log;
       
        private Logger()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath));
            log = LogManager.GetLogger("KTT Logs");
        }
        public static Logger Instance { get { return Logger._instance; } }
    }
}
