using KttLogService;
using System;
using System.Collections.Generic;
using System.Linq;
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

                //string windowsPath = Environment.GetEnvironmentVariable("windir");
                //windowsPath = windowsPath + "\\SysWOW64";


                //Process.Start(windowsPath + "\\regsvr32.exe ", windowsPath + "\\zkemkeeper.dll");
                //
                //KttLogs t = new KttLogs();
                //t.OnStart(null);

                //System.Configuration.Install.AssemblyInstaller Installer = new System.Configuration.Install.AssemblyInstaller("",null);
                //Installer.UseNewContext = true;
                //Installer.Install(null);
                //Installer.Commit(null);

                Logger.LoggerInstance.log.Info("Starting AttLogsService");
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new KttLogs()
                };

                //while (true)
                //{
                //    System.Threading.Thread.Sleep(10000);
                //};
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.log.Error($"Failed to start  AttLogsService due to Exception: {ex}");
            }
        }
    }
}
