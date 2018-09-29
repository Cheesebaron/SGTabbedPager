using MvvmCross.Logging;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.ViewModels;
using SampleMvx;

namespace SampleMvx.iOS
{
    public class Setup : MvxIosSetup<Core.App>
    {

        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
        }

    }
}
