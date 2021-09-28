using System.Windows;

namespace TagReporter.ViewModels
{
    public interface IViewComponent
    {
        public void SetMediator(IMediator mediator);
        public ResourceDictionary GetResourceDictionary();
    }
}
