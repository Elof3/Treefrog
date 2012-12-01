﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Presentation;

namespace Treefrog.Windows
{
    public partial class LayerPane : UserControl
    {
        #region Fields

        private ILayerListPresenter _controller;

        #endregion

        #region Constructors

        public LayerPane ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.layer--plus.png"));
            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.layer--minus.png"));
            _buttonUp.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.arrow-090.png"));
            _buttonDown.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.arrow-270.png"));
            _buttonCopy.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.layers.png"));
            _buttonProperties.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.tags.png"));

            _menuNewTileLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.grid.png"));
            _menuNewObjectLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game.png"));

            // Wire events

            _menuNewTileLayer.Click += NewTileLayerClickedHandler;

            _buttonRemove.Click += RemoveLayerClickedHandler;
            _buttonCopy.Click += CloneLayerClickedHandler;
            _buttonDown.Click += MoveLayerDownClickedHandler;
            _buttonUp.Click += MoveLayerUpClickedHandler;
            _buttonProperties.Click += ShowPropertiesClickedHandler;

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
            _listControl.ItemChecked += ItemCheckedHandler;
        }

        #endregion

        public void BindController (ILayerListPresenter controller) {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncLayerActions -= SyncLayerActionsHandler;
                _controller.SyncLayerList -= SyncLayerListHandler;
                _controller.SyncLayerSelection -= SyncLayerSelectionHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncLayerActions += SyncLayerActionsHandler;
                _controller.SyncLayerList += SyncLayerListHandler;
                _controller.SyncLayerSelection += SyncLayerSelectionHandler;

                _controller.RefreshLayerList();
            }
            else {
                ResetComponent();
            }
        }

        #region Event Handlers

        public void NewTileLayerClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionAddLayer();
        }

        public void RemoveLayerClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionRemoveSelectedLayer();
        }

        public void CloneLayerClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionCloneSelectedLayer();
        }

        public void MoveLayerUpClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionMoveSelectedLayerUp();
        }

        public void MoveLayerDownClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionMoveSelectedLayerDown();
        }

        public void ShowPropertiesClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionShowSelectedLayerProperties();
        }

        private void SelectedItemChangedHandler (object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) {
                return;
            }

            if (_controller != null) {
                _controller.ActionSelectLayer(e.Item.Name);
            }
        }

        private void ItemCheckedHandler (object sender, ItemCheckedEventArgs e)
        {
            if (_controller != null) {
                _controller.ActionShowHideLayer(e.Item.Name, e.Item.Checked ? LayerVisibility.Show : LayerVisibility.Hide);
            }
        }

        public void SyncLayerActionsHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                _buttonAdd.Enabled = _controller.CanAddLayer;
                _buttonCopy.Enabled = _controller.CanCloneSelectedLayer;
                _buttonRemove.Enabled = _controller.CanRemoveSelectedLayer;
                _buttonUp.Enabled = _controller.CanMoveSelectedLayerUp;
                _buttonDown.Enabled = _controller.CanMoveSelectedLayerDown;
                _buttonProperties.Enabled = _controller.CanShowSelectedLayerProperties;
            }
        }

        public void SyncLayerListHandler (object sender, EventArgs e)
        {
            _listControl.ItemSelectionChanged -= SelectedItemChangedHandler;

            _listControl.Items.Clear();

            if (_controller != null) {
                Stack<ListViewItem> items = new Stack<ListViewItem>();

                foreach (Layer layer in _controller.LayerList) {
                    ListViewItem layerItem = new ListViewItem(layer.Name, 0) {
                        Name = layer.Name,
                        Checked = true,
                    };

                    if (layer is ObjectLayer)
                        layerItem.ImageIndex = 1;

                    if (layer == _controller.SelectedLayer) {
                        layerItem.Selected = true;
                    }

                    items.Push(layerItem);
                }

                while (items.Count > 0) {
                    _listControl.Items.Add(items.Pop());
                }
            }

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
        }

        public void SyncLayerSelectionHandler (object sender, EventArgs e)
        {
            _listControl.ItemSelectionChanged -= SelectedItemChangedHandler;

            foreach (ListViewItem item in _listControl.Items) {
                if (_controller.SelectedLayer == null || item.Name != _controller.SelectedLayer.Name) {
                    item.Selected = false;
                }
                else {
                    item.Selected = true;
                }
            }

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
        }

        #endregion

        private void ResetComponent ()
        {
            _listControl.Items.Clear();

            _buttonAdd.Enabled = false;
            _buttonCopy.Enabled = false;
            _buttonDown.Enabled = false;
            _buttonUp.Enabled = false;
            _buttonRemove.Enabled = false;
            _buttonProperties.Enabled = false;
        }
    }
}
