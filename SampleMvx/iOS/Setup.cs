using MvvmCross.Platforms.Ios.Core;
using MvvmCross.ViewModels;

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
