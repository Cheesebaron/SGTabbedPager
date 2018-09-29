using MvvmCross.ViewModels;

namespace SampleMvx.Core.ViewModels
{
    public class PageViewModel : MvxViewModel
    {
        private string _hello;
        public string Hello
        {
            get { return _hello; }
            set { SetProperty(ref _hello, value); }
        }
    }
}