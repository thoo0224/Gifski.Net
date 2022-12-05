using System.Runtime.InteropServices;

namespace GifskiNet;

public class GifskiSettings
{

    /// <summary>
    /// Resize to max this width if non-0.
    /// </summary>
    public uint Width { get; set; }

    /// <summary>
    /// Resize to max this height if width is non-0. Note that aspect ratio is not preserved.
    /// </summary>
    public uint Height { get; set; }

    private byte _quality;
    /// <summary>
    /// 1-100, but useful range is 50-100. Recommended to set to 90 (default).
    /// </summary>
    public byte Quality
    {
        get => _quality;
        set => _quality = value switch
        {
            0 => 1,
            > 100 => 100,
            _ => value
        };
    }

    /// <summary>
    /// Lower quality, but faster encode. Default = <see langword="false"/>.
    /// </summary>
    public bool Fast { get; set; }

    /// <summary>
    /// If negative, looping is disabled. The number of times the sequence is repeated. 0 to loop forever (default).
    /// </summary>
    public short Repeat { get; set; }
    /// <summary>
    /// Make the sequence loop forever.
    /// </summary>
    public void RepeatForever() => Repeat = 0;
    /// <summary>
    /// Disable loop for the sequence.
    /// </summary>
    public void RepeatDisable() => Repeat = -1;

    /// <summary>
    /// If <see langword="true"/>, encoding will be significantly slower, but may look a bit better.
    /// <para>Requires Gifski version >= <c>1.7.0</c></para>
    /// </summary>
    public bool Extra { get; set; }

    private byte? _motionQuality;
    /// <summary>
    /// Quality 1-100 of temporal denoising. Lower values reduce motion. Defaults to <see cref="Quality"/>.
    /// <para>Requires Gifski version >= <c>1.8.0</c></para>
    /// </summary>
    public byte? MotionQuality
    {
        get => _motionQuality;
        set => _motionQuality = value switch
        {
            0 => 1,
            > 100 => 100,
            _ => value
        };
    }

    private byte? _lossyQuality;
    /// <summary>
    /// Quality 1-100 of gifsicle compression. Lower values add noise. Defaults to <see cref="Quality"/>.
    /// <para>Has no effect if the <c>gifsicle</c> feature hasn't been enabled.</para>
    /// <para>Requires Gifski version >= <c>1.8.0</c></para>
    /// </summary>
    public byte? LossyQuality
    {
        get => _lossyQuality;
        set => _lossyQuality = value switch
        {
            0 => 1,
            > 100 => 100,
            _ => value
        };
    }

    public GifskiSettings()
    {
        _quality = 90;
    }

}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct GifskiSettingsInternal
{

    public readonly uint Width;
    public readonly uint Height;
    public readonly byte Quality;
    public readonly bool Fast;
    public readonly short Repeat;

    public GifskiSettingsInternal(GifskiSettings settings)
    {
        Width = settings.Width;
        Height = settings.Height;
        Quality = settings.Quality;
        Fast = settings.Fast;
        Repeat = settings.Repeat;
    }

}
