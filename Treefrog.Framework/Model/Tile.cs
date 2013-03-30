﻿using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Collections;

namespace Treefrog.Framework.Model
{
    public abstract class Tile : IResource, IPropertyProvider
    {
        private readonly Guid _uid;

        private TilePool _pool;
        private PropertyCollection _properties;
        private TileProperties _predefinedProperties;

        protected List<DependentTile> _dependents;

        protected Tile ()
        {
            _uid = Guid.NewGuid();
            _dependents = new List<DependentTile>();

            _properties = new PropertyCollection(new string[0]);
            _predefinedProperties = new Tile.TileProperties(this);

            _properties.Modified += CustomProperties_Modified;
        }

        protected Tile (Guid uid)
            : this()
        {
            _uid = uid;
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public TilePool Pool
        {
            get { return _pool; }
            internal set { _pool = value; }
        }

        public int Height
        {
            get { return _pool.TileHeight; }
        }

        public int Width
        {
            get { return _pool.TileWidth; }
        }

        public event EventHandler TextureModified;

        protected virtual void OnTextureModified (EventArgs e)
        {
            var ev = TextureModified;
            if (ev != null)
                ev(this, e);

            OnModified(e);
        }

        public virtual void Update (TextureResource textureData)
        {
            foreach (DependentTile tile in _dependents)
                tile.UpdateFromBase(textureData);

            OnTextureModified(EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the internal state of the Layer is modified.
        /// </summary>
        public event EventHandler Modified;

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        private void CustomProperties_Modified (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #region IPropertyProvider Members

        private class TileProperties : PredefinedPropertyCollection
        {
            private Tile _parent;

            public TileProperties (Tile parent)
                : base(new string[0])
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield break;
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public string PropertyProviderName
        {
            get { return "Tile " + _uid; }
        }

        public PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
        }

        public PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
        }

        public Property LookupProperty (string name)
        {
            return _properties.Contains(name) ? _properties[name] : null;
        }

        //public void AddCustomProperty (Property property)
        //{
        //    if (property == null) {
        //        throw new ArgumentNullException("The property is null.");
        //    }
        //    if (_properties.Contains(property.Name)) {
        //        throw new ArgumentException("A property with the same name already exists.");
        //    }

        //    _properties.Add(property);
        //}

        //public void RemoveCustomProperty (string name)
        //{
        //    if (name == null) {
        //        throw new ArgumentNullException("The name is null.");
        //    }

        //    _properties.Remove(name);
        //}

        #endregion
    }

    public class PhysicalTile : Tile
    {
        public PhysicalTile ()
            : base()
        { }

        public PhysicalTile (Guid uid)
            : base(uid)
        { }

        public override void Update (TextureResource textureData)
        {
            Pool.Tiles.SetTileTexture(Uid, textureData);
            base.Update(textureData);
        }
    }

    public class DependentTile : Tile
    {
        Tile _base;
        TileTransform _transform;

        public DependentTile (Guid uid, Tile baseTile, TileTransform xform)
            : base(uid)
        {
            _base = baseTile;
            _transform = xform;
        }

        public override void Update (TextureResource textureData)
        {
            _base.Update(_transform.InverseTransform(textureData, Pool.TileWidth, Pool.TileHeight));
        }

        public virtual void UpdateFromBase (TextureResource textureData)
        {
            TextureResource xform = _transform.Transform(textureData, Pool.TileWidth, Pool.TileHeight);
            Pool.Tiles.SetTileTexture(Uid, xform);
        }
    }
}