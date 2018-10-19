### New in 1.1.0
- Added ability to position pager on bottom, through the ShowBottom property
- Breaking: Removed ISGTabbedPagerDelegate, use the OnShowViewController event instead
- (MvvmCross version) Updated to latest 4.4.0 packages 

### New in 1.2.0
- Added Icon support, adds a new method in the ISGTabbedPagerDatasource interface. Just return null if you don't want an icons showing.

### New in 1.3.0
- Added support for adjusting Selected Item Color and Font
- Fixed issue with IconSpacing, which wasn't respected 

### New in 1.3.1
- Added constructors for use in storyboards and designers. Thanks @eestein
- Added Exports for properties for use in designers

### New in 1.4.0
- Built against MvvmCross 5.0.3
- Added XML doc

### New in 1.5.0
- Added ability to have the Tab Bar Static, no scrolling ([#21](https://github.com/Cheesebaron/SGTabbedPager/pull/21)) ([BitKaitsu](https://github.com/BitKaitsu))
- Added ability to add spacing between tabs ([#21](https://github.com/Cheesebaron/SGTabbedPager/pull/21)) ([BitKaitsu](https://github.com/BitKaitsu))
- Built against MvvmCross 5.5.2
- Built against Cirrious.FluentLayout 2.6.0

### New in 1.6.0
- Built against MvvmCross 6.2.0 ([#25](https://github.com/Cheesebaron/SGTabbedPager/pull/25)) ([MrBasque](https://github.com/MrBasque))
- Built against Cirrious.FluentLayout 2.7.0 ([#25](https://github.com/Cheesebaron/SGTabbedPager/pull/25)) ([MrBasque](https://github.com/MrBasque))