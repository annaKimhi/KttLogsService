using KTT.Jobs;
using System;

namespace Ktt
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeReportsSyncJob job = new TimeReportsSyncJob();
            job.SyncData();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
