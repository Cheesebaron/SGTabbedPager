using Foundation;
using MvvmCross.Platforms.Ios.Core;

namespace SampleMvx.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : MvxApplicationDelegate<Setup, Core.App>
    {
        // using defaults
    }
}
