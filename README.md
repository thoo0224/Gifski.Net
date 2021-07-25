# Gifski.Net
 Gifski wrapper for .NET 5

<div align="center">

# Gifski.Net

A .NET 5 wrapper for Gifski

[![GitHub release](https://img.shields.io/github/v/release/thoo0224/Gifski.Net?logo=github)](https://github.com/thoo0224/Gifski.Net/releases/latest) [![Nuget](https://img.shields.io/nuget/v/Gifski.Net?logo=nuget)](https://www.nuget.org/packages/Gifski.Net) [![Nuget DLs](https://img.shields.io/nuget/dt/Gifski.Net?logo=nuget)](https://www.nuget.org/packages/Gifski.Net) [![GitHub issues](https://img.shields.io/github/issues/thoo0224/Gifski.Net?logo=github)](https://github.com/thoo0224/Gifski.Net/issues) [![GitHub License](https://img.shields.io/github/license/thoo0224/Gifski.Net)](https://github.com/thoo0224/Gifski.Net/blob/master/LICENSE)

</div>

## Example Usage

```cs
using GifskiNet;

var settings = new GifskiSettings
{
    Quality = 100
};
using var gifski = new Gifski(settings, @"C:\Test\gifski.dll");
gifski.SetFileOutput(@"C:\Test\animation.gif");
gifski.AddFramePngFile(0U, 0d, @"C:\Test\frame_1.gif");
gifski.AddFramePngFile(1U, 4d, @"C:\Test\frame_2.gif");
gifski.AddFramePngFile(3U, 6d, @"C:\Test\frame_3.gif");
gifski.Finish();
```

### NuGet

```md
Install-Package Gifski.Net
```

### Contribute
 
I am open for any contribution.
