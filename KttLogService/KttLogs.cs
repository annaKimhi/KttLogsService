using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KttLogService
{
    partial class KttLogs : ServiceBase
    {

        const string SYNC_ACTION = "sync";
        private const string CLEAN_ACTION = "clean";
        private static string _action = null;
        private static DateTime _from = new DateTime();
        private static string _kttUri;

        private static string _attServerIp;//= "192.168.1.7";
        private static int _attServerPort;// = 4370;
        private readonly string kttLogsJob = "kttLogsJob";

        public KttLogs()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                Logger.LoggerInstance.log.Info("Parsing  args onStart AttLogsService");

                _action = Properties.Settings.Default.Command;

                InitConfigParams();
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.log.Error($"Failed to parse args due to Exception: {ex}");
            }
            BuildJob();
        }

        private void BuildJob()
        {
            var schedFact = new StdSchedulerFactory();
            var sched = schedFact.GetScheduler();
            sched.Start();

            IJobDetail job1 = JobBuilder.Create<kttJob>()
                       .WithIdentity("Job1", "group2")
                       .UsingJobData("Action", _action)
                       .UsingJobData("Uri", _kttUri)
                       .UsingJobData("From", _from.ToString())
                       .UsingJobData("AttServerIp", _attServerIp)
                       .UsingJobData("AttServerPort", _attServerPort.ToString()).Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                       .WithIdentity("myTrigger1", "group2")                      
                       .StartNow()
                       .Build();



            IJobDetail job2 = JobBuilder.Create<kttJob>().WithIdentity("Job", "group1")
                .UsingJobData("Action", _action)
                .UsingJobData("Uri", _kttUri)
                .UsingJobData("From", _from.ToString())
                .UsingJobData("AttServerIp", _attServerIp)
                .UsingJobData("AttServerPort", _attServerPort.ToString()).Build();

            ITrigger trigger2 = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "group1").WithSchedule(
             CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(23, 59, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday)).Build();


            sched.ScheduleJob(job1, trigger1);

            sched.ScheduleJob(job2, trigger2);
        }


        private static void InitConfigParams()
        {
            try
            {
                if (_action == SYNC_ACTION)
                {


                    if (!DateTime.TryParse(Properties.Settings.Default.from.ToString().Replace('_', ' '), out _from))
                    {
                        _from = DateTime.Now;
                    }

                    Logger.LoggerInstance.log.Info($"_from: {_from.Date.ToString("yyyy:MM:dd hh:mm:ss")}");
                    #region set servers connection parameters
                    _kttUri = Properties.Settings.Default.ktturi;
                    _attServerIp = Properties.Settings.Default.attip;
                    IPAddress tmp;
                    if (!IPAddress.TryParse(_attServerIp, out tmp))
                        throw new InvalidOperationException("Invalid IP");
                    else _attServerPort = int.Parse(Properties.Settings.Default.attport);

                    Logger.LoggerInstance.log.Info($"_kttUri: {_kttUri} _attServerIp:{_attServerIp}");
                    #endregion
                }
                else
                {
                    throw new NotImplementedException("Action is not implemented");
                }

            }
            catch (Exception ex)
            {

                throw new InvalidCastException("falied to read command line arguments. " + ex.Message);
            }
        }
        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
