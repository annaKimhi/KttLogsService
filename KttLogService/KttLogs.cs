﻿using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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
        private string _action = null;
        private DateTime _from = new DateTime();
        private string _kttUri;
        private const string _dateformat = "d/M/yyyy hh:mm";
        private string _attServerIp;//= "192.168.1.7";
        private int _attServerPort;// = 4370;
        private readonly string kttLogsJob = "kttLogsJob";

        public KttLogs()
        {
            InitializeComponent();
        }

        internal void StartDebug()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            try
            {

                Logger.LoggerInstance.log.Info("Parsing  args onStart AttLogsService");

                _action = ApplicationSettings.Command;

                InitConfigParams();
                BuildJob();
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.log.Error($"Failed to parse args due to Exception: {ex}");
            }

        }

        private void BuildJob()
        {
            var schedFact = new StdSchedulerFactory();
            var sched = schedFact.GetScheduler();
            sched.Start();

            IJobDetail immediateJob = JobBuilder.Create<kttJob>()
                       .WithIdentity("immediateJob", "immediateGroup")
                       .UsingJobData("Action", _action)
                       .UsingJobData("Uri", _kttUri)
                       .UsingJobData("From", _from.ToString())
                       .UsingJobData("AttServerIp", _attServerIp)
                       .UsingJobData("AttServerPort", _attServerPort.ToString()).Build();

            ITrigger immediateTrigger = TriggerBuilder.Create()
                       .WithIdentity("immediateTrigger", "immediateGroup")
                       .StartNow()
                       .Build();



            IJobDetail dailyJob = JobBuilder.Create<kttJob>().WithIdentity("dailyJob", "dailyGroup")
             .UsingJobData("Action", _action)
             .UsingJobData("Uri", _kttUri)
             .UsingJobData("From", _from.ToString())
             .UsingJobData("AttServerIp", _attServerIp)
             .UsingJobData("AttServerPort", _attServerPort.ToString()).Build();

            ITrigger dailyTrigger = TriggerBuilder.Create()
            .WithIdentity("dailyTrigger", "dailyGroup").WithSchedule(
             CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(23, 58, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday)).Build();



            //IJobDetail job3 = JobBuilder.Create<kttJob>().WithIdentity("myJob3", "group3").UsingJobData("Action", _action)
            // .UsingJobData("Uri", _kttUri)
            // .UsingJobData("From", _from.ToString())
            // .UsingJobData("AttServerIp", _attServerIp)
            // .UsingJobData("AttServerPort", _attServerPort.ToString()).Build();


            //ITrigger trigger3 = TriggerBuilder.Create()
            //    .WithIdentity("myTrigger3", "group3")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithIntervalInMinutes(1)
            //        .RepeatForever())
            //    .Build();
           


          //  sched.ScheduleJob(job3, trigger3);

            sched.ScheduleJob(immediateJob, immediateTrigger);

            sched.ScheduleJob(dailyJob, dailyTrigger);
        }


        private void InitConfigParams()
        {
            try
            {
                if (_action == SYNC_ACTION)
                {
                    _from = ApplicationSettings.LastSync;
                    Logger.LoggerInstance.log.Info($"_from: {_from.Date.ToString("yyyy:MM:dd hh:mm:ss")}");
                    #region set servers connection parameters
                    _kttUri = ApplicationSettings.KttUri;
                    _attServerIp = ApplicationSettings.AttIp;
                    IPAddress tmp;
                    if (!IPAddress.TryParse("192.168.1.7", out tmp))
                        throw new InvalidOperationException("Invalid IP");
                    else _attServerPort = int.Parse(ApplicationSettings.AttPort);

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
