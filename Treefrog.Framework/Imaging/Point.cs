﻿using System;

namespace Treefrog.Framework.Imaging
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point (int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point Zero = new Point(0, 0);
    }
}
