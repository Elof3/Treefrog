﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;

namespace Treefrog.V2.ViewModel.Layers
{
    public struct DrawCommand
    {
        public string Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Rectangle DestRect { get; set; }
        public Color BlendColor { get; set; }
    }

    public abstract class RenderLayerVM : LayerVM
    {
        public abstract IEnumerable<DrawCommand> RenderCommands { get; }

        public virtual ObservableDictionary<string, TextureResource> TextureSource 
        { 
            get { return null; } 
        }

        public abstract double LayerWidth { get; }

        public abstract double LayerHeight { get; }
    }
}
