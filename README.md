# SGTabbedPager

[![Build status](https://ci.appveyor.com/api/projects/status/6thrax3emk41gab4?svg=true)](https://ci.appveyor.com/project/Cheesebaron/sgtabbedpager)

Port of [SGViewPager][4] for Xamarin.iOS

## Installing
Grab the latest version from NuGet

    > PM Install-Package SGTabbedPager

or for MvvmCross

    > PM Install-Package SGTabbedPagerMvx
    
## Usage
Make your `UIVievController` inherit from `SGTabbedPager` or if you are using MvvmCross `SGTabbedPager<TViewModel>`. Implement the two interfaces `ISGTabbedPagerDelegate` and `ISGTabbedPagerDatasource`.

Make sure you set the `DataSource` on your ViewControll to your `ISGTabbedPagerDatasource` implementation.

Use `TabColor` to set the color of the indicator on each tab item.

See the [Sample][2] for usage.

![gif][1]

## License
See the [License file][3].


[1]: http://zippy.gfycat.com/UnderstatedCheerfulAsiandamselfly.gif
[2]: https://github.com/Cheesebaron/SGTabbedPager/tree/master/Samples/Sample
[3]: https://github.com/Cheesebaron/SGTabbedPager/blob/master/LICENSE
[4]: https://github.com/graetzer/SGViewPager
