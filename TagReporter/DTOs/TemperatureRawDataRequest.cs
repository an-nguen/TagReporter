using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagReporter.DTOs
{
    public class TemperatureRawDataRequest
    {
        public string uuid { get; set; } = string.Empty;
        public string fromDate { get; set; } = string.Empty;
        public string toDate { get; set; } = string.Empty;
    }
}
