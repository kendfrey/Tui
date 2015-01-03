using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    /// <summary>
    /// Represents a color in RGB color space.
    /// </summary>
    public struct ColorData
    {
        /// <summary>
        /// Gets the R channel value.
        /// </summary>
        public byte R
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the G channel value.
        /// </summary>
        public byte G
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the B channel value.
        /// </summary>
        public byte B
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the ColorData structure with the specified RGB channel values.
        /// </summary>
        /// <param name="r">The R channel value.</param>
        /// <param name="g">The G channel value.</param>
        /// <param name="b">The B channel value.</param>
        public ColorData(byte r, byte g, byte b) : this()
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
