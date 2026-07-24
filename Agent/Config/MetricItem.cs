using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Config
{
    public class MetricItem
    {
        public long Id { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public double? CpuLoad { get; set; }
        public double? MemoryLoad { get; set; }
        public double? GpuLoad { get; set; }
    }
}
