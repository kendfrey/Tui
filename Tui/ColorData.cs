﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    public struct ColorData
    {
        public byte R
        {
            get;
            private set;
        }

        public byte G
        {
            get;
            private set;
        }

        public byte B
        {
            get;
            private set;
        }

        public ColorData(byte r, byte g, byte b) : this()
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
