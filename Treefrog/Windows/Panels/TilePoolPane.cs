﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Amphibian.Drawing;
using Treefrog.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;

namespace Treefrog.Windows
{
    using XColor = Microsoft.Xna.Framework.Color;
    using XRectangle = Microsoft.Xna.Framework.Rectangle;
    

    public partial class TilePoolPane : UserControl
    {
        #region Fields

        private ITilePoolListPresenter _controller;

        private TileSetControlLayer _tileLayer;

        private TileCoord _selectedTileCoord;

        private Amphibian.Drawing.Brush _selectBrush;

        #endregion

        #region Constructors

        public TilePoolPane ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.minus16.png"));
            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.plus16.png"));
            _buttonProperties.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.tags.png"));

            // Setup control

            _tileControl.BackColor = System.Drawing.Color.SlateGray;
            _tileControl.WidthSynced = true;
            _tileControl.Alignment = LayerControlAlignment.UpperLeft;

            _tileLayer = new TileSetControlLayer(_tileControl);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;
            _tileLayer.ShouldRespondToInput = LayerCondition.Always;

            // Wire events

            importNewToolStripMenuItem.Click += ImportTilePoolClickedHandler;
            _buttonRemove.Click += RemoveTilePoolClickedHandler;
            _buttonProperties.Click += ShowPropertiesClickedHandler;

            _poolComboBox.SelectedIndexChanged += SelectTilePoolHandler;

            _tileLayer.DrawExtraCallback += DrawSelectedTileIndicators;

            _tileLayer.MouseTileDown += TileControlMouseDownHandler;
            _tileLayer.VirtualSizeChanged += TileLayerResized;
        }

        #endregion

        public void BindController (ITilePoolListPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncTilePoolManager -= SyncTilePoolManagerHandler;
                _controller.SyncTilePoolActions -= SyncTilePoolActionsHandler;
                _controller.SyncTilePoolList -= SyncTilePoolListHandler;
                _controller.SyncTilePoolControl -= SyncTilePoolControlHandler;

                _tileControl.Services.RemoveService<TilePoolTextureService>();
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncTilePoolManager += SyncTilePoolManagerHandler;
                _controller.SyncTilePoolActions += SyncTilePoolActionsHandler;
                _controller.SyncTilePoolList += SyncTilePoolListHandler;
                _controller.SyncTilePoolControl += SyncTilePoolControlHandler;

                _controller.RefreshTilePoolList();

                TilePoolTextureService poolService = new TilePoolTextureService(_controller.TilePoolManager, _tileControl.GraphicsDeviceService);
                _tileControl.Services.AddService<TilePoolTextureService>(poolService);
            }
            else {
                ResetComponent();
            }
        }

        #region Event Dispatchers

        protected override void OnSizeChanged (EventArgs e)
        {
            base.OnSizeChanged(e);

            toolStrip1.CanOverflow = false;

            int width = toolStrip1.Width - _buttonAdd.Width - _buttonRemove.Width - _buttonProperties.Width - toolStripSeparator1.Width - toolStrip1.Padding.Horizontal - _buttonAdd.Margin.Horizontal - _buttonRemove.Margin.Horizontal - _buttonProperties.Margin.Horizontal - toolStripSeparator1.Margin.Horizontal - _poolComboBox.Margin.Horizontal - 1;
            _poolComboBox.Size = new Size(width, _poolComboBox.Height);
        }

        #endregion

        #region Event Handlers

        private void ImportTilePoolClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionImportTilePool();
        }

        private void RemoveTilePoolClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionRemoveSelectedTilePool();
        }

        private void ShowPropertiesClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionShowTilePoolProperties();
        }

        private void SelectTilePoolHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionSelectTilePool((string)_poolComboBox.SelectedItem);
        }

        private void TileControlMouseDownHandler (object sender, TileMouseEventArgs e)
        {
            if (_controller != null) {
                _controller.ActionSelectTile(e.Tile);
            }
        }

        private void SyncTilePoolManagerHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                TilePoolTextureService poolService = _tileControl.Services.GetService<TilePoolTextureService>();
                if (poolService != null) {
                    poolService.Dispose();
                    _tileControl.Services.RemoveService<TilePoolTextureService>();
                }

                poolService = new TilePoolTextureService(_controller.TilePoolManager, _tileControl.GraphicsDeviceService);
                _tileControl.Services.AddService<TilePoolTextureService>(poolService);
            }
        }

        private void SyncTilePoolActionsHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                _buttonAdd.Enabled = _controller.CanAddTilePool;
                _buttonRemove.Enabled = _controller.CanRemoveSelectedTilePool;
                _buttonProperties.Enabled = _controller.CanShowSelectedTilePoolProperties;
            }
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            _tileLayer.Layer = null;

            foreach (TilePool pool in _controller.TilePoolList) {
                _poolComboBox.Items.Add(pool.Name);

                if (pool == _controller.SelectedTilePool) {
                    _poolComboBox.SelectedItem = pool.Name;

                    _tileLayer.Layer = new TileSetLayer(pool.Name, pool);
                }
            }
        }

        private void SyncTilePoolControlHandler (object sender, EventArgs e)
        {
            Tile selected = _controller.SelectedTile;
            if (selected != null) {
                _selectedTileCoord = _tileLayer.TileToCoord(selected);
            }
        }

        private void TileLayerResized (object sender, EventArgs e)
        {
            if (_controller != null && _tileLayer.Layer != null) {
                Tile selected = _controller.SelectedTile;
                if (selected != null) {
                    _selectedTileCoord = _tileLayer.TileToCoord(selected);
                }
            }
        }

        #endregion

        private void DrawSelectedTileIndicators (object sender, DrawLayerEventArgs e)
        {
            if (_controller == null) {
                return;
            }

            Tile selectedTile = _controller.SelectedTile;
            if (_tileControl != null && _tileLayer != null && selectedTile != null) {
                if (_selectBrush == null) {
                    _selectBrush = _tileControl.CreateSolidColorBrush(new XColor(0.1f, 0.5f, 1f, 0.75f));
                }

                int x = _selectedTileCoord.X;
                int y = _selectedTileCoord.Y;

                Draw2D.FillRectangle(e.SpriteBatch, new XRectangle(
                    (int)(_selectedTileCoord.X * selectedTile.Width * _tileControl.Zoom),
                    (int)(_selectedTileCoord.Y * selectedTile.Height * _tileControl.Zoom),
                    (int)(selectedTile.Width * _tileControl.Zoom),
                    (int)(selectedTile.Height * _tileControl.Zoom)),
                    _selectBrush);
            }
        }

        private void ResetComponent ()
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            if (_tileLayer != null) {
                _tileLayer.Layer = null;
            }

            _buttonAdd.Enabled = false;
            _buttonRemove.Enabled = false;
            _buttonProperties.Enabled = false;
        }
    }
}
