﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Windows.Controllers;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;

namespace Treefrog.Windows
{
    public partial class LayerPane : UserControl
    {
        #region Fields

        private ILayerListPresenter _controller;

        private UICommandController _commandController;

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

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.LayerDelete, _buttonRemove },
                { CommandKey.LayerClone, _buttonCopy },
                { CommandKey.LayerProperties, _buttonProperties },
                { CommandKey.LayerMoveUp, _buttonUp },
                { CommandKey.LayerMoveDown, _buttonDown },
            });
            _commandController.MapMenuItems(new Dictionary<CommandKey, ToolStripMenuItem>() {
                { CommandKey.NewTileLayer, _menuNewTileLayer },
                { CommandKey.NewObjectLayer, _menuNewObjectLayer },
            });

            // Wire events

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
            _listControl.ItemChecked += ItemCheckedHandler;
        }

        #endregion

        public void BindController (ILayerListPresenter controller) {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncLayerList -= SyncLayerListHandler;
                _controller.SyncLayerSelection -= SyncLayerSelectionHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncLayerList += SyncLayerListHandler;
                _controller.SyncLayerSelection += SyncLayerSelectionHandler;

                _commandController.BindCommandManager(_controller.CommandManager);

                SyncLayerList();
                SyncLayerSelection();
            }
            else {
                _commandController.BindCommandManager(null);

                ResetComponent();
            }
        }

        #region Event Handlers

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

        private void SyncLayerList ()
        {
            _listControl.ItemSelectionChanged -= SelectedItemChangedHandler;

            _listControl.Items.Clear();

            if (_controller != null) {
                Stack<ListViewItem> items = new Stack<ListViewItem>();

                foreach (LevelLayerPresenter layer in _controller.LayerList) {
                    ListViewItem layerItem = new ListViewItem(layer.LayerName, 0) {
                        Name = layer.LayerName,
                        Checked = true,
                    };

                    if (layer is ObjectLayerPresenter)
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

        private void SyncLayerSelection ()
        {
            _listControl.ItemSelectionChanged -= SelectedItemChangedHandler;

            foreach (ListViewItem item in _listControl.Items) {
                if (_controller.SelectedLayer == null || item.Name != _controller.SelectedLayer.LayerName) {
                    item.Selected = false;
                }
                else {
                    item.Selected = true;
                }
            }

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
        }

        private void SyncLayerListHandler (object sender, EventArgs e)
        {
            SyncLayerList();
        }

        private void SyncLayerSelectionHandler (object sender, EventArgs e)
        {
            SyncLayerSelection();
        }

        #endregion

        private void ResetComponent ()
        {
            _listControl.Items.Clear();

            _buttonProperties.Enabled = false;
        }
    }
}
