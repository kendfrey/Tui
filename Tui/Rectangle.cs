using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    /// <summary>
    /// Represents a rectangular area on a screen.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// Initializes a new instance of the Rectangle structure with the specified dimensions.
        /// </summary>
        /// <param name="x">The X coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The Y coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Rectangle(int x, int y, int width, int height) : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the X coordinate of the top-left corner of the rectangle.
        /// </summary>
        public int X
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Y coordinate of the top-left corner of the rectangle.
        /// </summary>
        public int Y
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the width of the rectangle.
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the rectangle.
        /// </summary>
        public int Height
        {
            get;
            private set;
        }
    }
}
