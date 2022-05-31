using System;

namespace SX3.Tools.D3DRender64.Controls.Misc
{
    public class SX3Color
    {
        #region Globals

        public enum ColorFormat { ARGB, RGBA };

        #endregion

        #region Properties

        public byte A;
        public byte R;
        public byte G;
        public byte B;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a color-object using the given values for its channels
        /// </summary>
        /// <param name="a">Value of the alpha-channel (0-255)</param>
        /// <param name="r">Value of the red-channel (0-255)</param>
        /// <param name="g">Value of the green-channel (0-255)</param>
        /// <param name="b">Value of the blue-channel (0-255)</param>
        public SX3Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Initializes a color-object using the given values for its channels
        /// </summary>
        /// <param name="a">Value of the alpha-channel (0-1)</param>
        /// <param name="r">Value of the red-channel (0-1)</param>
        /// <param name="g">Value of the green-channel (0-1)</param>
        /// <param name="b">Value of the blue-channel (0-1)</param>
        public SX3Color(float a, float r, float g, float b) : this((byte)(255f * a), (byte)(255f * r), (byte)(255f * g), (byte)(255f * b))
        { }

        public static SX3Color FromFormat(uint color, ColorFormat format)
        {
            switch (format)
            {
                case ColorFormat.ARGB:
                    return new SX3Color((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)(color));
                case ColorFormat.RGBA:
                    return new SX3Color((byte)(color & 0xFF), (byte)((color & 0xFF000000) >> 24), (byte)((color & 0x00FF0000) >> 16), (byte)((color & 0x0000FF00) >> 8));
                default:
                    throw new ArgumentException();
            }
        }

        #endregion

        #region Methods

        public uint ToARGB()
        {
            return (uint)B + ((uint)G << 8) + ((uint)R << 16) + ((uint)A << 24);
        }

        public uint ToRGBA()
        {
            return (uint)A + ((uint)B << 8) + ((uint)G << 16) + ((uint)R << 24);
        }

        #endregion
    }
}
