using System;
using System.Runtime.InteropServices;

namespace GifskiNet
{
    public unsafe class Gifski : IDisposable
    {

        public delegate int ProgressCallback(IntPtr userData);
        public delegate GifskiError WriteCallback(IntPtr bufferSize, IntPtr buffer, IntPtr userData);

        private readonly IntPtr _dllHandle;
        private readonly IntPtr _gifskiHandle;

        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, string, double, GifskiError> _gifskiAddFramePngFile;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgba;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgbaStride;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameArgb;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgb;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, ProgressCallback, IntPtr, void> _gifskiSetProgressCallback;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, string, GifskiError> _gifskiSetFileOutput;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, WriteCallback, IntPtr, GifskiError> _gifskiSetWriteCallback;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, GifskiError> _gifskiFinish;

        /// <summary>
        /// Initializes <see cref="Gifski"/> with the recommended quality (<c>90</c>)
        /// </summary>
        /// <param name="libraryPath">Path to the gifski binary.</param>
        /// <param name="settings">Optional <see cref="GifskiSettings"/> modifications.</param>
        public static Gifski Create(string libraryPath, Action<GifskiSettings> settings = null)
        {
            var gifskiSettings = new GifskiSettings();
            settings?.Invoke(gifskiSettings);
            if (gifskiSettings.Quality == 0) gifskiSettings.Quality = 90;
            return new Gifski(gifskiSettings, libraryPath);
        }

        private Gifski(GifskiSettings settings, string libraryPath)
        {
            _dllHandle = NativeLibrary.Load(libraryPath);

            _gifskiAddFramePngFile = (delegate* unmanaged[Cdecl]<IntPtr, uint, string, double, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_add_frame_png_file");
            _gifskiAddFrameRgba = (delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, byte[], double, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_add_frame_rgba");
            _gifskiAddFrameRgbaStride = (delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_add_frame_rgba_stride");
            _gifskiAddFrameArgb = (delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_add_frame_argb");
            _gifskiAddFrameRgb = (delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_add_frame_rgb");
            _gifskiSetProgressCallback = (delegate* unmanaged[Cdecl]<IntPtr, ProgressCallback, IntPtr, void>)
                NativeLibrary.GetExport(_dllHandle, "gifski_set_progress_callback");
            _gifskiSetFileOutput = (delegate* unmanaged[Cdecl]<IntPtr, string, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_set_file_output");
            _gifskiSetWriteCallback = (delegate* unmanaged[Cdecl]<IntPtr, WriteCallback, IntPtr, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_set_write_callback");
            _gifskiFinish = (delegate* unmanaged[Cdecl]<IntPtr, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_finish");

            var gifskiNew = (delegate* unmanaged[Cdecl]<ref GifskiSettingsInternal, IntPtr>)
                NativeLibrary.GetExport(_dllHandle, "gifski_new");

            var internalSettings = new GifskiSettingsInternal(settings);
            _gifskiHandle = gifskiNew(ref internalSettings);
        }

        public GifskiError AddFramePngFile(uint frameNumber, double presentationTimestamp, string filePath)
        {
            return _gifskiAddFramePngFile(_gifskiHandle, frameNumber, filePath, presentationTimestamp);
        }

        public GifskiError AddFrameRgba(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels)
        {
            return _gifskiAddFrameRgba(_gifskiHandle, frameNumber, width, height, pixels, presentationTimestamp);
        }

        public GifskiError AddFrameArgb(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow)
        {
            return _gifskiAddFrameArgb(_gifskiHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);
        }

        public GifskiError AddFrameRgbaStride(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow)
        {
            return _gifskiAddFrameRgbaStride(_gifskiHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);
        }

        public GifskiError AddFrameRgb(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow)
        {
            return _gifskiAddFrameRgb(_gifskiHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);
        }

        public void SetProgressCallback(IntPtr userData, ProgressCallback callback)
        {
            _gifskiSetProgressCallback(_gifskiHandle, callback, userData);
        }

        public GifskiError SetFileOutput(string path)
        {
            return _gifskiSetFileOutput(_gifskiHandle, path);
        }

        public GifskiError SetWriteCallback(IntPtr userData, WriteCallback callback)
        {
            return _gifskiSetWriteCallback(_gifskiHandle, callback, userData);
        }

        public GifskiError Finish()
        {
            return _gifskiFinish(_gifskiHandle);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_dllHandle != IntPtr.Zero)
                {
                    NativeLibrary.Free(_dllHandle);
                }
            }

            IsDisposed = true;
        }

        ~Gifski()
        {
            Dispose(false);
        }

    }
}
