using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KttLogService
{
    public struct TimeReportRepository
    {
        public int EnrollNumber { get; set; }
        public int VerifyMode { get; set; }
        public bool InOutMode { get; set; }
        public DateTime TimeReport { get; set; }
        public int WorkCode { get; set; }
    }
}
