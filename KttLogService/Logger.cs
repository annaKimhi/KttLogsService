using KttLogService;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KttLogService
{
    internal class Logger
    {
        private static Logger _LoggerInstance = new Logger();
        public ILog log;
        private Logger()
        {

            XmlConfigurator.ConfigureAndWatch(new FileInfo("KttLogService.exe.config"));
            log = LogManager.GetLogger(typeof(KttLogs));

        }
        public static Logger LoggerInstance
        {
            get
            {

                return _LoggerInstance;
            }
        }


    }
}
