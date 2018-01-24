using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KttLogService
{
    public class kttJob : IJob
    {
        private static DateTime _from = new DateTime();
        private static string _kttUri;
        private static string _attServerIp ;
        private static int _attServerPort ;
        private string _action;

        void IJob.Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _action = dataMap.GetString("Action");
            _kttUri = dataMap.GetString("Uri");
             DateTime.TryParse(dataMap.GetString("From"), out _from);           
            _attServerIp = dataMap.GetString("AttServerIp");            
            _attServerPort = int.Parse(dataMap.GetString("AttServerPort"));

            GetAttLogs();
        }


        private void GetAttLogs()
        {
            if (_action == "sync")
            {
                Sync();

            }
        }

        private void Sync()
        {
            AttDataHandler dataHandler = new AttDataHandler(_kttUri, _attServerIp, _attServerPort);
            string lastSync = null;
            try
            {
                Logger.LoggerInstance.log.Info($"Syncronize ATT clock reports with KTT server from { _from.ToString()}");

                Logger.LoggerInstance.log.Info($"Last Sync timestamp from server:  { (!String.IsNullOrEmpty(Properties.Settings.Default.lastSync) ? Properties.Settings.Default.lastSync : "UNKNOWN")}");

                Logger.LoggerInstance.log.Info($"Connecting ATT clock...");

                dataHandler.ConnectAttMachine();


                Logger.LoggerInstance.log.Info($"Sending data to KTT server...");
                Logger.LoggerInstance.log.Info($"Server URI: " + _kttUri);
                Logger.LoggerInstance.log.Info($"clock IP: {_attServerIp }, {_attServerPort.ToString()}");

                int res;
                dataHandler.SyncData(_from, out lastSync, out res);
                Logger.LoggerInstance.log.Info(res.ToString() + " records copied");
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.log.Error($"Failed to Sync()   due to Exception: {ex}");
            }
            finally
            {
                if (lastSync != null)
                {
                    Properties.Settings.Default.lastSync = lastSync;
                    Properties.Settings.Default.Save();
                }
                Logger.LoggerInstance.log.Info($"Disconnecting ATT clock...");

                dataHandler.DisconnectAttMachine();
                Logger.LoggerInstance.log.Info($"Process Done!");

            }
        }
    }
}
