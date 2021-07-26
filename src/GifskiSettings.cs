using System.Runtime.InteropServices;

namespace GifskiNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GifskiSettings
    {

        /// <summary>
        /// Resize to max this width if non-0.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Resize to max this height if width is non-0. Note that aspect ratio is not preserved.
        /// </summary>
        public uint Height { get; set; }

        private byte _quality;

        /// <summary>
        /// 1-100, but useful range is 50-100. Recommended to set to 90.
        /// </summary>
        public byte Quality
        {
            get => _quality;
            set => _quality = value switch
            {
                > 100 => 100,
                _ => value
            };
        }

        /// <summary>
        /// Lower quality, but faster encode.
        /// </summary>
        public bool Fast { get; set; }

        /// <summary>
        /// If negative, looping is disabled. The number of times the sequence is repeated. 0 to loop forever.
        /// </summary>
        public ushort Repeat { get; set; }

    }
}
