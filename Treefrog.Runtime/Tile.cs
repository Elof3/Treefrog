﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Runtime
{
    public class Tile
    {
        private Tileset _tileset;

        public Tile (int id, Tileset tileset, int x, int y)
        {
            Id = id;
            _tileset = tileset;
            Source = new Rectangle(x * tileset.TileWidth, y * tileset.TileHeight, tileset.TileWidth, tileset.TileHeight);
        }

        public int Id { get; private set; }

        public Tileset Tileset
        {
            get { return _tileset; }
        }

        public Rectangle Source { get; private set; }

        public PropertyCollection Properties { get; internal set; }

        public void Draw (SpriteBatch spriteBatch, Rectangle dest, float opacity, float layerDepth)
        {
            Draw(spriteBatch, dest, Color.White * opacity, layerDepth);
        }

        public void Draw (SpriteBatch spriteBatch, Rectangle dest, Color color, float layerDepth)
        {
            spriteBatch.Draw(_tileset.Texture, dest, Source, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        }
    }
}
