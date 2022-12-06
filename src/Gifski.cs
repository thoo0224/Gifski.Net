using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GifskiNet;

public unsafe class Gifski : IDisposable
{
    private readonly delegate* unmanaged[Cdecl] <IntPtr, uint, string, double, GifskiError> _gifskiAddFramePngFile;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgba;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgbaStride;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameArgb;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgb;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, ProgressCallbackProxy, IntPtr, GifskiError> _gifskiSetProgressCallback;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, string, GifskiError> _gifskiSetFileOutput;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, WriteCallbackProxy, IntPtr, GifskiError> _gifskiSetWriteCallback;
    private readonly delegate* unmanaged[Cdecl] <IntPtr, GifskiError> _gifskiFinish;

    private IntPtr _libHandle;
    private IntPtr _gifskiHandle;

    private IntPtr _progressCallbackHandle;
    private IntPtr _writeCallbackHandle;

    /// <summary>
    /// Initializes <see cref="Gifski"/> with the recommended quality (<c>90</c>)
    /// </summary>
    /// <param name="libraryPath">Path to the gifski binary.</param>
    /// <param name="settings">Optional <see cref="GifskiSettings"/>modifications.</param>
    public static Gifski Create(string libraryPath, Action<GifskiSettings> settings = null)
    {
        var gifskiSettings = new GifskiSettings();
        settings?.Invoke(gifskiSettings);
        return new Gifski(gifskiSettings, libraryPath);
    }

    private Gifski(GifskiSettings settings, string libraryPath)
    {
        _libHandle = NativeLibrary.Load(libraryPath);

        _gifskiAddFramePngFile = (delegate* unmanaged[Cdecl] <IntPtr, uint, string, double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_png_file");
        _gifskiAddFrameRgba = (delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_rgba");
        _gifskiAddFrameRgbaStride = (delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_rgba_stride");
        _gifskiAddFrameArgb = (delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_argb");
        _gifskiAddFrameRgb = (delegate* unmanaged[Cdecl] <IntPtr, uint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_rgb");
        _gifskiSetProgressCallback = (delegate* unmanaged[Cdecl] <IntPtr, ProgressCallbackProxy, IntPtr, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_progress_callback");
        _gifskiSetFileOutput = (delegate* unmanaged[Cdecl] <IntPtr, string, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_file_output");
        _gifskiSetWriteCallback = (delegate* unmanaged[Cdecl] <IntPtr, WriteCallbackProxy, IntPtr, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_write_callback");
        _gifskiFinish = (delegate* unmanaged[Cdecl] <IntPtr, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_finish");

        var gifskiNew = (delegate* unmanaged[Cdecl] <GifskiSettingsInternal*, IntPtr>)
            NativeLibrary.GetExport(_libHandle, "gifski_new");

        var internalSettings = new GifskiSettingsInternal(settings);
        _gifskiHandle = gifskiNew(&internalSettings);

        if (settings.Extra && NativeLibrary.TryGetExport(_libHandle, "gifski_set_extra_effort", out var gifskiSetExtraEffortAddr))
        {
            var gifskiSetExtraEffort = (delegate* unmanaged[Cdecl] <IntPtr, bool, GifskiError>)gifskiSetExtraEffortAddr;
            gifskiSetExtraEffort(_gifskiHandle, true);
        }

        if (settings.MotionQuality.HasValue && NativeLibrary.TryGetExport(_libHandle, "gifski_set_motion_quality", out var gifskiSetMotionQualityAddr))
        {
            var gifskiSetMotionQuality = (delegate* unmanaged[Cdecl] <IntPtr, byte, GifskiError>)gifskiSetMotionQualityAddr;
            gifskiSetMotionQuality(_gifskiHandle, settings.MotionQuality.Value);
        }

        if (settings.LossyQuality.HasValue && NativeLibrary.TryGetExport(_libHandle, "gifski_set_lossy_quality", out var gifskiSetLossyQualityAddr))
        {
            var gifskiSetLossyQuality = (delegate* unmanaged[Cdecl] <IntPtr, byte, GifskiError>)gifskiSetLossyQualityAddr;
            gifskiSetLossyQuality(_gifskiHandle, settings.LossyQuality.Value);
        }
    }

    public GifskiError AddFramePngFile(uint frameNumber, double presentationTimestamp, string filePath) =>
        _gifskiAddFramePngFile(_gifskiHandle, frameNumber, filePath, presentationTimestamp);

    public GifskiError AddFrameRgba(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels) =>
        _gifskiAddFrameRgba(_gifskiHandle, frameNumber, width, height, pixels, presentationTimestamp);

    public GifskiError AddFrameArgb(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow) =>
        _gifskiAddFrameArgb(_gifskiHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);

    public GifskiError AddFrameRgbaStride(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow) =>
        _gifskiAddFrameRgbaStride(_gifskiHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);

    public GifskiError AddFrameRgb(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow) =>
        _gifskiAddFrameRgb(_gifskiHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);

    public GifskiError SetFileOutput(string path) =>
        _gifskiSetFileOutput(_gifskiHandle, path);

    public GifskiError SetProgressCallback(GifskiProgressCallback callback, object context = null)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));
        Util.FreeGCHandleIfValid(ref _progressCallbackHandle);
        var del = context is null
            ? callback
            : _ => callback(context);
        var proxy = Delegates.Create(del, Delegates.ProgressCallbackProxy, out _, out var contextPtr);
        _progressCallbackHandle = contextPtr;
        return _gifskiSetProgressCallback(_gifskiHandle, proxy, contextPtr);
    }

    public GifskiError SetStreamOutput(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        var del = new GifskiWriteCallback((bufferLength, bufferPtr, context) =>
        {
            var outStream = (Stream)context;
            var span = new ReadOnlySpan<byte>(bufferPtr.ToPointer(), (int)bufferLength);
            outStream.Write(span);
            return true;
        });
        return SetWriteCallback(del, stream);
    }

    public GifskiError SetWriteCallback(GifskiWriteCallback callback, object context = null)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));
        Util.FreeGCHandleIfValid(ref _writeCallbackHandle);
        var del = context is null
            ? callback
            : (a, b, _) => callback(a, b, context);
        var proxy = Delegates.Create(del, Delegates.WriteCallbackProxy, out _, out var contextPtr);
        _writeCallbackHandle = contextPtr;
        return _gifskiSetWriteCallback(_gifskiHandle, proxy, contextPtr);
    }

    /// <summary>
    /// Starts the encoding process. All configurations must happen before executing.
    /// </summary>
    /// <exception cref="GifskiException">Thrown when executed more than once.</exception>
    public GifskiError Finish()
    {
        if (_gifskiHandle == IntPtr.Zero)
            throw new GifskiException("You must not execute the 'Finish' function more than once!");
        var result = _gifskiFinish(_gifskiHandle);
        _gifskiHandle = IntPtr.Zero;
        return result;
    }

    private void ReleaseUnmanagedResources()
    {
        Util.FreeLibraryIfValid(ref _libHandle);
        Util.FreeGCHandleIfValid(ref _progressCallbackHandle);
        Util.FreeGCHandleIfValid(ref _writeCallbackHandle);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Gifski() => ReleaseUnmanagedResources();
}
