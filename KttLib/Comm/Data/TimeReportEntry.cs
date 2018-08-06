using System;

namespace KTT.Comm.Data
{
    public struct TimeReportEntry
    {
        public int EnrollNumber { get; set; }
        public int VerifyMode { get; set; }
        public bool InOutMode { get; set; }
        public DateTime TimeReport { get; set; }
        public int WorkCode { get; set; }
    }
}
