﻿using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    using Support;
    using Treefrog.Framework.Imaging;

    public class LocatedTileEventArgs : TileEventArgs 
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public LocatedTileEventArgs (Tile tile, int x, int y)
            : base(tile)
        {
            X = x;
            Y = y;
        }
    }

    public abstract class TileGridLayer : TileLayer
    {
        #region Fields

        private int _tileOriginX;
        private int _tileOriginY;
        private int _tilesWide;
        private int _tilesHigh;

        #endregion

        #region Constructors

        protected TileGridLayer (string name, int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(name, tileWidth, tileHeight)
        {
            _tileOriginX = 0;
            _tileOriginY = 0;
            _tilesWide = tilesWide;
            _tilesHigh = tilesHigh;
        }

        protected TileGridLayer (string name, int tileWidth, int tileHeight, Level level)
            : base(name, tileWidth, tileHeight)
        {
            _tileOriginX = (int)Math.Floor(level.OriginX * 1.0 / tileWidth);
            _tileOriginY = (int)Math.Floor(level.OriginY * 1.0 / tileHeight);

            int diffOriginX = level.OriginX - (_tileOriginX * tileWidth);
            int diffOriginY = level.OriginY - (_tileOriginY * tileHeight);

            _tilesWide = (int)Math.Ceiling((level.OriginX + level.Width + diffOriginX) * 1.0 / tileWidth) - _tileOriginX;
            _tilesHigh = (int)Math.Ceiling((level.OriginY + level.Height + diffOriginY) * 1.0 / tileHeight) - _tileOriginY;
        }

        protected TileGridLayer (string name, TileGridLayer layer)
            : base(name, layer)
        {
            _tileOriginX = layer._tileOriginX;
            _tileOriginY = layer._tileOriginY;
            _tilesHigh = layer._tilesHigh;
            _tilesWide = layer._tilesWide;
        }

        #endregion

        #region Properties

        public int TileOriginX
        {
            get { return _tileOriginX; }
        }

        public int TileOriginY
        {
            get { return _tileOriginY; }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
        }

        #endregion

        #region Public API

        public void AddTile (int x, int y, Tile tile)
        {
            CheckBoundsFail(x, y);
            CheckTileFail(tile);

            LocatedTileEventArgs ea = new LocatedTileEventArgs(tile, x, y);
            OnTileAdding(ea);
            AddTileImpl(x, y, tile);
            OnTileAdded(ea);
        }

        public void RemoveTile (int x, int y, Tile tile)
        {
            CheckBoundsFail(x, y);

            LocatedTileEventArgs ea = new LocatedTileEventArgs(tile, x, y);
            OnTileRemoving(ea);
            RemoveTileImpl(x, y, tile);
            OnTileRemoved(ea);
        }

        // NB: Consider changing event to give back the original TileStack
        public void ClearTile (int x, int y)
        {
            CheckBoundsFail(x, y);

            LocatedTileEventArgs ea = new LocatedTileEventArgs(null, x, y);
            OnTileClearing(ea);
            ClearTileImpl(x, y);
            OnTileCleared(ea);
        }

        public abstract IEnumerable<LocatedTile> Tiles { get; }

        public abstract IEnumerable<LocatedTile> TilesAt (TileCoord location);

        public abstract IEnumerable<LocatedTile> TilesAt (Rectangle region);

        public override int LayerOriginX
        {
            get { return _tileOriginX * TileWidth; }
        }

        public override int LayerOriginY
        {
            get { return _tileOriginY * TileHeight; }
        }

        public override int LayerHeight
        {
            get { return _tilesHigh * TileHeight; }
        }

        public override int LayerWidth
        {
            get { return _tilesWide * TileWidth; }
        }

        public override bool IsResizable
        {
            get { return true; }
        }

        public override void RequestNewSize (int originX, int originY, int pixelsWide, int pixelsHigh)
        {
            if (pixelsWide <= 0 || pixelsHigh <= 0) {
                throw new ArgumentException("New layer dimensions must be greater than 0.");
            }

            int tileX = (int)Math.Floor(originX * 1.0 / TileWidth);
            int tileY = (int)Math.Floor(originY * 1.0 / TileHeight);

            int tilesW = (int)Math.Ceiling((originX + pixelsWide) * 1.0 / TileWidth) - tileX;
            int tilesH = (int)Math.Ceiling((originY + pixelsHigh) * 1.0 / TileHeight) - tileY;

            if (tileX != _tileOriginX || tileY != _tileOriginY || tilesW != _tilesWide || tilesH != _tilesHigh) {
                ResizeLayer(tileX, tileY, tilesW, tilesH);
                OnLayerSizeChanged(EventArgs.Empty);
            }
        }

        public bool InRange (int x, int y)
        {
            return CheckBounds(x, y);
        }

        public bool InRange (TileCoord coord)
        {
            return CheckBounds(coord.X, coord.Y);
        }

        public bool InRange (LocatedTile tile)
        {
            return CheckBounds(tile.X, tile.Y);
        }

        #endregion

        #region Virtual Backing API

        protected abstract void AddTileImpl (int x, int y, Tile tile);

        protected abstract void RemoveTileImpl (int x, int y, Tile tile);

        protected abstract void ClearTileImpl (int x, int y);

        protected abstract void ResizeLayer (int newOriginX, int newOriginY, int newTilesWide, int newTilesHigh);

        #endregion

        public event EventHandler<LocatedTileEventArgs> TileAdding = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileRemoving = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileClearing = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileAdded = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileRemoved = (s, e) => { };

        public event EventHandler<LocatedTileEventArgs> TileCleared = (s, e) => { };

        protected virtual void OnTileAdding (LocatedTileEventArgs e)
        {
            TileAdding(this, e);
        }

        protected virtual void OnTileRemoving (LocatedTileEventArgs e)
        {
            TileRemoving(this, e);
        }

        protected virtual void OnTileClearing (LocatedTileEventArgs e)
        {
            TileClearing(this, e);
        }

        protected virtual void OnTileAdded (LocatedTileEventArgs e)
        {
            TileAdded(this, e);
            OnModified(e);
        }

        protected virtual void OnTileRemoved (LocatedTileEventArgs e)
        {
            TileRemoved(this, e);
            OnModified(e);
        }

        protected virtual void OnTileCleared (LocatedTileEventArgs e)
        {
            TileCleared(this, e);
            OnModified(e);
        }

        #region Checking Code

        protected void CheckTileFail (Tile tile)
        {
            if (!CheckTile(tile)) {
                throw new ArgumentException(String.Format("Tried to add tile with dimenions ({0}, {1}), layer expects tile dimensions ({2}, {3})",
                    new string[] { tile.Width.ToString(), tile.Height.ToString(), TileWidth.ToString(), TileHeight.ToString() }));
            }
        }

        protected void CheckBoundsFail (int x, int y)
        {
            if (!CheckBounds(x, y)) {
                throw new ArgumentOutOfRangeException(String.Format("Tried to add tile at ({0}, {1}), which is outside of layer dimensions ({2}, {3}),({4}, {5})",
                    new string[] { x.ToString(), y.ToString(), 
                        TileOriginX.ToString(), TileOriginY.ToString(),
                        (TilesWide - TileOriginX).ToString(), (TilesHigh - TileOriginY).ToString() }));
            }
        }

        protected bool CheckTile (Tile tile)
        {
            return tile.Width == TileWidth && tile.Height == TileHeight;
        }

        protected bool CheckBounds (int x, int y)
        {
            return x >= TileOriginX &&
                y >= TileOriginY &&
                x < TilesWide &&
                y < TilesHigh;
        }

        #endregion
    }
}
