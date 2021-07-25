using System;
using System.Runtime.InteropServices;

namespace GifskiNet
{
    public unsafe class Gifski : IDisposable
    {

        public delegate int ProgressCallback(IntPtr userData);

        private readonly IntPtr _dllHandle;
        private readonly IntPtr _objHandle;

        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, string, double, GifskiError> _gifskiAddFramePngFile;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgba;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgbaStride;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameArgb;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, uint, uint, uint, uint, byte[], double, GifskiError> _gifskiAddFrameRgb;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, ProgressCallback, IntPtr, void> _gifskiSetProgressCallback;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, string, GifskiError> _gifskiSetFileOutput;
        private readonly delegate* unmanaged[Cdecl]<IntPtr, GifskiError> _gifskiFinish;

        public Gifski(GifskiSettings settings, string libraryPath)
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
            _gifskiFinish = (delegate* unmanaged[Cdecl]<IntPtr, GifskiError>)
                NativeLibrary.GetExport(_dllHandle, "gifski_finish");

            var gifskiNew = (delegate* unmanaged[Cdecl]<ref GifskiSettings, IntPtr>)
                NativeLibrary.GetExport(_dllHandle, "gifski_new");
            _objHandle = gifskiNew(ref settings);
        }

        ~Gifski()
        {
            Dispose();
        }

        public GifskiError AddFramePngFile(uint frameNumber, double presentationTimestamp, string filePath)
        {
            return _gifskiAddFramePngFile(_objHandle, frameNumber, filePath, presentationTimestamp);
        }

        public GifskiError AddFrameRgba(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels)
        {
            return _gifskiAddFrameRgba(_objHandle, frameNumber, width, height, pixels, presentationTimestamp);
        }

        public GifskiError AddFrameArgb(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow)
        {
            return _gifskiAddFrameArgb(_objHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);
        }

        public GifskiError AddFrameRgbaStride(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow)
        {
            return _gifskiAddFrameRgbaStride(_objHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);
        }

        public GifskiError AddFrameRgb(uint frameNumber, double presentationTimestamp, uint width, uint height, byte[] pixels, uint bytesPerRow)
        {
            return _gifskiAddFrameRgb(_objHandle, frameNumber, width, height, bytesPerRow, pixels, presentationTimestamp);
        }

        public void SetProgressCallback(IntPtr userData, ProgressCallback callback)
        {
            _gifskiSetProgressCallback(_objHandle, callback, userData);
        }

        public GifskiError SetFileOutput(string path)
        {
            return _gifskiSetFileOutput(_objHandle, path);
        }

        public GifskiError Finish()
        {
            return _gifskiFinish(_objHandle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_dllHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_dllHandle);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
