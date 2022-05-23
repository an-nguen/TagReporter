using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TagReporter.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page? GetPage(string key);
}

