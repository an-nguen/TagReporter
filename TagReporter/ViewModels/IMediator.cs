using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagReporter.ViewModels
{
    public interface IMediator
    {
        public void RegisterView(IViewComponent o);
        public void ReloadResourceDictionary();
    }
}
