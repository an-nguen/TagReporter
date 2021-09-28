using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TagReporter.Models
{
    public class Config
    {
        [BsonId(false)]
        public string? Parameter { get; set; }
        public string? Value { get; set; }
    }
}
