﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Runtime
{
    public class Tileset
    {
        private bool _disposed;
        private ContentManager _manager;

        private Dictionary<int, Tile> _tiles;
        private Texture2D _texture;

        internal Tileset ()
        {
            _tiles = new Dictionary<int, Tile>();
        }

        internal Tileset (ContentReader reader)
            : this()
        {
            _manager = reader.ContentManager;

            int version = reader.ReadInt32();
            int id = reader.ReadInt16();

            TileWidth = reader.ReadInt16();
            TileHeight = reader.ReadInt16();
            string texAsset = reader.ReadString();

            Properties = new PropertyCollection(reader);

            int tileCount = reader.ReadInt16();
            for (int i = 0; i < tileCount; i++) {
                int tileId = reader.ReadInt16();
                int tileX = reader.ReadInt16();
                int tileY = reader.ReadInt16();

                Tile tile = new Tile(tileId, this, tileX, tileY)
                {
                    Properties = new PropertyCollection(reader),
                };
                _tiles.Add(tileId, tile);
            }

            _texture = _manager.Load<Texture2D>(texAsset);
        }

        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        public PropertyCollection Properties { get; private set; }

        public Dictionary<int, Tile> Tiles
        {
            get { return _tiles; }
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }
    }
}
