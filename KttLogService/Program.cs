using KttLogService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KttLogService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {

                Logger.LoggerInstance.log.Info("Starting AttLogsService");
                //string windowsPath = Environment.GetEnvironmentVariable("windir");
                //windowsPath = windowsPath + "\\SysWOW64";
                //Process.Start(windowsPath + "\\regsvr32.exe ", windowsPath + "\\zkemkeeper.dll");
#if Debug       //   Properties.Settings.Default.Save();

                KttLogs t = new KttLogs();
                t.StartDebug();
                while (true)
                {
                    System.Threading.Thread.Sleep(10000);
                };
#endif


                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new KttLogs()
                };


                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
               
                Logger.LoggerInstance.log.Error($"Failed to start  AttLogsService due to Exception: {ex}");
            }
        }
    }
}
