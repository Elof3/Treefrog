﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Treefrog.Windows.Controls;
using TFImaging = Treefrog.Framework.Imaging;
using Treefrog.Framework;
using System.Collections.Generic;

namespace Treefrog.Presentation.Layers
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

        public MultiTileControlLayer (LayerControl control, MultiTileGridLayer layer)
            : base(control, layer)
        {
            Layer = layer;
        }

        public MultiTileControlLayer (LayerControl control, Layer layer)
            : this(control, layer as MultiTileGridLayer)
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
                if (_layer != null) {
                    _layer.LayerSizeChanged += TileLayerSizeChangedHandler;
                }

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
                return _layer.LayerHeight;
            }
        }

        public override int VirtualWidth
        {
            get 
            {
                if (_layer == null) {
                    return 0;
                } 
                return _layer.LayerWidth;
            }
        }

        #endregion

        #region Event Handlers

        private void TileLayerSizeChangedHandler (object sender, EventArgs e)
        {
            OnVirutalSizeChanged(EventArgs.Empty);
        }

        #endregion

        private RenderTarget2D _target;

        protected override void DrawTiles (SpriteBatch spriteBatch, TFImaging.Rectangle tileRegion)
        {
            base.DrawTiles(spriteBatch, tileRegion);

            TilePoolTextureService textureService = Control.Services.GetService<TilePoolTextureService>();
            if (textureService == null)
                return;

            //RenderTarget2D target = null;
            if (_layer.Opacity < 1f) {
                if (_target == null
                    || _target.GraphicsDevice != spriteBatch.GraphicsDevice
                    || _target.Width != spriteBatch.GraphicsDevice.Viewport.Width
                    || _target.Height != spriteBatch.GraphicsDevice.Viewport.Height) {
                    _target = new RenderTarget2D(spriteBatch.GraphicsDevice,
                        spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height,
                        false, SurfaceFormat.Color, DepthFormat.None);
                }

                spriteBatch.GraphicsDevice.SetRenderTarget(_target);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }

            Vector2 offset = BeginDraw(spriteBatch);

            foreach (LocatedTile locTile in _layer.TilesAt(tileRegion)) {
                Rectangle dest = new Rectangle(
                    locTile.X * (int)(_layer.TileWidth * Control.Zoom),
                    locTile.Y * (int)(_layer.TileHeight * Control.Zoom),
                    (int)(_layer.TileWidth * Control.Zoom),
                    (int)(_layer.TileHeight * Control.Zoom));
                //locTile.Tile.Pool(spriteBatch, dest);

                Tile tile = locTile.Tile;
                Texture2D poolTexture = textureService.GetTexture(locTile.Tile.Pool.Name);
                TileCoord tileLoc = locTile.Tile.Pool.GetTileLocation(locTile.Tile.Id);
                Rectangle srcRect = new Rectangle(tileLoc.X * tile.Width, tileLoc.Y * tile.Height, tile.Width, tile.Height);
                spriteBatch.Draw(poolTexture, dest, srcRect, Color.White);
            }

            if (TileSelection != null && TileSelection.Floating) {
                foreach (KeyValuePair<TileCoord, TileStack> item in TileSelection.Tiles) {
                    foreach (Tile tile in item.Value) {
                        TileCoord scoord = tile.Pool.GetTileLocation(tile.Id);
                        TileCoord dcoord = item.Key;

                        Texture2D poolTexture = textureService.GetTexture(tile.Pool.Name);
                        Rectangle srcRect = new Rectangle(scoord.X * tile.Width, scoord.Y * tile.Height, tile.Width, tile.Height);
                        Rectangle dstRect = new Rectangle(
                                (int)((dcoord.X + TileSelection.Offset.X) * tile.Width * Control.Zoom),
                                (int)((dcoord.Y + TileSelection.Offset.Y) * tile.Height * Control.Zoom),
                                (int)(tile.Width * Control.Zoom),
                                (int)(tile.Height * Control.Zoom));
                        spriteBatch.Draw(poolTexture, dstRect, srcRect, Color.White);
                    }
                }
            }

            EndDraw(spriteBatch);

            if (_layer.Opacity < 1f) {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDraw(spriteBatch);
                spriteBatch.Draw(_target, new Vector2(-offset.X, -offset.Y), new Color(1f, 1f, 1f, _layer.Opacity));
                EndDraw(spriteBatch);

                //target.Dispose();
            }
        }

        protected override Func<int, int, bool> TileInRegionPredicate (TFImaging.Rectangle tileRegion)
        {
            return (int x, int y) =>
            {
                return (x >= tileRegion.X) && (x < tileRegion.X + tileRegion.Width) &&
                    (y >= tileRegion.Y) && (y < tileRegion.Y + tileRegion.Height);
            };
        }
    }
}
