using System;

namespace GifskiNet;

public sealed class GifskiException : Exception
{
    public GifskiException(string message, Exception innerException = null) : base(message, innerException) { }
}
