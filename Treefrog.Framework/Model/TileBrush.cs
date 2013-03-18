﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class TileBrushRegistry : TypeRegistry<TileBrush>
    {
        public TileBrushRegistry ()
        {
            Register("Static Brush", typeof(StaticTileBrush));
            Register("Dynamic Brush", typeof(DynamicTileBrush));
        }
    }

    public abstract class TileBrush : IKeyProvider<string>
    {
        private Guid _id;
        private string _name;

        private int _tileHeight;
        private int _tileWidth;

        protected TileBrush (string name, int tileWidth, int tileHeight)
        {
            _name = name;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        protected TileBrush (string name, int tileWidth, int tileHeight, Guid uid)
            : this(name, tileWidth, tileHeight)
        {
            _id = uid;
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public virtual int TilesHigh
        {
            get { return 1; }
        }

        public virtual int TilesWide
        {
            get { return 1; }
        }

        public Guid Uid
        {
            get { return _id; }
            set { _id = value; }
        }

        public string GetKey ()
        {
            return Name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool SetName (string name)
        {
            if (_name != name) {
                KeyProviderEventArgs<string> e = new KeyProviderEventArgs<string>(_name, name);
                try {
                    OnKeyChanging(e);
                }
                catch (KeyProviderException) {
                    return false;
                }

                _name = name;
                OnKeyChanged(e);
            }

            return true;
        }

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanging;

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanged;

        protected virtual void OnKeyChanging (KeyProviderEventArgs<string> e)
        {
            if (KeyChanging != null)
                KeyChanging(this, e);
        }

        protected virtual void OnKeyChanged (KeyProviderEventArgs<string> e)
        {
            if (KeyChanged != null)
                KeyChanged(this, e);
        }

        public abstract void ApplyBrush (TileGridLayer tileLayer, int x, int y);

        public abstract TextureResource MakePreview ();

        public abstract TextureResource MakePreview (int maxWidth, int maxHeight);

        public virtual void Normalize ()
        { }
    }
}
