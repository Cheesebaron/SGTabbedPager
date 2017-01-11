using System;
using System.Collections.Generic;
using Cirrious.FluentLayouts.Touch;
using DK.Ostebaronen.Touch.SGTabbedPager;
using ObjCRuntime;
using UIKit;

namespace Sample
{
    public class MyViewController : SGTabbedPager, ISGTabbedPagerDatasource
    {
        private static readonly List<PageViewController> Pages = new List<PageViewController> {
            new PageViewController {Number = 1},
            new PageViewController {Number = 2},
            new PageViewController {Number = 3},
            new PageViewController {Number = 4},
            new PageViewController {Number = 5},
            new PageViewController {Number = 6},
            new PageViewController {Number = 7},
            new PageViewController {Number = 8},
        };

        public override void ViewDidLoad()
        {
            View.BackgroundColor = UIColor.White;

            base.ViewDidLoad();

            // Adjust this to put pager on bottom or top
            //ShowOnBottom = true;

            //IconAlignment = IconAlignment.Right;

            OnShowViewController += OnPageShowing;

            if (RespondsToSelector(new Selector("setEdgesForExtendedLayout:")))
                EdgesForExtendedLayout = UIRectEdge.None;

            Datasource = this;
            TabColor = UIColorHelpers.GetRandomColor();
            HeaderFont = UIFont.SystemFontOfSize(25);
            HeaderColor = UIColor.DarkGray;
            SelectedHeaderFont = UIFont.BoldSystemFontOfSize(25);
            SelectedHeaderColor = UIColor.Black;
            IconSpacing = 20;
            BottomLineColor = UIColor.White;
            Title = "SGTabbedPager Sample";
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            OnShowViewController -= OnPageShowing;
        }

        public int NumberOfViewControllers => Pages.Count;

        public UIViewController GetViewController(int page)
        {
            return Pages[page];
        }

        public string GetViewControllerTitle(int page)
        {
            return Pages[page].Title;
        }

        private void OnPageShowing(object sender, int page)
        {
            Console.WriteLine($"Did show {page}");
        }

        private Random _rand = new Random();

        public UIImage GetViewControllerIcon(int page)
        {
            var index = _rand.Next(0, 4);
            return TitleImages[index];
        }

        private UIImage[] TitleImages = {
            UIImage.FromBundle("ic_email"),
            UIImage.FromBundle("ic_favorite"),
            UIImage.FromBundle("ic_help"),
            UIImage.FromBundle("ic_new_releases"),
            null
        };
    }

    public class PageViewController : UIViewController
    {
        public int Number { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColorHelpers.GetRandomColor();

            var label = new UILabel();
            Title = label.Text = $"Page {Number}";

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
