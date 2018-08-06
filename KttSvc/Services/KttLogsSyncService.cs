using KTT.Jobs;
using KTT.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace KTT.Services
{

    public delegate void ShutDownEventHandler();

    class KttLogsSyncService : ServiceBase
    {
        public event ShutDownEventHandler ShuttingDown;

        public KttLogsSyncService()
        {
            this.ServiceName = "KTT Synchronizer";
        }

        public void Start(string[] args)
        {
            this.OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Instance.log.Info("Parsing  args onStart AttLogsService");
                BuildJob();
            }
            catch (Exception ex)
            {
                Logger.Instance.log.Error($"Failed to parse args due to Exception: {ex}");
            }

        }

        private void BuildJob()
        {
            StdSchedulerFactory schedFact = new StdSchedulerFactory();
            Task<IScheduler> sched = schedFact.GetScheduler();
            sched.Result.Start();
            IJobDetail immediateTimeReportsSyncJob = JobBuilder.Create<TimeReportsSyncJob>()
                                                .WithIdentity("immediateTimeReportsSync", "timeReportsSync")
                                                .Build();

            ITrigger immediateTimeReportsSyncTrigger = TriggerBuilder.Create()
                                                      .WithIdentity("immediateTimeReportsSync", "timeReportsSync")
                                                      .StartNow()
                                                      .Build();

            IJobDetail dailyTimeReportsSyncJob = JobBuilder.Create<TimeReportsSyncJob>()
                                            .WithIdentity("dailyTimeReportsSync", "timeReportsSync")
                                            .Build();


            ITrigger dailyTimeReportsSyncTrigger = TriggerBuilder.Create()
                                                  .WithIdentity("dailyTimeReportsSync", "timeReportsSync")
                                                  .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(23, 58, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday))
                                                  .Build();

            sched.Result.ScheduleJob(immediateTimeReportsSyncJob, immediateTimeReportsSyncTrigger).ContinueWith(t => sched.Result.ScheduleJob(dailyTimeReportsSyncJob, dailyTimeReportsSyncTrigger));
        }


        protected override void OnStop()
        {
            
        }

        protected override void OnShutdown()
        {
            ShuttingDown?.Invoke();
        }
    }
}
