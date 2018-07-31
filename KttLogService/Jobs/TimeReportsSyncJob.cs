using KTT.Comm;
using KTT.Comm.Data;
using KTT.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KTT.Jobs
{
   internal class TimeReportsSyncJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            Logger .LoggerInstance.log.Info($"Start to Execute job {context.JobDetail}");
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            return Task.Factory.StartNew((v) => { SyncData((JobDataMap)v); }, dataMap);
        }

        //Download the attendance records from the device(For both Black&White and TFT screen devices).
        private void SyncData(JobDataMap args)
        {
            Logger.LoggerInstance.log.Info($"Time Reports Synchronization Start");
            DateTime from;
            IList<TimeReportEntry> reports;
            try
            {
                from = KttComm.LastSynced();
            }
            catch(Exception ex)
            {
                Logger.LoggerInstance.log.Info($"failed to retreive last sync time, operation cancled", ex);
                return;
            }
            
            Logger.LoggerInstance.log.Info($"Connecting ATT clock...");

            AttComm att = new AttComm();
            
            try
            {
                att.Connect();
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.log.Error($"connection error to clock, operation cancled", ex);
                return;
            }

            Logger.LoggerInstance.log.Info($"retreiveing time reports from: { from.ToString("dd/MM/yyyy HH:mm:ss")}");

            try
            {
                reports = att.ReadTimeReports(from);
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.log.Error($"read time reports from clock failed", ex);
                return;
            }
            finally
            {
                try
                {
                    att.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger.LoggerInstance.log.Warn($"failed to disconnect from clock", ex);
                }
            }


            Logger.LoggerInstance.log.Info($"retrieved {reports.Count} from clock");
            StringBuilder reportsStr = new StringBuilder();
            for (int i = 0; i < reports.Count; i++)
            {
                TimeReportEntry item = reports[i];
                string mode = item.InOutMode ? "Out" : "In";
                reportsStr.AppendLine($"Record {i} EnrollNumber {item.EnrollNumber} TimeReport {item.TimeReport}  WorkCode {item.WorkCode} action {mode}");
            }

            Logger.LoggerInstance.log.Debug(reportsStr.ToString());

            Logger.LoggerInstance.log.Info($"updating KTT time reports");

            try
            {
                from = KttComm.UpdateReports(DateTime.Now, reports);
            }
            catch(Exception ex)
            {
                Logger.LoggerInstance.log.Error($"failed to update KTT time reports", ex);
            }
        }
    }
}
