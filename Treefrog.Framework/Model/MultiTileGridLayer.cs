﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace Treefrog.Framework.Model
{
    using Support;
    using Treefrog.Framework.Imaging;

    public class MultiTileGridLayer : TileGridLayer
    {
        #region Fields

        private TileStack[,] _tiles;

        #endregion

        #region Constructors

        public MultiTileGridLayer (string name, int tileWidth, int tileHeight, int tilesWide, int tilesHigh)
            : base(name, tileWidth, tileHeight, tilesWide, tilesHigh)
        {
            _tiles = new TileStack[tilesHigh, tilesWide];
        }

        public MultiTileGridLayer (string name, MultiTileGridLayer layer)
            : base(name, layer)
        {
            _tiles = new TileStack[layer.TilesHigh, layer.TilesWide];

            for (int y = 0; y < layer.TilesHigh; y++) {
                for (int x = 0; x < layer.TilesWide; x++) {
                    if (layer._tiles[y, x] != null) {
                        _tiles[y, x] = layer._tiles[y, x].Clone() as TileStack;
                    }
                }
            }
        }

        public MultiTileGridLayer (MultiTileGridLayerXmlProxy proxy, Level level)
            : this(proxy.Name, level.TileWidth, level.TileHeight, level.TilesWide, level.TilesHigh)
        {
            Opacity = proxy.Opacity;
            IsVisible = proxy.Visible;
            Level = level;

            foreach (TileStackXmlProxy tileProxy in proxy.Tiles) {
                string[] coords = tileProxy.At.Split(new char[] { ',' });
                string[] ids = tileProxy.Items.Split(new char[] { ',' });

                TilePoolManager manager = Level.Project.TilePoolManager;

                foreach (string id in ids) {
                    int tileId = Convert.ToInt32(id);

                    TilePool pool = manager.PoolFromTileId(tileId);
                    Tile tile = pool.GetTile(tileId);

                    AddTile(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]), tile);
                }
            }

            foreach (PropertyXmlProxy propertyProxy in proxy.Properties)
                CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
        }

        public static MultiTileGridLayerXmlProxy ToXmlProxy (MultiTileGridLayer layer)
        {
            if (layer == null)
                return null;

            List<TileStackXmlProxy> tiles = new List<TileStackXmlProxy>();
            foreach (LocatedTileStack ts in layer.TileStacks) {
                if (ts.Stack != null && ts.Stack.Count > 0) {
                    List<int> ids = new List<int>();
                    foreach (Tile tile in ts.Stack) {
                        ids.Add(tile.Id);
                    }
                    string idSet = String.Join(",", ids);

                    tiles.Add(new TileStackXmlProxy()
                    {
                        At = ts.X + "," + ts.Y,
                        Items = idSet,
                    });
                }
            }

            List<PropertyXmlProxy> props = new List<PropertyXmlProxy>();
            foreach (Property prop in layer.CustomProperties)
                props.Add(Property.ToXmlProxy(prop));

            return new MultiTileGridLayerXmlProxy()
            {
                Name = layer.Name,
                Opacity = layer.Opacity,
                Visible = layer.IsVisible,
                Tiles = tiles.Count > 0 ? tiles : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        #endregion

        #region Properties

        public TileStack this[TileCoord location]
        {
            get { return this[location.X, location.Y]; }
            set
            {
                this[location.X, location.Y] = value;
            }
        }

        public TileStack this[int x, int y]
        {
            get
            {
                CheckBoundsFail(x, y);
                return _tiles[y, x];
            }

            set
            {
                CheckBoundsFail(x, y);

                if (_tiles[y, x] != null) {
                    _tiles[y, x].Modified -= TileStackModifiedHandler;
                }

                if (value != null) {
                    _tiles[y, x] = new TileStack(value);
                    _tiles[y, x].Modified += TileStackModifiedHandler;
                }
                else {
                    _tiles[y, x] = null;
                }

                OnModified(EventArgs.Empty);
            }
        }

        #endregion

        #region Event Handlers

        private void TileStackModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        protected override void ResizeLayer (int newTilesWide, int newTilesHigh)
        {
            TileStack[,] newTiles = new TileStack[newTilesHigh, newTilesWide];
            int copyLimX = Math.Min(TilesWide, newTilesWide);
            int copyLimY = Math.Min(TilesHigh, newTilesHigh);

            for (int y = 0; y < copyLimY; y++) {
                for (int x = 0; x < copyLimX; x++) {
                    newTiles[y, x] = _tiles[y, x];
                }
            }

            _tiles = newTiles;
        }

        public override IEnumerable<LocatedTile> Tiles
        {
            get { return TilesAt(new Rectangle(0, 0, TilesWide, TilesHigh)); }
        }

        public override IEnumerable<LocatedTile> TilesAt (TileCoord location)
        {
            if (_tiles[location.Y, location.X] == null) {
                yield break;
            }

            foreach (Tile tile in _tiles[location.Y, location.X]) {
                yield return new LocatedTile(tile, location);
            }
        }

        public override IEnumerable<LocatedTile> TilesAt (Rectangle region)
        {
            int xs = Math.Max(region.X, 0);
            int ys = Math.Max(region.Y, 0);
            int xe = Math.Min(region.X + region.Width, TilesWide);
            int ye = Math.Min(region.Y + region.Height, TilesHigh);

            for (int y = ys; y < ye; y++) {
                for (int x = xs; x < xe; x++) {
                    if (_tiles[y, x] == null) {
                        continue;
                    }

                    foreach (Tile tile in _tiles[y, x]) {
                        yield return new LocatedTile(tile, x, y);
                    }
                }
            }
        }

        public IEnumerable<LocatedTileStack> TileStacks
        {
            get { return TileStacksAt(new Rectangle(0, 0, TilesWide, TilesHigh)); }
        }

        public TileStack TileStacksAt (TileCoord location)
        {
            return _tiles[location.Y, location.X];
        }

        public IEnumerable<LocatedTileStack> TileStacksAt (Rectangle region)
        {
            int xs = Math.Max(region.X, 0);
            int ys = Math.Max(region.Y, 0);
            int w = Math.Min(region.Width, TilesWide - region.X);
            int h = Math.Min(region.Height, TilesHigh - region.Y);

            for (int y = ys; y < ys + h; y++) {
                for (int x = xs; x < xs + w; x++) {
                    if (_tiles[y, x] != null) {
                        yield return new LocatedTileStack(_tiles[y, x], x, y);
                    }
                }
            }
        }

        public void AddTileStack (int x, int y, TileStack stack)
        {
            if (stack != null) {
                foreach (Tile t in stack) {
                    AddTile(x, y, t);
                }
            }
        }

        protected override void AddTileImpl (int x, int y, Tile tile)
        {
            if (_tiles[y, x] == null) {
                _tiles[y, x] = new TileStack();
                _tiles[y, x].Modified += TileStackModifiedHandler;
            }

            _tiles[y, x].Remove(tile);
            _tiles[y, x].Add(tile);
        }

        protected override void RemoveTileImpl (int x, int y, Tile tile)
        {
            if (_tiles[y, x] != null) {
                _tiles[y, x].Remove(tile);
            }
        }

        protected override void ClearTileImpl (int x, int y)
        {
            if (_tiles[y, x] != null) {
                _tiles[y, x].Clear();
            }
        }

        #region XML Import / Export

        public override void WriteXml (XmlWriter writer)
        {
            // <layer name="" type="multi">
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "tilemulti");

            if (Opacity < 1f) {
                writer.WriteAttributeString("opacity", Opacity.ToString("0.###"));
            }

            if (!IsVisible) {
                writer.WriteAttributeString("visible", "False");
            }

            writer.WriteStartElement("tiles");
            foreach (LocatedTileStack ts in TileStacks) {
                if (ts.Stack != null && ts.Stack.Count > 0) {
                    List<int> ids = new List<int>();
                    foreach (Tile tile in ts.Stack) {
                        ids.Add(tile.Id);
                    }
                    string idSet = String.Join(",", ids);

                    writer.WriteStartElement("tile");
                    writer.WriteAttributeString("at", ts.X + "," + ts.Y);
                    writer.WriteString(idSet);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();

            // <properties> [optional]
            if (CustomProperties.Count > 0) {
                writer.WriteStartElement("properties");

                foreach (Property property in CustomProperties) {
                    property.WriteXml(writer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        protected override bool ReadXmlElement (XmlReader reader, string name)
        {
            switch (name) {
                case "tiles":
                    ReadXmlTiles(reader);
                    return true;
            }

            return base.ReadXmlElement(reader, name);
        }

        private void ReadXmlTiles (XmlReader reader)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "tile":
                        AddTileFromXml(xmlr);
                        break;
                }
            });
        }

        private void AddTileFromXml (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "at",
            });

            string[] coords = attribs["at"].Split(new char[] { ',' });

            string idstr = reader.ReadString();
            string[] ids = idstr.Split(new char[] { ',' });

            TilePoolManager manager = Level.Project.TilePoolManager;

            foreach (string id in ids) {
                int tileId = Convert.ToInt32(id);

                TilePool pool = manager.PoolFromTileId(tileId);
                Tile tile = pool.GetTile(tileId);

                AddTile(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]), tile);
            }
        }

        #endregion

        #region ICloneable Members

        public override object Clone ()
        {
            return new MultiTileGridLayer(Name, this);
        }

        #endregion

        
    }

    
}
