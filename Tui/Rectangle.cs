using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    public struct Rectangle
    {
        public Rectangle(int x, int y, int width, int height) : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }
    }
}
