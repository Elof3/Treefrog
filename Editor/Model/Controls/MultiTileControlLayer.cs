﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Editor.Model.Controls
{
    public class MultiTileControlLayer : TileControlLayer
    {
        #region Fields

        MultiTileGridLayer _layer;

        #endregion

        #region Constructors

        public MultiTileControlLayer (LayerControl control)
            : base(control)
        {
        }

        #endregion

        #region Properties

        public new MultiTileGridLayer Layer
        {
            get { return _layer; }
            set
            {
                if (_layer != null) {
                    _layer.LayerSizeChanged -= TileLayerSizeChangedHandler;
                }

                _layer = value;
                _layer.LayerSizeChanged += TileLayerSizeChangedHandler;

                base.Layer = value;

                OnVirutalSizeChanged(EventArgs.Empty);
            }
        }

        public override int VirtualHeight
        {
            get 
            {
                if (_layer == null) {
                    return 0;
                }
                return _layer.LayerHeight * _layer.TileHeight;
            }
        }

        public override int VirtualWidth
        {
            get 
            {
                if (_layer == null) {
                    return 0;
                } 
                return _layer.LayerWidth * _layer.TileWidth;
            }
        }

        #endregion

        #region Event Handlers

        private void TileLayerSizeChangedHandler (object sender, EventArgs e)
        {
            OnVirutalSizeChanged(EventArgs.Empty);
        }

        #endregion

        protected override void DrawTiles (SpriteBatch spriteBatch, Rectangle tileRegion)
        {
            base.DrawTiles(spriteBatch, tileRegion);

            foreach (LocatedTile locTile in _layer.TilesAt(tileRegion)) {
                Rectangle dest = new Rectangle(
                    locTile.X * (int)(_layer.TileWidth * Control.Zoom),
                    locTile.Y * (int)(_layer.TileHeight * Control.Zoom),
                    (int)(_layer.TileWidth * Control.Zoom),
                    (int)(_layer.TileHeight * Control.Zoom));
                locTile.Tile.Draw(spriteBatch, dest);
            }
        }

        protected override Func<int, int, bool> TileInRegionPredicate (Rectangle tileRegion)
        {
            return (int x, int y) =>
            {
                return (x >= tileRegion.X) && (x < tileRegion.X + tileRegion.Width) &&
                    (y >= tileRegion.Y) && (y < tileRegion.Y + tileRegion.Height);
            };
        }
    }
}
