using System.Windows;

namespace TagReporter.Utils
{
    class ResourceUtils
    {
        public static string GetResourceStr(ResourceDictionary resource, string name) => resource[name].ToString() ?? name;
    }
}
