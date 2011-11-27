﻿using System;
using System.Collections.Generic;
using System.Xml;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Model
{
    public class TileSetLayer : TileLayer
    {
        #region Fields

        TilePool _pool;

        private List<Tile> _index;

        #endregion

        #region Constructors

        public TileSetLayer (string name, TilePool pool)
            : base(name, pool.TileWidth, pool.TileHeight)
        {
            _pool = pool;
            SyncIndex();
        }

        public TileSetLayer (string name, TileSetLayer layer)
            : base(name, layer)
        {
            _pool = layer._pool;
            SyncIndex();
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return _pool.Count; }
        }

        public int Capacity
        {
            get { return _pool.Capacity; }
        }

        public Tile this[int index]
        {
            get
            {
                if (index < 0 || index >= _index.Count) {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _index[index];
            }
        }

        #endregion

        #region Public API

        public virtual IEnumerable<Tile> Tiles
        {
            get { return _pool; }
        }

        #endregion

        private void SyncIndex ()
        {
            _index = new List<Tile>();

            foreach (Tile t in _pool) {
                _index.Add(t);
            }
        }

        public override void WriteXml (XmlWriter writer)
        {
            // <layer name="" type="multi">
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "tileset");

            writer.WriteEndElement();
        }

        #region ICloneable Members

        public override object Clone ()
        {
            return new TileSetLayer(Name, this);
        }

        #endregion
    }
}
