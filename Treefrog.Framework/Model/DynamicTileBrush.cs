﻿using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public class DynamicTileBrush : TileBrush
    {
        private DynamicTileBrushClass _brushClass;
        private IList<TileProxy> _tiles;

        public DynamicTileBrush (string name, int tileWidth, int tileHeight, DynamicTileBrushClass brushClass)
            : base(name, tileWidth, tileHeight)
        {
            _brushClass = brushClass;
            _tiles = brushClass.CreateTileProxyList();
        }

        public DynamicTileBrushClass BrushClass
        {
            get { return _brushClass; }
        }

        public Tile PrimaryTile
        {
            get
            {
                int index = _brushClass.PrimaryTileIndex;
                if (index >= 0 && index < _tiles.Count && _tiles[index] != null)
                    return _tiles[index].Tile;
                else
                    return null;
            }
        }

        public override TextureResource MakePreview ()
        {
            Tile tile = PrimaryTile;
            if (tile == null)
                return null;

            return tile.Pool.GetTileTexture(tile.Id);
        }

        public override TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            return _brushClass.MakePreview(_tiles, TileWidth, TileHeight, maxWidth, maxHeight);
        }

        public bool IsMemberTile (LocatedTile tile)
        {
            foreach (TileProxy proxy in _tiles) {
                if (proxy.Tile != null && proxy.Tile.Id == tile.Tile.Id)
                    return true;
            }

            return false;
        }

        public bool ContainsMemberTile (IEnumerable<LocatedTile> tiles)
        {
            foreach (LocatedTile tile in tiles) {
                if (IsMemberTile(tile))
                    return true;
            }

            return false;
        }

        public void SetTile (int position, Tile tile)
        {
            if (position >= 0 && position < _tiles.Count)
                _tiles[position].Tile = tile;
        }

        public void SetTile (int x, int y, Tile tile)
        {
            int position = y * _brushClass.TemplateWidth + x;
            SetTile(position, tile);
        }

        public Tile GetTile (int position)
        {
            if (position >= 0 && position < _tiles.Count)
                return _tiles[position].Tile;
            else
                return null;
        }

        public LocatedTile GetLocatedTile (int position)
        {
            if (position >= 0 && position < _tiles.Count)
                return new LocatedTile(_tiles[position].Tile, position % _brushClass.TemplateWidth, position / _brushClass.TemplateWidth);
            else
                return new LocatedTile();
        }

        public override void ApplyBrush (TileGridLayer tileLayer, int x, int y)
        {
            List<LocatedTile> updatedTiles = new List<LocatedTile>();

            updatedTiles.Add(new LocatedTile(InnerApply(tileLayer, x, y), x, y));

            // Update valid neighboring tiles
            foreach (TileCoord coord in NeighborCoordSet(x, y)) {
                if (ContainsMemberTile(tileLayer.TilesAt(coord)))
                    updatedTiles.Add(new LocatedTile(InnerApply(tileLayer, coord.X, coord.Y), coord.X, coord.Y));
            }
        }

        private Tile InnerApply (TileGridLayer tileLayer, int x, int y)
        {
            TileCoord[] coordSet = NeighborCoordSet(x, y);
            List<int> neighbors = new List<int>();

            for (int i = 0; i < coordSet.Length; i++) {
                if (ContainsMemberTile(tileLayer.TilesAt(coordSet[i])))
                    neighbors.Add(i + 1);
            }

            List<LocatedTile> targetStack = new List<LocatedTile>();
            foreach (LocatedTile tile in tileLayer.TilesAt(new TileCoord(x, y)))
                targetStack.Add(tile);

            foreach (LocatedTile tile in targetStack) {
                if (IsMemberTile(tile))
                    tileLayer.RemoveTile(x, y, tile.Tile);
            }

            int index = _brushClass.ApplyRules(neighbors);
            Tile newTile = null;

            if (index >= 0 && index < _tiles.Count && _tiles[index].Tile != null)
                newTile = _tiles[index].Tile;

            if (newTile != null)
                tileLayer.AddTile(x, y, newTile);

            return newTile;
        }

        private static TileCoord[] NeighborCoordSet (int x, int y)
        {
            return new TileCoord[] {
                new TileCoord(x - 1, y - 1),
                new TileCoord(x, y - 1),
                new TileCoord(x + 1, y - 1),
                new TileCoord(x + 1, y),
                new TileCoord(x + 1, y + 1),
                new TileCoord(x, y + 1),
                new TileCoord(x - 1, y + 1),
                new TileCoord(x - 1, y),
            };
        }

        public static DynamicTileBrushXmlProxy ToXmlProxy (DynamicTileBrush brush)
        {
            if (brush == null)
                return null;

            List<TileBrushEntryXmlProxy> brushEntries = new List<TileBrushEntryXmlProxy>();
            for (int i = 0; i < brush.BrushClass.SlotCount; i++) {
                Tile tile = brush.GetTile(i);
                if (tile != null)
                    brushEntries.Add(new TileBrushEntryXmlProxy() {
                        Slot = i,
                        TileId = tile.Id,
                    });
            }

            return new DynamicTileBrushXmlProxy() {
                Id = brush.Id,
                Name = brush.Name,
                Type = brush.BrushClass.ClassName,
                TileWidth = brush.TileWidth,
                TileHeight = brush.TileHeight,
                BrushEntries = brushEntries,
            };
        }

        public static DynamicTileBrush FromXmlProxy (DynamicTileBrushXmlProxy proxy, TilePoolManager manager, DynamicTileBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            DynamicTileBrushClass brushClass = registry.Lookup(proxy.Type);
            if (brushClass == null)
                return null;

            DynamicTileBrush brush = new DynamicTileBrush(proxy.Name, proxy.TileWidth, proxy.TileHeight, brushClass);

            foreach (TileBrushEntryXmlProxy entry in proxy.BrushEntries) {
                TilePool pool = manager.PoolFromTileId(entry.TileId);
                if (pool == null)
                    continue;

                brush.SetTile(entry.Slot, pool.GetTile(entry.TileId));
            }

            return brush;
        }
    }
}
