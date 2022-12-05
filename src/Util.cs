using System;
using System.Runtime.InteropServices;

namespace GifskiNet;

internal static class Util
{
    public static void FreeGCHandleIfValid(ref IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return;
        var gch = GCHandle.FromIntPtr(ptr);
        gch.Free();
        ptr = IntPtr.Zero;
    }

    public static void FreeLibraryIfValid(ref IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;
        NativeLibrary.Free(handle);
        handle = IntPtr.Zero;
    }
}
