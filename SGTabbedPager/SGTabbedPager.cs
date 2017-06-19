using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

#if __MVX__
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Views;
#endif

namespace DK.Ostebaronen.Touch.SGTabbedPager
{
    /// <summary>
    /// Tabbed pager ViewController. Shows a pager strip on top of the View, with a ScrollView underneath in pager mode
    /// </summary>
    [Register("SGTabbedPager")]
    [DesignTimeVisible(true)]
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
        private readonly IList<UIViewController> _viewControllers = new List<UIViewController>();
        private int _viewControllerCount;
        private readonly IList<UIButton> _tabButtons = new List<UIButton>();
        private UIView _bottomLine, _tabIndicator;
        private int _selectedIndex;
        private bool _enableParallax = true;
        private UIColor _tabColor;
        private UIColor _bottomLineColor;
        private UIColor _titleBackgroundColor = UIColor.White;
        private bool _showOnBottom;

        /// <summary>
        /// Scroll View for the Pager
        /// </summary>
        protected UIScrollView TitleScrollView;

        /// <summary>
        /// Scroll View for the Content
        /// </summary>
        protected UIScrollView ContentScrollView;

#if __MVX__

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DK.Ostebaronen.Touch.SGTabbedPager.MvxSGTabbedPager`1"/> class.
        /// </summary>
        public MvxSGTabbedPager() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DK.Ostebaronen.Touch.SGTabbedPager.MvxSGTabbedPager`1"/> class
        /// to be instantiated by Storyboard
        /// </summary>
        /// <param name="handle">Handle.</param>
        public MvxSGTabbedPager(IntPtr handle) : base(handle) { }

#else

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DK.Ostebaronen.Touch.SGTabbedPager.SGTabbedPager"/> class.
        /// </summary>
        public SGTabbedPager() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DK.Ostebaronen.Touch.SGTabbedPager.SGTabbedPager"/> class
        /// to be instantiated by Storyboard
        /// </summary>
        /// <param name="handle">Handle.</param>
        public SGTabbedPager(IntPtr handle) : base(handle) { }

#endif

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="T:DK.Ostebaronen.Touch.SGTabbedPager.SGTabbedPager"/> shows on bottom.
        /// </summary>
        /// <value><c>true</c> if show on bottom; otherwise, <c>false</c>.</value>
        [Export("ShowOnBottom"), Browsable(true)]
        public bool ShowOnBottom
        {
            get { return _showOnBottom; }
            set
            {
                _showOnBottom = value;
                AdjustConstraints();
            }
        }

        /// <summary>
        /// Currently selected <see cref="UIViewController"/>.
        /// </summary>
        public UIViewController SelectedViewController
        {
            get
            {
                if (_selectedIndex < 0) return null;
                if (!_viewControllers.Any()) return null;
                return _viewControllers[_selectedIndex];
            }
        }

        /// <summary>
        /// <see cref="UIColor"/> used for the background color of the Tab Indicator.
        /// </summary>
        [Export("TabColor"), Browsable(true)]
        public UIColor TabColor
        {
            get { return _tabColor; }
            set
            {
                _tabColor = value;
                if (_tabIndicator != null)
                    _tabIndicator.BackgroundColor = _tabColor;
            }
        }

        /// <summary>
        /// <see cref="UIColor"/> used for the background color of the Tab Scroll Area.
        /// </summary>
        [Export("TitleBackgroundColor"), Browsable(true)]
        public UIColor TitleBackgroundColor
        {
            get { return _titleBackgroundColor; }
            set
            {
                _titleBackgroundColor = value;
                if (TitleScrollView != null)
                    TitleScrollView.BackgroundColor = _titleBackgroundColor;
            }
        }

        /// <summary>
        /// <see cref="UIFont"/> used for the Tab Items.
        /// </summary>
        /// <value>The header font.</value>
        [Export("HeaderFont"), Browsable(true)]
        public UIFont HeaderFont { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UIFont"/> for a selected Tab Item.
        /// </summary>
        /// <value>The selected header font.</value>
        [Export("SelectedHeaderFont"), Browsable(true)]
        public UIFont SelectedHeaderFont { get; set; }

        /// <summary>
        /// <see cref="UIColor"/> of the line of the bottom of the Tab Indicator.
        /// </summary>
        [Export("BottomLineColor"), Browsable(true)]
        public UIColor BottomLineColor
        {
            get { return _bottomLineColor; }
            set
            {
                _bottomLineColor = value;
                if (_bottomLine != null)
                    _bottomLine.BackgroundColor = _bottomLineColor;
            }
        }

        /// <summary>
        /// <see cref="UIColor"/> of the title on each Tab Item.
        /// </summary>
        [Export("HeaderColor"), Browsable(true)]
        public UIColor HeaderColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the selected Tab Item.
        /// </summary>
        /// <value>The color of the selected Tab Item.</value>
        [Export("SelectedHeaderColor"), Browsable(true)]
        public UIColor SelectedHeaderColor { get; set; }

        /// <summary>
        /// The <see cref="ISGTabbedPagerDatasource"/> describing which <see cref="UIViewController"/> to present.
        /// </summary>
        public ISGTabbedPagerDatasource Datasource { get; set; }

        /// <summary>
        /// Gets or sets the icon alignment for tab titles.
        /// </summary>
        /// <value>The icon alignment.</value>
        [Export("IconAlignment"), Browsable(true)]
        public IconAlignment IconAlignment { get; set; }

        /// <summary>
        /// Gets or sets the icon spacing. This is the space for the Edge Inset for the
        /// Image. 
        /// </summary>
        /// <value>The icon spacing.</value>
        [Export("IconSpacing"), Browsable(true)]
        public int IconSpacing { get; set; } = 6;

        /// <summary>
        /// Event invoked when <see cref="UIViewController"/> is changed.
        /// </summary>
        public event EventHandler<int> OnShowViewController;

        /// <inheritdoc />
        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(_selectedIndex, "selectedIndex");
        }

        /// <inheritdoc />
        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            _selectedIndex = coder.DecodeInt("selectedIndex");
        }

        /// <inheritdoc />
        public override void LoadView()
        {
            base.LoadView();
            TitleScrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = _titleBackgroundColor,
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

            AdjustConstraints();
        }

        /// <inheritdoc />
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TitleScrollView.Delegate = this;
            ContentScrollView.Delegate = this;

            ReloadData();
        }

        /// <inheritdoc />
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            foreach (var btn in _tabButtons)
            {
                btn.RemoveTarget(ReceivedButtonTab, UIControlEvent.TouchUpInside);
            }

            if (TitleScrollView != null)
                TitleScrollView.Delegate = null;
            if (ContentScrollView != null)
                ContentScrollView.Delegate = null;
        }

        /// <inheritdoc />
        public override void ViewWillLayoutSubviews() => Layout();

        /// <inheritdoc />
        public override void WillTransitionToTraitCollection(UITraitCollection traitCollection,
            IUIViewControllerTransitionCoordinator coordinator)
        {
            if (TitleScrollView != null)
                TitleScrollView.Delegate = null;
            if (ContentScrollView != null)
                ContentScrollView.Delegate = null;

            coordinator?.AnimateAlongsideTransition(context => { }, context =>
            {
                if (TitleScrollView != null)
                    TitleScrollView.Delegate = this;
                if (ContentScrollView != null)
                    ContentScrollView.Delegate = this;
                SwitchPage(_selectedIndex, false);
            });
        }

        /// <summary>
        /// Reload data. Use this if you add or remove <see cref="UIViewController"/> instances from the <see cref="ISGTabbedPagerDatasource"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only (should actually never happen).</exception>
        public void ReloadData()
        {
            foreach (var viewController in _viewControllers)
            {
                viewController.WillMoveToParentViewController(null);
                viewController.View.RemoveFromSuperview();
                viewController.RemoveFromParentViewController();
            }
            _viewControllers.Clear();

            if (Datasource == null) return;

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
            _selectedIndex = Math.Max(0, _selectedIndex);
            if (_selectedIndex > 0)
                SwitchPage(_selectedIndex, false);
        }

        /// <summary>
        /// Switch the page
        /// </summary>
        /// <param name="index"><see cref="int"/> with the index of the page to switch to. Zero indexed.</param>
        /// <param name="animated"><see cref="bool"/> describing whether to animate the scroll to the page. 
        /// Will also enable a parallax effect on the Tab Indicator itself.</param>
        public void SwitchPage(int index, bool animated)
        {
            if (ContentScrollView == null) return;
            if (TitleScrollView == null) return;

            var frame = new CGRect(ContentScrollView.Frame.Size.Width * index, 0,
                ContentScrollView.Frame.Size.Width, ContentScrollView.Frame.Size.Height);
            if (frame.X >= ContentScrollView.ContentSize.Width) return;

            _enableParallax = !animated;

            var point = _tabButtons[index].Frame;
            point.X -= (TitleScrollView.Bounds.Size.Width - _tabButtons[index].Frame.Size.Width) / 2f;
            TitleScrollView.SetContentOffset(new CGPoint(point.X, point.Y), animated);
            ContentScrollView.ScrollRectToVisible(frame, animated);
        }

        private void GenerateTabs()
        {
            foreach (var button in _tabButtons)
                button.RemoveFromSuperview();

            _tabButtons.Clear();

            if (Datasource == null) return;

            var font = HeaderFont ?? UIFont.FromName("HelveticaNeue", 20);
            var selectedFont =
                SelectedHeaderFont ?? UIFont.FromName("HelveticaNeue-Bold", 20);
            var headerColor = HeaderColor ?? UIColor.DarkGray;
            var selectedHeaderColor = SelectedHeaderColor ?? UIColor.Black;

            for (var i = 0; i < _viewControllerCount; i++)
            {
                var title = Datasource.GetViewControllerTitle(i);
                var image = Datasource.GetViewControllerIcon(i);

                var button = UIButton.FromType(UIButtonType.Custom);
                button.SetTitle(title, UIControlState.Normal);
                button.SetTitleColor(headerColor, UIControlState.Normal);
                if (button.TitleLabel != null)
                {
                    button.TitleLabel.Font = font;
                    button.TitleLabel.TextAlignment = UITextAlignment.Center;
                }

                if (i == _selectedIndex)
                {
                    button.TitleLabel.Font = selectedFont;
                    button.SetTitleColor(selectedHeaderColor, UIControlState.Normal);
                }

                if (image != null)
                {
                    button.SetImage(image, UIControlState.Normal);
                    button.ImageEdgeInsets = new UIEdgeInsets(0, -IconSpacing, 0, 0);

                    if (IconAlignment == IconAlignment.Right)
                    {
                        button.Transform =
                            CGAffineTransform.MakeScale(-1.0f, 1.0f);
                        button.TitleLabel.Transform =
                            CGAffineTransform.MakeScale(-1.0f, 1.0f);
                        button.ImageView.Transform =
                            CGAffineTransform.MakeScale(-1.0f, 1.0f);
                    }

                    SizeButtonToFit(button, title, font, selectedFont);

                    var imageSize = image.Size.Width;
                    var textSize = new NSString(title).StringSize(font).Width;
                    var width = textSize + imageSize + IconSpacing;
                    button.Frame = new CGRect(0, 0, width, button.Frame.Height);
                }
                else
                {
                    SizeButtonToFit(button, title, font, selectedFont);
                }

                button.AddTarget(ReceivedButtonTab, UIControlEvent.TouchUpInside);
                _tabButtons.Add(button);
                TitleScrollView.AddSubview(button);
            }
        }

        private void SizeButtonToFit(UIButton button, string title, UIFont normalFont, UIFont selectedFont)
        {
            if (title != null)
            {
                var currentFont = button.Font;

                var first = new NSString(title).StringSize(normalFont).Width;
                var second = new NSString(title).StringSize(selectedFont).Width;

                if (first > second)
                    button.Font = normalFont;
                else
                    button.Font = selectedFont;

                button.SizeToFit();

                button.Font = currentFont;
            } 
            else 
            {
                button.SizeToFit();
            }
        }

        private void ReceivedButtonTab(object sender, EventArgs e)
        {
            var button = sender as UIButton;
            if (button == null) return;

            var index = _tabButtons.IndexOf(button);
            SwitchPage(index, true);
        }

        /// <summary>
        /// Layout content and title
        /// </summary>
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
                    currentX +=
                        (size.Width - label.Frame.Size.Width) / 2f + label.Frame.Size.Width;
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
            if (_tabButtons.Count == 0) return;
            var labelF = _tabButtons[_selectedIndex].Frame;
            _tabIndicator.Frame = new CGRect(labelF.X, labelF.Size.Height - 4, labelF.Size.Width, 4);
        }

        /// <inheritdoc cref="UIScrollView.Scrolled" />
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

                UIView.Animate(_enableParallax ? 0.3 : 0, LayoutTabIndicator, () =>
                {
                    var headerColor = HeaderColor ?? UIColor.DarkGray;
                    // Set original font for all buttons
                    foreach (var button in _tabButtons)
                    {
                        var originalFont =
                            HeaderFont ?? UIFont.FromName("HelveticaNeue", 20);
                        button.TitleLabel.Font = originalFont;
                        button.SetTitleColor(headerColor, UIControlState.Normal);
                    }

                    headerColor = SelectedHeaderColor ?? UIColor.Black;
                    // Set the selected font for current button
                    var item = _tabButtons[_selectedIndex];
                    var font =
                        SelectedHeaderFont ?? UIFont.FromName("HelveticaNeue-Bold", 20);
                    item.TitleLabel.Font = font;
                    item.SetTitleColor(headerColor, UIControlState.Normal);
                    OnShowViewController?.Invoke(this, _selectedIndex);
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

        /// <inheritdoc cref="UIScrollView.ScrollAnimationEnded" />
        [Export("scrollViewDidEndScrollingAnimation:")]
        public void ScrollAnimationEnded(UIScrollView scrollView)
        {
            if (Equals(scrollView, ContentScrollView))
                _enableParallax = true;
        }

        private void AdjustConstraints()
        {
            View.RemoveConstraints(View.Constraints);

            View.AddConstraints(
                ShowOnBottom ?
                    TitleScrollView.AtBottomOf(View) :
                    TitleScrollView.AtTopOf(View),

                TitleScrollView.AtLeftOf(View),
                TitleScrollView.AtRightOf(View),
                TitleScrollView.Height().EqualTo(_tabHeight),

                ContentScrollView.WithSameWidth(TitleScrollView),

                ShowOnBottom ?
                    ContentScrollView.Above(TitleScrollView) :
                    ContentScrollView.Below(TitleScrollView),

                ShowOnBottom ?
                    ContentScrollView.AtTopOf(View) :
                    ContentScrollView.AtBottomOf(View)
                );
        }
    }
}
