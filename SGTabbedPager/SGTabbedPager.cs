using System;
using System.Collections.Generic;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

#if __MVX__
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.ViewModels;
#endif

namespace DK.Ostebaronen.Touch.SGTabbedPager
{
#if __MVX__
    public class MvxSGTabbedPager<TViewModel>
        : MvxViewController<TViewModel>
        , IUIScrollViewDelegate
        where TViewModel : class, IMvxViewModel
#else
    public class SGTabbedPager
        : UIViewController
        , IUIScrollViewDelegate
#endif
    {
        private readonly nfloat _tabHeight = 44f;
        protected UIScrollView TitleScrollView, ContentScrollView;
        private readonly IList<UIViewController> _viewControllers = new List<UIViewController>();
        private int _viewControllerCount;
        private readonly IList<UIButton> _tabButtons = new List<UIButton>();
        private UIView _bottomLine, _tabIndicator;
        private int _selectedIndex;
        private bool _enableParallax = true;
        private UIColor _tabColor;

        public UIViewController SelectedViewController => _viewControllers[_selectedIndex];

        public UIColor TabColor
        {
            get { return _tabColor; }
            set
            {
                _tabColor = value;
                if (_bottomLine != null)
                    _bottomLine.BackgroundColor = _tabColor;
                if (_tabIndicator != null)
                    _tabIndicator.BackgroundColor = _tabColor;
            }
        }

        public ISGTabbedPagerDatasource Datasource { get; set; }
        public ISGTabbedPagerDelegate Delegate { get; set; }

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(_selectedIndex, "selectedIndex");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            _selectedIndex = coder.DecodeInt("selectedIndex");
        }

        public override void LoadView()
        {
            base.LoadView();
            TitleScrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.White,
                CanCancelContentTouches = false,
                ShowsHorizontalScrollIndicator = false,
                Bounces = false,
                Delegate = this
            };
            View.AddSubview(TitleScrollView);

            _bottomLine = new UIView { BackgroundColor = _tabColor };
            TitleScrollView.AddSubview(_bottomLine);

            _tabIndicator = new UIView { BackgroundColor = _tabColor };
            TitleScrollView.AddSubview(_tabIndicator);

            ContentScrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DelaysContentTouches = false,
                ShowsHorizontalScrollIndicator = false,
                PagingEnabled = true,
                ScrollEnabled = true,
                Delegate = this
            };
            View.AddSubview(ContentScrollView);

            View.AddConstraints(
                TitleScrollView.AtTopOf(View),
                TitleScrollView.AtLeftOf(View),
                TitleScrollView.AtRightOf(View),
                TitleScrollView.Height().EqualTo(_tabHeight),

                ContentScrollView.Below(TitleScrollView),
                ContentScrollView.WithSameWidth(TitleScrollView),
                ContentScrollView.AtBottomOf(View)
                );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            ReloadData();
        }

        public override void ViewWillLayoutSubviews() => Layout();

        public override void WillTransitionToTraitCollection(UITraitCollection traitCollection,
            IUIViewControllerTransitionCoordinator coordinator)
        {
            TitleScrollView.Delegate = null;
            ContentScrollView.Delegate = null;
            coordinator.AnimateAlongsideTransition(null, context => {
                TitleScrollView.Delegate = this;
                ContentScrollView.Delegate = this;
                SwitchPage(_selectedIndex, false);
            });
        }

        public void ReloadData()
        {
            foreach (var viewController in _viewControllers)
            {
                viewController.WillMoveToParentViewController(null);
                viewController.View.RemoveFromSuperview();
                viewController.RemoveFromParentViewController();
            }
            _viewControllers.Clear();

            if (Datasource != null)
            {
                _viewControllerCount = Datasource.NumberOfViewControllers;
                for (var i = 0; i < _viewControllerCount; i++)
                {
                    var viewController = Datasource.GetViewController(i);

                    AddChildViewController(viewController);
                    var size = ContentScrollView.Frame.Size;
                    viewController.View.Frame = new CGRect(size.Width * i, 0, size.Width, size.Height);
                    ContentScrollView.AddSubview(viewController.View);
                    viewController.DidMoveToParentViewController(this);
                    _viewControllers.Add(viewController);
                }

                GenerateTabs();
                Layout();

                _selectedIndex = Math.Min(_viewControllerCount - 1, _selectedIndex);
                if (_selectedIndex > 0)
                {
                    SwitchPage(_selectedIndex, false);
                }
            }
        }

        public void SwitchPage(int index, bool animated)
        {
            var frame = new CGRect(ContentScrollView.Frame.Size.Width * index, 0,
                ContentScrollView.Frame.Size.Width, ContentScrollView.Frame.Size.Height);
            if (frame.X < ContentScrollView.ContentSize.Width)
            {
                _enableParallax = !animated;

                var point = _tabButtons[index].Frame;
                point.X -= (TitleScrollView.Bounds.Size.Width - _tabButtons[index].Frame.Size.Width) / 2f;
                TitleScrollView.SetContentOffset(new CGPoint(point.X, point.Y), animated);
                ContentScrollView.ScrollRectToVisible(frame, animated);
            }
        }

        private void GenerateTabs()
        {
            foreach (var button in _tabButtons)
                button.RemoveFromSuperview();

            _tabButtons.Clear();

            var font = UIFont.FromName("HelveticaNeue-Thin", 20);
            for (var i = 0; i < _viewControllerCount; i++)
            {
                var button = UIButton.FromType(UIButtonType.Custom);
                button.SetTitle(Datasource?.GetViewControllerTitle(i), UIControlState.Normal);
                button.SetTitleColor(UIColor.Black, UIControlState.Normal);
                if (button.TitleLabel != null)
                {
                    button.TitleLabel.Font = font;
                    button.TitleLabel.TextAlignment = UITextAlignment.Center;
                }
                button.SizeToFit();
                button.AddTarget(ReceivedButtonTab, UIControlEvent.TouchUpInside);
                _tabButtons.Add(button);
                TitleScrollView.AddSubview(button);
            }
        }

        private void ReceivedButtonTab(object sender, EventArgs e)
        {
            var button = sender as UIButton;
            if (button == null) return;

            var index = _tabButtons.IndexOf(button);
            SwitchPage(index, true);
        }

        protected void Layout()
        {
            var size = View.Bounds.Size;
            TitleScrollView.Frame = new CGRect(0, 0, size.Width, _tabHeight);
            ContentScrollView.Frame = new CGRect(0, _tabHeight, View.Bounds.Size.Width,
                View.Bounds.Size.Height - _tabHeight);

            nfloat currentX = 0f;
            size = ContentScrollView.Frame.Size;
            for (var i = 0; i < _viewControllerCount; i++)
            {
                var label = _tabButtons[i];
                if (i == 0)
                    currentX += (size.Width - label.Frame.Size.Width) / 2f;

                label.Frame = new CGRect(currentX, 0.0, label.Frame.Size.Width, _tabHeight);
                if (i == _viewControllerCount - 1)
                    currentX += (size.Width - label.Frame.Size.Width) / 2f + label.Frame.Size.Width;
                else
                    currentX += label.Frame.Size.Width + 30;
                var vc = _viewControllers[i];
                vc.View.Frame = new CGRect(size.Width * i, 0, size.Width, size.Height);
            }
            TitleScrollView.ContentSize = new CGSize(currentX, _tabHeight);
            ContentScrollView.ContentSize = new CGSize(size.Width * _viewControllerCount, size.Height);
            _bottomLine.Frame = new CGRect(0, _tabHeight - 1, TitleScrollView.ContentSize.Width, 1);
            LayoutTabIndicator();
        }

        private void LayoutTabIndicator()
        {
            var labelF = _tabButtons[_selectedIndex].Frame;
            _tabIndicator.Frame = new CGRect(labelF.X, labelF.Size.Height - 4, labelF.Size.Width, 4);
        }

        [Export("scrollViewDidScroll:")]
        public void Scrolled(UIScrollView scrollView)
        {
            if (!Equals(scrollView, ContentScrollView)) return;

            var pageWidth = scrollView.Frame.Size.Width;
            var page = (scrollView.ContentOffset.X - pageWidth / 2f) / pageWidth + 1;
            var next = (int)Math.Floor(page);

            if (next != _selectedIndex)
            {
                _selectedIndex = next;

                UIView.Animate(_enableParallax ? 0.3 : 0, LayoutTabIndicator, () => {
                    Delegate?.DidShowViewController(_selectedIndex);
                });
            }

            page = scrollView.ContentOffset.X / pageWidth;
            var index = (int)page;
            if (_enableParallax && index + 1 < _viewControllerCount)
            {
                var diff = _tabButtons[index + 1].Frame.X - _tabButtons[index].Frame.X;
                var centering1 = (TitleScrollView.Bounds.Size.Width -
                                  _tabButtons[index].Frame.Size.Width) / 2f;
                var centering2 = (TitleScrollView.Bounds.Size.Width -
                                  _tabButtons[index + 1].Frame.Size.Width) / 2f;
                var frac = page - Math.Truncate(page);
                var newXOffset = _tabButtons[index].Frame.X + diff * frac - centering1 * (1 - frac) -
                                 centering2 * frac;
                TitleScrollView.ContentOffset = new CGPoint(Math.Max(0, newXOffset), 0);
            }
        }

        [Export("scrollViewDidEndScrollingAnimation:")]
        public void ScrollAnimationEnded(UIScrollView scrollView)
        {
            if (Equals(scrollView, ContentScrollView))
                _enableParallax = true;
        }
    }
}
