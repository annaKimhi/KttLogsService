using KTT.Logging;
using KTT.Services;
using System;
using System.ServiceProcess;
using System.Threading;

namespace KTT
{
    static class Program
    {

        private static bool keepAlive;
        static Program()
        {
            keepAlive = true;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread()]
        static void Main()
        {
            Logger.Instance.log.Info("Starting AttLogsService");
            try
            {
                KttLogsSyncService service = new KttLogsSyncService();
                if (!Environment.UserInteractive)
                {
                
                    
                    ServiceBase.Run(service);
                }
                else
                {
                    service.ShuttingDown += OnShuttingDown;
                    service.Start(null);
                    while (keepAlive)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {

                Logger.Instance.log.Error($"Failed to start  AttLogsService due to Exception: {ex}");
            }
        }

        private static void OnShuttingDown()
        {
            keepAlive = false;
        }
    }
}
