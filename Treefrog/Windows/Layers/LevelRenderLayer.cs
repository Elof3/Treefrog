﻿using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;

namespace Treefrog.Windows.Layers
{
    public class LevelRenderLayer : RenderLayer
    {
        public LevelRenderLayer (LevelLayerPresenter model)
            : base(model)
        { }

        protected new LevelLayerPresenter Model
        {
            get { return ModelCore as LevelLayerPresenter; }
        }

        protected override void RenderContent (SpriteBatch spriteBatch)
        {
            if (Model != null && TextureCache != null)
                RenderCommands(spriteBatch, TextureCache, Model.RenderCommands);
        }
    }
}
