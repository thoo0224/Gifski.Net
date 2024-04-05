using System;
using System.Runtime.InteropServices;

namespace GifskiNet;

internal static class Util
{
    public static void FreeGCHandleIfValid(ref nint ptr)
    {
        if (ptr == nint.Zero) return;
        var gch = GCHandle.FromIntPtr(ptr);
        gch.Free();
        ptr = nint.Zero;
    }

    public static void FreeLibraryIfValid(ref nint handle)
    {
        if (handle == nint.Zero) return;
        NativeLibrary.Free(handle);
        handle = nint.Zero;
    }
}
