using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TagReporter.Utils
{
    class ResourceUtils
    {
        public static string GetResourceStr(ResourceDictionary resource, string name) => resource[name].ToString() ?? name;
    }
}
