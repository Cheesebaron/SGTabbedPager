using UIKit;

namespace DK.Ostebaronen.Touch.SGTabbedPager
{
    /// <summary>
    /// Data Source interface
    /// </summary>
    public interface ISGTabbedPagerDatasource
    {
        /// <summary>
        /// Number of ViewControllers to be presented in Pager
        /// </summary>
        int NumberOfViewControllers { get; }

        /// <summary>
        /// Method to get the ViewController for a specific page index
        /// </summary>
        /// <param name="page">page index</param>
        /// <returns><see cref="UIViewController"/> to show</returns>
        UIViewController GetViewController(int page);

        /// <summary>
        /// Method to get the title for a ViewController for a specific page index
        /// </summary>
        /// <param name="page">page index</param>
        /// <returns>Title for ViewController</returns>
        string GetViewControllerTitle(int page);

        /// <summary>
        /// Method to get the icon for a ViewController for a specific page index
        /// </summary>
        /// <param name="page">page index</param>
        /// <returns><see cref="UIImage"/> with the icon</returns>
        UIImage GetViewControllerIcon(int page);
    }
}