using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagReporter.DTOs
{
    public class DefaultWstResponse<T>
    {
        public List<T>? d { get; set; }
    }
}
