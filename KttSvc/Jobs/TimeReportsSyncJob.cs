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
   public class TimeReportsSyncJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            Logger.Instance.log.Info($"Start to Execute job {context.JobDetail}");
            //JobDataMap dataMap = context.JobDetail.JobDataMap;
            return Task.Factory.StartNew(SyncData);
        }

        public void SyncData()
        {
            Logger.Instance.log.Info($"Time Reports Synchronization Start");
            DateTime from;
            IList<TimeReportEntry> reports;
            try
            {
                from = KttComm.LastSynced();
            }
            catch (Exception ex)
            {
                Logger.Instance.log.Info($"failed to retreive last sync time, operation cancled", ex);
                return;
            }

            Logger.Instance.log.Info($"Connecting ATT clock...");

            AttComm att = new AttComm();

            try
            {
                att.Connect();
            }
            catch (Exception ex)
            {
                Logger.Instance.log.Error($"connection error to clock, operation cancled", ex);
                return;
            }

            Logger.Instance.log.Info($"retreiveing time reports from: { from.ToString("dd/MM/yyyy HH:mm:ss")}");

            try
            {
                reports = att.ReadTimeReports(from);
            }
            catch (Exception ex)
            {
                Logger.Instance.log.Error($"read time reports from clock failed", ex);
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
                    Logger.Instance.log.Warn($"failed to disconnect from clock", ex);
                }
            }

            

            if (reports.Count == 0)
            {
                Logger.Instance.log.Info($"no new reports since {from.ToString("dd/MM/yyyy HH:mm:ss")} found");
                return;
            }

            Logger.Instance.log.Info($"retrieved {reports.Count} from clock");

            StringBuilder reportsStr = new StringBuilder();
            for (int i = 0; i < reports.Count; i++)
            {
                TimeReportEntry item = reports[i];
                string mode = item.InOutMode ? "Out" : "In";
                reportsStr.AppendLine($"Record {i} EnrollNumber {item.EnrollNumber} TimeReport {item.TimeReport}  WorkCode {item.WorkCode} action {mode}");
            }

            Logger.Instance.log.Debug(reportsStr.ToString());

            Logger.Instance.log.Info($"updating KTT time reports");

            try
            {
                from = KttComm.UpdateReports(DateTime.Now, reports);
            }
            catch (Exception ex)
            {
                Logger.Instance.log.Error($"failed to update KTT time reports", ex);
            }
        }
    }
}
