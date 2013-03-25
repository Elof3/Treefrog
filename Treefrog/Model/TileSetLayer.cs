﻿using System;
using System.Collections.Generic;
using System.Xml;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Model
{
    public class TileSetLayer : TileLayer, IDisposable
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
            _pool.TileAdded += TileAdded;
            _pool.TileRemoved += TileRemoved;

            SyncIndex();
        }

        public TileSetLayer (string name, TileSetLayer layer)
            : base(name, layer)
        {
            _pool = layer._pool;
            _pool.TileAdded += TileAdded;
            _pool.TileRemoved += TileRemoved;

            SyncIndex();
        }

        #endregion

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (_pool != null) {
                if (disposing) {
                    _pool.TileAdded -= TileAdded;
                    _pool.TileRemoved -= TileRemoved;
                }

                _pool = null;
            }
        }

        #region Properties

        public int Count
        {
            get { return _pool.Tiles.Count; }
        }

        public int Capacity
        {
            get { return _pool.Tiles.Capacity; }
        }

        public Tile this[int index]
        {
            get
            {
                if (_index == null)
                    SyncIndex();

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
            get { return _pool.Tiles; }
        }

        #endregion

        private void SyncIndex ()
        {
            _index = new List<Tile>();

            foreach (Tile t in _pool.Tiles) {
                _index.Add(t);
            }
        }

        private void TileAdded (object sender, EventArgs e)
        {
            _index = null;
        }

        private void TileRemoved (object sender, EventArgs e)
        {
            _index = null;
        }

        /*
        public override void WriteXml (XmlWriter writer)
        {
            // <layer name="" type="multi">
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "tileset");

            writer.WriteEndElement();
        }
        */

        #region ICloneable Members

        public override object Clone ()
        {
            return new TileSetLayer(Name, this);
        }

        #endregion
    }
}
