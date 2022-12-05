using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GifskiNet;

/// <summary>
/// The callback is called once per input frame, even if the encoder decides to skip some frames.
/// The callback must be thread-safe (it will be called from another thread).
/// It must remain valid at all times, until <see cref="Gifski.Finish"/> completes.
/// </summary>
/// <param name="context">Context to arbitrary user data.</param>
/// <returns>The callback must return <see langword="true"/> to continue processing, or <see langword="false"/> to abort.</returns>
public delegate bool GifskiProgressCallback(object context);
// int (*progress_callback)(void *user_data)
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate int ProgressCallbackProxy(IntPtr userDataPtr);

/// <summary>
/// The callback must be thread-safe (it will be called from another thread).
/// It must remain valid at all times, until <see cref="Gifski.Finish"/> completes.
/// </summary>
/// <param name="bufferLength">Size of the buffer to write, in bytes. IT MAY BE ZERO (when it's zero, either do nothing, or flush internal buffers if necessary).</param>
/// <param name="bufferPtr">Pointer to the buffer.</param>
/// <param name="context">Context to arbitrary user data.</param>
/// <returns>The callback should return <see langword="true"/> on success, and <see langword="false"/> on error.</returns>
public delegate bool GifskiWriteCallback(nint bufferLength, IntPtr bufferPtr, object context);
// int (*write_callback)(size_t buffer_length, const uint8_t *buffer, void *user_data)
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate GifskiError WriteCallbackProxy(nint bufferLength, IntPtr bufferPtr, IntPtr userDataPtr);

internal static class Delegates
{
    public static readonly ProgressCallbackProxy ProgressCallbackProxy = ProgressCallbackProxyImplementation;
    public static readonly WriteCallbackProxy WriteCallbackProxy = WriteCallbackProxyImplementation;

    [MonoPInvokeCallback(typeof(ProgressCallbackProxy))]
    private static int ProgressCallbackProxyImplementation(IntPtr userDataPtr)
    {
        var del = Get<GifskiProgressCallback>(userDataPtr, out _);
        try
        {
            return del.Invoke(null) ? 1 : 0;
        }
        catch
        {
            return 0;
        }
    }

	[MonoPInvokeCallback(typeof(WriteCallbackProxy))]
    private static GifskiError WriteCallbackProxyImplementation(nint bufferLength, IntPtr bufferPtr, IntPtr userDataPtr)
    {
        var del = Get<GifskiWriteCallback>(userDataPtr, out _);
        try
        {
            return del.Invoke(bufferLength, bufferPtr, null) ? GifskiError.OK : GifskiError.INTERRUPTED;
        }
        catch
        {
            return GifskiError.INTERRUPTED;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TManaged Get<TManaged>(IntPtr contextPtr, out GCHandle gch) where TManaged : Delegate
    {
        if (contextPtr == IntPtr.Zero)
        {
            gch = default;
            return default;
        }
        gch = GCHandle.FromIntPtr(contextPtr);
        return (TManaged)gch.Target;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNative Create<TManaged, TNative>(TManaged managedDel, TNative nativeDel, out GCHandle gch, out IntPtr contextPtr)
        where TManaged : Delegate
        where TNative : Delegate
    {
        if (managedDel == null)
        {
            gch = default;
            contextPtr = IntPtr.Zero;
            return default;
        }
        gch = GCHandle.Alloc(managedDel);
        contextPtr = GCHandle.ToIntPtr(gch);
        return nativeDel;
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type type) => Type = type;
    public Type Type { get; }
}
