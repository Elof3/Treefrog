﻿using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Model.Proxy;
using System.Text;
using System;

namespace Treefrog.Framework.Model
{
    public class StaticTileBrush : TileBrush
    {
        private Dictionary<TileCoord, TileStack> _tiles;

        private int _minX = int.MaxValue;
        private int _minY = int.MaxValue;
        private int _maxX = int.MinValue;
        private int _maxY = int.MinValue;

        public StaticTileBrush (string name, int tileWidth, int tileHeight)
            : base(name, tileWidth, tileHeight)
        {
            _tiles = new Dictionary<TileCoord, TileStack>();
        }

        public IEnumerable<LocatedTile> Tiles
        {
            get
            {
                foreach (var kv in _tiles) {
                    foreach (Tile tile in kv.Value)
                        yield return new LocatedTile(tile, kv.Key);
                }
            }
        }

        public void AddTile (TileCoord coord, Tile tile)
        {
            if (!_tiles.ContainsKey(coord))
                _tiles.Add(coord, new TileStack());

            _tiles[coord].Remove(tile);
            _tiles[coord].Add(tile);

            UpdateExtants(coord);
        }

        public void ClearTile (TileCoord coord)
        {
            _tiles.Remove(coord);
        }

        public void Normalize ()
        {
            if (_minX == 0 || _minY == 0)
                return;

            Dictionary<TileCoord, TileStack> adjustedTiles = new Dictionary<TileCoord, TileStack>();
            foreach (var kv in _tiles)
                adjustedTiles.Add(new TileCoord(kv.Key.X - _minX, kv.Key.Y - _minY), kv.Value);

            _tiles = adjustedTiles;

            ResetExtants();
        }

        public override void ApplyBrush (TileGridLayer tileLayer, int x, int y)
        {
            List<LocatedTile> updatedTiles = new List<LocatedTile>();

            foreach (LocatedTile tile in Tiles)
                tileLayer.AddTile(x + tile.X, y + tile.Y, tile.Tile);
        }

        public override TextureResource MakePreview ()
        {
            if (TilesWide <= 0 || TilesHigh <= 0)
                return null;

            TextureResource resource = new TextureResource(TilesWide * TileWidth, TilesHigh * TileHeight);
            foreach (LocatedTile tile in Tiles) {
                int x = (tile.X - _minX) * TileWidth;
                int y = (tile.Y - _minY) * TileHeight;
                if (tile.Tile != null)
                    resource.Set(tile.Tile.Pool.GetTileTexture(tile.Tile.Id), new Point(x, y));
            }

            return resource;
        }

        public override TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            TextureResource resource = MakePreview();

            if (resource.Width <= maxWidth && resource.Height <= maxHeight)
                return resource;

            double aspectRatio = resource.Width / resource.Height;
            double scale = (aspectRatio > 1)
                ? maxWidth / resource.Width : maxHeight / resource.Height;

            TextureResource preview = new TextureResource((int)(resource.Width * scale), (int)(resource.Height * scale));
            preview.Set(resource, Point.Zero);

            return preview;
        }

        public override int TilesHigh
        {
            get { return (_maxY >= _minY) ? _maxY - _minY + 1 : 0; }
        }

        public override int TilesWide
        {
            get { return (_maxX >= _minX) ? _maxX - _minX + 1 : 0; }
        }

        private void UpdateExtants (TileCoord coord)
        {
            if (coord.X < _minX)
                _minX = coord.X;
            if (coord.X > _maxX)
                _maxX = coord.X;
            if (coord.Y < _minY)
                _minY = coord.Y;
            if (coord.Y > _maxY)
                _maxY = coord.Y;
        }

        private void ResetExtants ()
        {
            _minX = int.MaxValue;
            _minY = int.MaxValue;
            _maxX = int.MinValue;
            _maxY = int.MinValue;

            foreach (TileCoord coord in _tiles.Keys)
                UpdateExtants(coord);
        }

        public static StaticTileBrushXmlProxy ToXmlProxy (StaticTileBrush brush)
        {
            if (brush == null)
                return null;

            List<TileStackXmlProxy> stacks = new List<TileStackXmlProxy>();
            foreach (var kv in brush._tiles) {
                List<string> tileIds = new List<string>();
                foreach (Tile tile in kv.Value)
                    tileIds.Add(tile.Id.ToString());

                stacks.Add(new TileStackXmlProxy() {
                    At = kv.Key.X + "," + kv.Key.Y,
                    Items = String.Join(",", tileIds.ToArray()),
                });
            }

            return new StaticTileBrushXmlProxy() {
                Id = brush.Id,
                Name = brush.Name,
                TileWidth = brush.TileWidth,
                TileHeight = brush.TileHeight,
                Tiles = stacks,
            };
        }

        public static StaticTileBrush FromXmlProxy (StaticTileBrushXmlProxy proxy, TilePoolManager manager)
        {
            if (proxy == null)
                return null;

            StaticTileBrush brush = new StaticTileBrush(proxy.Name, proxy.TileWidth, proxy.TileHeight);

            foreach (TileStackXmlProxy stack in proxy.Tiles) {
                string[] coord = stack.Items.Split(',');
                string[] tileIds = stack.Items.Split(',');

                int x = (coord.Length > 0) ? Convert.ToInt32(coord[0].Trim()) : 0;
                int y = (coord.Length > 1) ? Convert.ToInt32(coord[1].Trim()) : 0;

                foreach (string tileId in tileIds) {
                    int id = Convert.ToInt32(tileId.Trim());

                    TilePool pool = manager.PoolFromTileId(id);
                    if (pool == null)
                        continue;

                    brush.AddTile(new TileCoord(x, y), pool.GetTile(id));
                }
            }

            brush.Normalize();

            return brush;
        }
    }
}
