using System;
using System.Collections.ObjectModel;
using Cirrious.FluentLayouts.Touch;
using DK.Ostebaronen.Touch.SGTabbedPager;
using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using ObjCRuntime;
using SampleMvx.Core.ViewModels;
using UIKit;

namespace SampleMvx.iOS.Views
{
    public class FirstView : MvxSGTabbedPager<FirstViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (RespondsToSelector(new Selector("setEdgesForExtendedLayout:")))
                EdgesForExtendedLayout = UIRectEdge.None;

            var removeButton = new UIBarButtonItem(UIBarButtonSystemItem.Trash);
            removeButton.Clicked += (sender, args) => ViewModel.RemovePageCommand.Execute(null);

            var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            addButton.Clicked += (sender, args) => ViewModel.AddPageCommand.Execute(null);

            NavigationItem.LeftBarButtonItem = removeButton;
            NavigationItem.RightBarButtonItem = addButton;

            var dataSource = new DataSource(this, ViewModel.Pages);

            var bset = this.CreateBindingSet<FirstView, FirstViewModel>();
            bset.Bind(dataSource).For(v => v.NumberOfViewControllers).To(vm => vm.PageCount);
            bset.Apply();

            Datasource = dataSource;
            TabColor = UIColorHelpers.GetRandomColor();
            HeaderFont = UIFont.SystemFontOfSize(25);
            HeaderColor = UIColor.DarkGray;
            SelectedHeaderFont = UIFont.BoldSystemFontOfSize(25);
            SelectedHeaderColor = UIColor.Black;
            BottomLineColor = UIColor.White;
            IconSpacing = 15;
            TabSpacing = 20;
            //StaticTabBar = true;
            Title = "MvxSGTabbedPager Sample";

            IconAlignment = IconAlignment.Right;
        }

        public class DataSource : ISGTabbedPagerDatasource
        {
            private readonly ObservableCollection<PageViewModel> _pages;

            public DataSource(FirstView pager, ObservableCollection<PageViewModel> pages)
            {
                _pages = pages;
                _pages.CollectionChanged += (sender, args) => pager.ReloadData();
            }

            public int NumberOfViewControllers { get; set; }
            public UIViewController GetViewController(int page)
            {
                return new PageViewController
                {
                    DataContext = _pages?[page]
                };
            }

            private Random _rand = new Random();
            public UIImage GetViewControllerIcon(int page)
            {
                var index = _rand.Next(0, 3);
                return TitleImages[index];
            }

            private UIImage[] TitleImages = {
                UIImage.FromBundle("ic_email"),
                UIImage.FromBundle("ic_favorite"),
                UIImage.FromBundle("ic_help"),
                UIImage.FromBundle("ic_new_releases")
            };

            public string GetViewControllerTitle(int page) => _pages[page]?.Hello;
        }
    }

    public class PageViewController : MvxViewController<PageViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColorHelpers.GetRandomColor();

            var label = new UILabel();

            var bset = this.CreateBindingSet<PageViewController, PageViewModel>();
            bset.Bind(label).To(vm => vm.Hello);
            bset.Apply();

            Add(label);

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            View.AddConstraints(
                label.AtTopOf(View, 10f),
                label.AtLeftOf(View, 10f),
                label.AtRightOf(View, 10f));
        }
    }

    public static class UIColorHelpers
    {
        public static UIColor GetRandomColor()
        {
            var random = new Random();
            var hue = (nfloat)(random.Next(0, 256) / 256.0f);
            var saturation = (nfloat)(random.Next(0, 128) / 256.0f) + 0.5f;
            var brightness = (nfloat)(random.Next(0, 128) / 256.0f) + 0.5f;
            return UIColor.FromHSB(hue, saturation, brightness);
        }
    }
}
