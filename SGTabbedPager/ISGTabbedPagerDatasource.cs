using UIKit;

namespace DK.Ostebaronen.Touch.SGTabbedPager
{
    public interface ISGTabbedPagerDatasource
    {
        int NumberOfViewControllers { get; }
        UIViewController GetViewController(int page);
        string GetViewControllerTitle(int page);
        UIImage GetViewControllerIcon(int page);
    }
}