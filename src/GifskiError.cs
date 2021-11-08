namespace GifskiNet;

public enum GifskiError
{
    OK = 0,
    /** one of input arguments was NULL */
    NULL_ARG,
    /** a one-time function was called twice, or functions were called in wrong order */
    INVALID_STATE,
    /** internal error related to palette quantization */
    QUANT,
    /** internal error related to gif composing */
    GIF,
    /** internal error related to multithreading */
    THREAD_LOST,
    /** I/O error: file or directory not found */
    NOT_FOUND,
    /** I/O error: permission denied */
    PERMISSION_DENIED,
    /** I/O error: file already exists */
    ALREADY_EXISTS,
    /** invalid arguments passed to function */
    INVALID_INPUT,
    /** misc I/O error */
    TIMED_OUT,
    /** misc I/O error */
    WRITE_ZERO,
    /** misc I/O error */
    INTERRUPTED,
    /** misc I/O error */
    UNEXPECTED_EOF,
    /** progress callback returned 0, writing aborted */
    ABORTED,
    /** should not happen, file a bug */
    OTHER,
}
