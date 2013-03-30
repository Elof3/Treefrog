﻿using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Presentation
{
    public interface IPropertyListPresenter
    {
        bool CanAddProperty { get; }
        bool CanRemoveSelectedProperty { get; }
        bool CanRenameSelectedProperty { get; }
        bool CanEditSelectedProperty { get; }

        IPropertyProvider Provider { get; set; }
        string ProviderName { get; }
        IEnumerable<Property> PredefinedProperties { get; }
        IEnumerable<Property> CustomProperties { get; }
        Property SelectedProperty { get; }

        event EventHandler SyncPropertyContainer;
        event EventHandler SyncPropertyActions;
        event EventHandler SyncPropertyList;
        event EventHandler SyncPropertySelection;

        void ActionAddCustomProperty ();
        void ActionRemoveSelectedProperty ();
        void ActionRenameSelectedProperty (string name);
        void ActionEditSelectedProperty (string value);
        void ActionSelectProperty (string name);

        void RefreshPropertyList ();
    }

    public class PropertyListPresenter : IPropertyListPresenter
    {
        IPropertyProvider _provider;

        string _selectedProperty;

        public PropertyListPresenter ()
        {
        }

        public PropertyListPresenter (IPropertyProvider provider)
        {
            Provider = provider;
        }

        public IPropertyProvider Provider
        {
            get { return _provider; }
            set
            {
                if (_provider != null) {
                    _provider.PropertyProviderNameChanged -= PropertyProvider_PropertyProviderNameChanged;
                }

                _provider = value;
                _selectedProperty = null;

                if (value != null) {
                    _provider.PropertyProviderNameChanged += PropertyProvider_PropertyProviderNameChanged;
                }

                RefreshPropertyList();
            }
        }

        private void PropertyProvider_PropertyProviderNameChanged (object sender, EventArgs e)
        {
            OnSyncPropertyContainer(EventArgs.Empty);
        }

        #region IPropertyListPresenter Members

        #region Properties

        public bool CanAddProperty
        {
            get { return _provider != null; }
        }

        public bool CanRemoveSelectedProperty
        {
            get { return _provider != null && _provider.LookupPropertyCategory(_selectedProperty) == PropertyCategory.Custom; }
        }

        public bool CanRenameSelectedProperty
        {
            get { return _provider != null && _provider.LookupPropertyCategory(_selectedProperty) == PropertyCategory.Custom; }
        }

        public bool CanEditSelectedProperty
        {
            get { return _provider != null && _provider.LookupPropertyCategory(_selectedProperty) != PropertyCategory.None; }
        }

        public string ProviderName 
        {
            get 
            { 
                if (_provider == null) {
                    return "";
                }
                return _provider.PropertyProviderName;
            }
        }

        public IEnumerable<Property> PredefinedProperties
        {
            get 
            {
                if (_provider == null) {
                    yield break;
                }

                foreach (Property property in _provider.PredefinedProperties) {
                    yield return property;
                }
            }
        }

        public IEnumerable<Property> CustomProperties
        {
            get
            {
                if (_provider == null) {
                    yield break;
                }

                foreach (Property property in _provider.CustomProperties) {
                    yield return property;
                }
            }
        }

        public Property SelectedProperty
        {
            get
            {
                if (_provider != null) {
                    return _provider.LookupProperty(_selectedProperty);
                }
                return null;
            }
        }

        #endregion

        #region Events

        public event EventHandler SyncPropertyContainer;

        public event EventHandler SyncPropertyActions;

        public event EventHandler SyncPropertyList;

        public event EventHandler SyncPropertySelection;

        #endregion

        #region Event Dispatchers

        protected void OnSyncPropertyContainer (EventArgs e)
        {
            if (SyncPropertyContainer != null) {
                SyncPropertyContainer(this, e);
            }
        }

        protected void OnSyncPropertyActions (EventArgs e)
        {
            if (SyncPropertyActions != null) {
                SyncPropertyActions(this, e);
            }
        }

        protected void OnSyncPropertyList (EventArgs e)
        {
            if (SyncPropertyList != null) {
                SyncPropertyList(this, e);
            }
        }

        protected void OnSyncPropertySelection (EventArgs e)
        {
            if (SyncPropertySelection != null) {
                SyncPropertySelection(this, e);
            }
        }

        #endregion

        #region View Action API

        public void ActionAddCustomProperty ()
        {
            if (CanAddProperty) {
                Property property = new StringProperty(FindDefaultPropertyName(), "");
                _provider.CustomProperties.Add(property);

                _selectedProperty = property.Name;
            }

            OnSyncPropertyActions(EventArgs.Empty);
            OnSyncPropertyList(EventArgs.Empty);
        }

        public void ActionRemoveSelectedProperty ()
        {
            if (CanRemoveSelectedProperty) {
                _provider.CustomProperties.Remove(_selectedProperty);
            }

            _selectedProperty = null;

            OnSyncPropertyActions(EventArgs.Empty);
            OnSyncPropertyList(EventArgs.Empty);
        }

        public void ActionRenameSelectedProperty (string name)
        {
            if (CanRenameSelectedProperty && _provider.LookupProperty(name) == null) {
                Property property = _provider.LookupProperty(_selectedProperty);
                if (!property.TrySetName(name))
                    return;

                _selectedProperty = name;

                OnSyncPropertyActions(EventArgs.Empty);
                OnSyncPropertyList(EventArgs.Empty);
            }
        }

        public void ActionEditSelectedProperty (string value)
        {
            if (CanEditSelectedProperty) {
                Property property = _provider.LookupProperty(_selectedProperty);

                property.Parse(value);
                OnSyncPropertyList(EventArgs.Empty);
            }
        }

        public void ActionSelectProperty (string name)
        {
            if (_provider != null && _provider.LookupProperty(name) != null) {
                _selectedProperty = name;

                OnSyncPropertyActions(EventArgs.Empty);
                OnSyncPropertySelection(EventArgs.Empty);
            }
        }

        #endregion

        public void RefreshPropertyList ()
        {
            OnSyncPropertyContainer(EventArgs.Empty);
            OnSyncPropertyActions(EventArgs.Empty);
            OnSyncPropertyList(EventArgs.Empty);
        }

        private string FindDefaultPropertyName ()
        {
            List<string> names = new List<string>();
            foreach (Property property in _provider.PredefinedProperties) {
                names.Add(property.Name);
            }

            foreach (Property property in _provider.CustomProperties) {
                names.Add(property.Name);
            }

            int i = 0;
            while (true) {
                string name = "Property " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }

        #endregion
    }
}
