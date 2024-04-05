using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GifskiNet;

public unsafe class Gifski : IDisposable
{
    private readonly delegate* unmanaged[Cdecl]<nint, uint, string, double, GifskiError> _gifskiAddFramePngFile;
    private readonly delegate* unmanaged[Cdecl]<nint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgba;
    private readonly delegate* unmanaged[Cdecl]<nint, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgbaStride;
    private readonly delegate* unmanaged[Cdecl]<nint, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameArgb;
    private readonly delegate* unmanaged[Cdecl]<nint, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgb;
    private readonly delegate* unmanaged[Cdecl]<nint, ErrorMessageCallbackProxy, nint, GifskiError> _gifskiSetErrorMessageCallback;
    private readonly delegate* unmanaged[Cdecl]<nint, ProgressCallbackProxy, nint, GifskiError> _gifskiSetProgressCallback;
    private readonly delegate* unmanaged[Cdecl]<nint, WriteCallbackProxy, nint, GifskiError> _gifskiSetWriteCallback;
    private readonly delegate* unmanaged[Cdecl]<nint, string, GifskiError> _gifskiSetFileOutput;
    private readonly delegate* unmanaged[Cdecl]<nint, GifskiError> _gifskiFinish;

    private nint _libHandle;
    private nint _gifskiHandle;

    private nint _errorMessageCallbackHandle;
    private nint _progressCallbackHandle;
    private nint _writeCallbackHandle;

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

        _gifskiAddFramePngFile = (delegate* unmanaged[Cdecl]<nint, uint, string, double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_png_file");
        _gifskiAddFrameRgba = (delegate* unmanaged[Cdecl]<nint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_rgba");
        _gifskiAddFrameRgbaStride = (delegate* unmanaged[Cdecl]<nint, uint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_rgba_stride");
        _gifskiAddFrameArgb = (delegate* unmanaged[Cdecl]<nint, uint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_argb");
        _gifskiAddFrameRgb = (delegate* unmanaged[Cdecl]<nint, uint, uint, uint, uint, byte[], double, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_add_frame_rgb");
        _gifskiSetErrorMessageCallback = (delegate* unmanaged[Cdecl]<nint, ErrorMessageCallbackProxy, nint, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_error_message_callback");
        _gifskiSetProgressCallback = (delegate* unmanaged[Cdecl]<nint, ProgressCallbackProxy, nint, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_progress_callback");
        _gifskiSetWriteCallback = (delegate* unmanaged[Cdecl]<nint, WriteCallbackProxy, nint, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_write_callback");
        _gifskiSetFileOutput = (delegate* unmanaged[Cdecl]<nint, string, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_set_file_output");
        _gifskiFinish = (delegate* unmanaged[Cdecl]<nint, GifskiError>)
            NativeLibrary.GetExport(_libHandle, "gifski_finish");

        var gifskiNew = (delegate* unmanaged[Cdecl]<GifskiSettingsInternal*, nint>)
            NativeLibrary.GetExport(_libHandle, "gifski_new");

        var internalSettings = new GifskiSettingsInternal(settings);
        _gifskiHandle = gifskiNew(&internalSettings);

        if (settings.Extra && NativeLibrary.TryGetExport(_libHandle, "gifski_set_extra_effort", out var gifskiSetExtraEffortAddr))
        {
            var gifskiSetExtraEffort = (delegate* unmanaged[Cdecl]<nint, bool, GifskiError>)gifskiSetExtraEffortAddr;
            gifskiSetExtraEffort(_gifskiHandle, true);
        }

        if (settings.MotionQuality.HasValue && NativeLibrary.TryGetExport(_libHandle, "gifski_set_motion_quality", out var gifskiSetMotionQualityAddr))
        {
            var gifskiSetMotionQuality = (delegate* unmanaged[Cdecl]<nint, byte, GifskiError>)gifskiSetMotionQualityAddr;
            gifskiSetMotionQuality(_gifskiHandle, settings.MotionQuality.Value);
        }

        if (settings.LossyQuality.HasValue && NativeLibrary.TryGetExport(_libHandle, "gifski_set_lossy_quality", out var gifskiSetLossyQualityAddr))
        {
            var gifskiSetLossyQuality = (delegate* unmanaged[Cdecl]<nint, byte, GifskiError>)gifskiSetLossyQualityAddr;
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

    public GifskiError SetErrorMessageCallback(GifskiErrorMessageCallback callback, object context = null)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));
        Util.FreeGCHandleIfValid(ref _errorMessageCallbackHandle);
        var del = context is null
            ? callback
            : (a, _) => callback(a, context);
        var proxy = Delegates.Create(del, Delegates.ErrorCallbackProxy, out _, out var contextPtr);
        _errorMessageCallbackHandle = contextPtr;
        return _gifskiSetErrorMessageCallback(_gifskiHandle, proxy, contextPtr);
    }

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
        if (_gifskiHandle == nint.Zero)
            throw new GifskiException("You must not execute the 'Finish' function more than once!");
        var result = _gifskiFinish(_gifskiHandle);
        _gifskiHandle = nint.Zero;
        return result;
    }

    private void ReleaseUnmanagedResources()
    {
        Util.FreeLibraryIfValid(ref _libHandle);
        Util.FreeGCHandleIfValid(ref _errorMessageCallbackHandle);
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
