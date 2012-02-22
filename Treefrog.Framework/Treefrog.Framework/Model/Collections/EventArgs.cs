﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework.Model.Collections
{
    public class PropertyEventArgs : EventArgs
    {
        public Property Property { get; private set; }

        public PropertyEventArgs (Property property)
        {
            Property = property;
        }
    }
}
