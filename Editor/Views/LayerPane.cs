﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model;
using System.Drawing.Drawing2D;

namespace Editor.Views
{
    public partial class LayerPane : UserControl
    {
        #region Fields

        Project _project;
        Level _level;

        Layer _currentLayer;

        #endregion

        #region Constructors

        public LayerPane ()
        {
            InitializeComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.layer--plus.png"));
            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.layer--minus.png"));
            _buttonUp.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.arrow-090.png"));
            _buttonDown.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.arrow-270.png"));
            _buttonCopy.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.layers.png"));

            _menuNewTileLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.grid.png"));
            _menuNewObjectLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.game.png"));

            _listControl.Items.Clear();
            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;

            _menuNewTileLayer.Click += NewTileLayerClickedHandler;

            _buttonRemove.Click += RemoveTileLayerClickedHandler;
            _buttonDown.Click += MoveDownTileLayerClickedHandler;
            _buttonUp.Click += MoveUpTileLayerClickedHandler;
        }

        public LayerPane (Project project, string level)
            : this()
        {
            SetupDefault(project, level);
        }

        #endregion

        #region Properties

        public Layer SelectedLayer
        {
            get { return _currentLayer; }
        }

        #endregion

        #region Events

        public event EventHandler SelectedLayerChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnSelectedLayerChanged (EventArgs e)
        {
            if (SelectedLayerChanged != null) {
                SelectedLayerChanged(this, e);
            }
        }

        #endregion

        #region Event Handlers

        private void NewTileLayerClickedHandler (object sender, EventArgs e)
        {
            string name = FindDefaultName();

            MultiTileGridLayer layer = new MultiTileGridLayer(name, _level.TileWidth, _level.TileHeight, _level.TilesWide, _level.TilesHigh);
            _level.Layers.Add(layer);

            ListViewItem layerItem = new ListViewItem(name, "grid.png")
            {
                Name = name,
                Checked = true,
                Selected = true,
            };

            _listControl.Items.Insert(0, layerItem);

            SelectLayer(name);
        }

        private void RemoveTileLayerClickedHandler (object sender, EventArgs e)
        {
            // Buffer items
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in _listControl.SelectedItems) {
                items.Add(item);
            }

            // Remove items
            foreach (ListViewItem item in items) {
                _listControl.Items.Remove(item);
                _level.Layers.Remove(item.Text);

                if (item.Selected) {
                    _currentLayer = null;
                }
            }

            if (_currentLayer == null && _listControl.Items.Count > 0) {
                _listControl.Items[0].Selected = true;
                //SelectLayer(_listControl.Items[0].Text);
            }

            UpdateToolbar();
        }

        private void MoveDownTileLayerClickedHandler (object sender, EventArgs e)
        {
            ListViewItem item = _listControl.Items[_currentLayer.Name];
            int index = item.Index;

            _listControl.Items.Remove(item);
            _listControl.Items.Insert(index + 1, item);

            _level.Layers.ChangeIndexRelative(_currentLayer.Name, -1);

            UpdateToolbar();
        }

        private void MoveUpTileLayerClickedHandler (object sender, EventArgs e)
        {
            ListViewItem item = _listControl.Items[_currentLayer.Name];
            int index = item.Index;

            _listControl.Items.Remove(item);
            _listControl.Items.Insert(index - 1, item);

            _level.Layers.ChangeIndexRelative(_currentLayer.Name, 1);

            UpdateToolbar();
        }

        private void SelectedItemChangedHandler (object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) {
                return;
            }

            SelectLayer(e.Item.Text);
        }

        #endregion

        public void SetupDefault (Project project, string level)
        {
            _project = project;
            _level = _project.Levels[level];

            UpdateToolbar();
        }

        private string FindDefaultName ()
        {
            List<string> names = new List<string>();
            foreach (ListViewItem item in _listControl.Items) {
                names.Add(item.Text);
            }

            int i = 0;
            while (true) {
                string name = "Tile Layer " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }

        private void SelectLayer (string name)
        {
            if (_currentLayer != null && _currentLayer.Name == name) {
                return;
            }

            if (!_level.Layers.Contains(name)) {
                throw new InvalidOperationException("Attempted to select a non-existent layer");
            }

            _currentLayer = _level.Layers[name];

            UpdateToolbar();
            OnSelectedLayerChanged(EventArgs.Empty);
        }

        private void UpdateToolbar ()
        {
            _buttonRemove.Enabled = (_listControl.Items.Count > 0);
            _buttonCopy.Enabled = (_listControl.Items.Count > 0);

            if (_currentLayer == null) {
                _buttonUp.Enabled = false;
                _buttonDown.Enabled = false;
            }
            else {
                _buttonDown.Enabled = (_listControl.Items[_currentLayer.Name].Index < _listControl.Items.Count - 1);
                _buttonUp.Enabled = (_listControl.Items[_currentLayer.Name].Index > 0);
            }
        }

        // Too painful to deal with for now
        /*private void listView1_DrawItem (object sender, DrawListViewItemEventArgs e)
        {
            if ((e.State & ListViewItemStates.Selected) != 0) {
                // Draw the background and focus rectangle for a selected item.
                e.Graphics.FillRectangle(Brushes.Maroon, e.Bounds);
                e.DrawFocusRectangle();
            }
            else {
                // Draw the background for an unselected item.
                using (SolidBrush brush = new SolidBrush(SystemColors.Highlight)) {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            // Draw the item text for views other than the Details view.
            if (_listControl.View != View.Details) {
                e.DrawText();
            }
        }*/
    }

    public class LayerListView : ListView
    {
        protected override void WndProc (ref Message m)
        {
            if (m.Msg >= 0x201 && m.Msg <= 0x209) {
                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                ListViewHitTestInfo hit = this.HitTest(pos);

                switch (hit.Location) {
                    case ListViewHitTestLocations.AboveClientArea:
                    case ListViewHitTestLocations.BelowClientArea:
                    case ListViewHitTestLocations.LeftOfClientArea:
                    case ListViewHitTestLocations.RightOfClientArea:
                    case ListViewHitTestLocations.None:
                        return;
                }
            }

            base.WndProc(ref m);
        }
    }
}
