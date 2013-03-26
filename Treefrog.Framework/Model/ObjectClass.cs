﻿using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Collections;
using System.Collections.Generic;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public class SystemProperty : Attribute
    {

    }

    /* INamedResource, IPropertyProvider */
    public class ObjectClass : IResource, IKeyProvider<string>, INotifyPropertyChanged, IPropertyProvider
    {
        private static string[] _reservedPropertyNames = new string[] { "Name", /*"Width", "Height", "OriginX", "OriginY"*/ };

        private ObjectPool _pool;
        private Guid _uid;
        private string _name;
        private Guid _textureId;

        private bool _canRotate;
        private bool _canScale;

        private TextureResource _image;

        private Point _origin;
        private Rectangle _maskBounds;
        private Rectangle _imageBounds;

        private PropertyCollection _properties;
        private ObjectClassProperties _predefinedProperties;

        public ObjectClass (string name)
        {
            _uid = Guid.NewGuid();
            _name = name;
            _origin = Point.Zero;

            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new ObjectClass.ObjectClassProperties(this);
        }

        public ObjectClass (string name, TextureResource image)
            : this(name)
        {
            _image = image;
            _imageBounds = image.Bounds;
            _maskBounds = image.Bounds;
        }

        public ObjectClass (string name, TextureResource image, Rectangle maskBounds)
            : this(name, image)
        {
            _maskBounds = maskBounds;
        }

        public ObjectClass (string name, TextureResource image, Rectangle maskBounds, Point origin)
            : this(name, image, maskBounds)
        {
            _origin = origin;
        }

        public Guid Uid
        {
            get { return _uid; }
            internal set { _uid = value; }
        }

        public ObjectPool Pool
        {
            get { return _pool; }
            internal set { _pool = value; }
        }

        [SystemProperty]
        public bool CanRotate
        {
            get { return _canRotate; }
            set
            {
                if (_canRotate != value) {
                    _canRotate = value;
                    RaisePropertyChanged("CanRotate");
                }
            }
        }

        [SystemProperty]
        public bool CanScale
        {
            get { return _canScale; }
            set
            {
                if (_canScale != value) {
                    _canScale = value;
                    RaisePropertyChanged("CanScale");
                }
            }
        }

        public Rectangle ImageBounds
        {
            get { return _imageBounds; }
        }

        public Rectangle MaskBounds
        {
            get { return _maskBounds; }
            set {
                if (_maskBounds != value) {
                    _maskBounds = value;
                    RaisePropertyChanged("MaskBounds");
                }
            }
        }

        public Point Origin
        {
            get { return _origin; }
            set
            {
                if (_origin != value) {
                    _origin = value;
                    RaisePropertyChanged("Origin");
                }
            }
        }

        public Guid ImageId
        {
            get { return _textureId; }
        }

        public TextureResource Image
        {
            get { return _pool != null ? _pool.TexturePool.GetResource(_textureId) : null; }
            set
            {
                if (_image != value) {
                    if (_image == null)
                        _textureId = _pool.TexturePool.AddResource(value);
                    else if (value == null) {
                        _pool.TexturePool.RemoveResource(_textureId);
                        _textureId = Guid.Empty;
                    }
                    else {
                        _pool.TexturePool.RemoveResource(_textureId);
                        _textureId = _pool.TexturePool.AddResource(value);
                    }

                    _image = value;
                    if (_image == null) {
                        _imageBounds = Rectangle.Empty;
                        RaisePropertyChanged("ImageBounds");
                    }
                    else if (_imageBounds.Width != _image.Width || _imageBounds.Height != _image.Height) {
                        _imageBounds = _image.Bounds;
                        RaisePropertyChanged("ImageBounds");
                    }
                    RaisePropertyChanged("Image");
                }
            }
            /*set
            {
                if (_image != value) {
                    _image = value;

                    if (_imageBounds.Width != _image.Width || _imageBounds.Height != _image.Height) {
                        _imageBounds = _image.Bounds;
                        RaisePropertyChanged("ImageBounds");
                    }
                    RaisePropertyChanged("Image");
                }
            }*/
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
                OnPropertyProviderNameChanged(EventArgs.Empty);
                RaisePropertyChanged("Name");
            }

            return true;
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if (ev != null)
                ev(this, e);
        }

        /*#region INamedResource Members

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value) {
                    NameChangingEventArgs ea = new NameChangingEventArgs(_name, value);
                    OnNameChanging(ea);
                    if (ea.Cancel)
                        return;

                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                    OnPropertyProviderNameChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler<NameChangingEventArgs> NameChanging = (s, e) => { };

        public event EventHandler<NameChangedEventArgs> NameChanged = (s, e) => { };

        public event EventHandler Modified;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            NameChanging(this, e);
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            NameChanged(this, e);
        }

        protected virtual void OnModified (EventArgs e)
        {
            Modified(this, e);
        }

        #endregion*/


        #region IPropertyProvider Members

        private class ObjectClassProperties : PredefinedPropertyCollection
        {
            private ObjectClass _parent;

            public ObjectClassProperties (ObjectClass parent)
                : base(_reservedPropertyNames)
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield return _parent.LookupProperty("Name");
                //yield return _parent.LookupProperty("Width");
                //yield return _parent.LookupProperty("Height");
                //yield return _parent.LookupProperty("OriginX");
                //yield return _parent.LookupProperty("OriginY");
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public string PropertyProviderName
        {
            get { return _name; }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged;

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public Collections.PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public Collections.PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            switch (name) {
                case "Name":
                //case "Width":
                //case "Height":
                //case "OriginX":
                //case "OriginY":
                    return PropertyCategory.Predefined;
                default:
                    return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
            }
        }

        public Property LookupProperty (string name)
        {
            Property prop;

            switch (name) {
                case "Name":
                    prop = new StringProperty("Name", _name);
                    prop.ValueChanged += NamePropertyChangedHandler;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        /*private void CustomProperties_Modified (object sender, EventArgs e)
        {
            OnModified(e);
        }*/

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            SetName(property.Value);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void RaisePropertyChanged (string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
            OnModified(EventArgs.Empty);
        }

        #endregion

        public static LibraryX.ObjectClassX ToXmlProxyX (ObjectClass objClass)
        {
            if (objClass == null)
                return null;

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in objClass.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LibraryX.ObjectClassX() {
                Uid = objClass.Uid,
                Name = objClass._name,
                Texture = objClass._textureId,
                ImageBounds = objClass._imageBounds,
                MaskBounds = objClass._maskBounds,
                Origin = objClass._origin,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static ObjectClass FromXmlProxy (LibraryX.ObjectClassX proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            ObjectClass objClass = new ObjectClass(proxy.Name);
            objClass._uid = proxy.Uid;
            objClass._textureId = proxy.Texture;
            objClass._imageBounds = proxy.ImageBounds;
            objClass._maskBounds = proxy.MaskBounds;
            objClass._origin = proxy.Origin;

            objClass._image = texturePool.GetResource(objClass._textureId);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    objClass.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            return objClass;
        }
    }
}
