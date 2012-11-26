﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Tools
{
    public class RubberBand2
    {
        public RubberBand2 ()
            : this(new Point(0, 0))
        {
        }

        public RubberBand2 (Point start)
        {
            Start = start;
            End = start;
        }

        public Point Start { get; set; }
        public Point End { get; set; }

        public Rectangle Selection
        {
            get
            {
                int sx = Math.Min(Start.X, End.X);
                int sy = Math.Min(Start.Y, End.Y);
                int ex = Math.Max(Start.X, End.X) + 1;
                int ey = Math.Max(Start.Y, End.Y) + 1;

                return new Rectangle(sx, sy, ex - sx, ey - sy);
            }
        }
    }
}
