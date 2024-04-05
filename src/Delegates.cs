using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GifskiNet;

/// <summary>
/// The callback must be thread-safe (it will be called from another thread).
/// It must remain valid at all times, until <see cref="Gifski.Finish"/> completes.
/// </summary>
/// <param name="utf8Message">Message encoded in UTF-8.</param>
/// <param name="context">Context to arbitrary user data.</param>
public delegate bool GifskiErrorMessageCallback(ReadOnlySpan<byte> utf8Message, object context);
// void (*error_message_callback)(const char*, void*)
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void ErrorMessageCallbackProxy(nint utf8MessagePtr, nint userDataPtr);

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
internal delegate int ProgressCallbackProxy(nint userDataPtr);

/// <summary>
/// The callback must be thread-safe (it will be called from another thread).
/// It must remain valid at all times, until <see cref="Gifski.Finish"/> completes.
/// </summary>
/// <param name="bufferLength">Size of the buffer to write, in bytes. IT MAY BE ZERO (when it's zero, either do nothing, or flush internal buffers if necessary).</param>
/// <param name="bufferPtr">Pointer to the buffer.</param>
/// <param name="context">Context to arbitrary user data.</param>
/// <returns>The callback should return <see langword="true"/> on success, and <see langword="false"/> on error.</returns>
public delegate bool GifskiWriteCallback(nint bufferLength, nint bufferPtr, object context);
// int (*write_callback)(size_t buffer_length, const uint8_t *buffer, void *user_data)
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate GifskiError WriteCallbackProxy(nint bufferLength, nint bufferPtr, nint userDataPtr);

internal static class Delegates
{
    public static readonly ErrorMessageCallbackProxy ErrorCallbackProxy = ErrorMessageCallbackProxyImplementation;
    public static readonly ProgressCallbackProxy ProgressCallbackProxy = ProgressCallbackProxyImplementation;
    public static readonly WriteCallbackProxy WriteCallbackProxy = WriteCallbackProxyImplementation;

    [MonoPInvokeCallback(typeof(ProgressCallbackProxy))]
    private static unsafe void ErrorMessageCallbackProxyImplementation(nint utf8MessagePtr, nint userDataPtr)
    {
        var del = Get<GifskiErrorMessageCallback>(userDataPtr, out _);
        var utf8Message = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)utf8MessagePtr);
        del.Invoke(utf8Message, null);
    }

    [MonoPInvokeCallback(typeof(ProgressCallbackProxy))]
    private static int ProgressCallbackProxyImplementation(nint userDataPtr)
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
    private static GifskiError WriteCallbackProxyImplementation(nint bufferLength, nint bufferPtr, nint userDataPtr)
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
    public static TManaged Get<TManaged>(nint contextPtr, out GCHandle gch) where TManaged : Delegate
    {
        if (contextPtr == nint.Zero)
        {
            gch = default;
            return default;
        }
        gch = GCHandle.FromIntPtr(contextPtr);
        return (TManaged)gch.Target;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TNative Create<TManaged, TNative>(TManaged managedDel, TNative nativeDel, out GCHandle gch, out nint contextPtr)
        where TManaged : Delegate
        where TNative : Delegate
    {
        if (managedDel == null)
        {
            gch = default;
            contextPtr = nint.Zero;
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
