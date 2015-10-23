using System;
using System.Collections.Generic;
using Cirrious.FluentLayouts.Touch;
using DK.Ostebaronen.Touch.SGTabbedPager;
using ObjCRuntime;
using UIKit;

namespace Sample
{
    public class MyViewController : SGTabbedPager, ISGTabbedPagerDatasource, ISGTabbedPagerDelegate
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

            if (RespondsToSelector(new Selector("setEdgesForExtendedLayout:")))
                EdgesForExtendedLayout = UIRectEdge.None;

            Datasource = this;
            TabColor = UIColorHelpers.GetRandomColor();
            HeaderFont = UIFont.BoldSystemFontOfSize(25);
            HeaderColor = UIColor.DarkGray;
            BottomLineColor = UIColor.White;
            Title = "SGTabbedPager Sample";
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

        public void DidShowViewController(int page)
        {
            Console.WriteLine($"Did show {page}");
        }
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
