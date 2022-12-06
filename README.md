<div align="center">

# Gifski.Net

[Gifski](https://github.com/ImageOptim/gifski) wrapper for .NET 7 | Up to date with v1.8.1

[![GitHub release](https://img.shields.io/github/v/release/thoo0224/Gifski.Net?logo=github)](https://github.com/thoo0224/Gifski.Net/releases/latest) [![Nuget](https://img.shields.io/nuget/v/Gifski.Net?logo=nuget)](https://www.nuget.org/packages/Gifski.Net) [![Nuget DLs](https://img.shields.io/nuget/dt/Gifski.Net?logo=nuget)](https://www.nuget.org/packages/Gifski.Net) [![GitHub issues](https://img.shields.io/github/issues/thoo0224/Gifski.Net?logo=github)](https://github.com/thoo0224/Gifski.Net/issues) [![GitHub License](https://img.shields.io/github/license/thoo0224/Gifski.Net)](https://github.com/thoo0224/Gifski.Net/blob/master/LICENSE)

</div>

## Example Usage

```cs
using GifskiNet;

using var gifski = Gifski.Create(@"C:\Test\gifski.dll", settings =>
{
    settings.Quality = 100;
});

// Sets the output file of the gif
gifski.SetFileOutput(@"C:\Test\animation.gif");
// Or you can write it directly to a stream
var stream = new MemoryStream();
gifski.SetStreamOutput(stream);

gifski.AddFramePngFile(frameNumber: 0, presentationTimestamp: 0.0, filePath: @"C:\Test\frame_1.png");
gifski.AddFramePngFile(1, 4.0, @"C:\Test\frame_2.png");
gifski.AddFramePngFile(3, 6.0, @"C:\Test\frame_3.png");
gifski.Finish();
```

### NuGet

```md
Install-Package Gifski.Net
```

### Contribute
 
I am open for any contribution.
