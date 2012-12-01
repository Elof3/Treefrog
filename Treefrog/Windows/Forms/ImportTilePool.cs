﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework.Model;
using Treefrog.Model;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;

namespace Treefrog.Windows.Forms
{
    using XnaColor = Microsoft.Xna.Framework.Color;
    using Treefrog.Framework.Imaging;
    using Treefrog.Aux;
    

    public partial class ImportTilePool : Form
    {
        Project _project;

        LayerControl _layerControl;
        TileSetControlLayer _tileLayer;

        //TileRegistry _localRegistry;
        TilePoolManager _localManager;
        Stream _fileStream;

        private bool _useTransColor;
        private Color _transColor;

        int _width;
        int _height;

        public ImportTilePool (Project project)
        {
            InitializeComponent();

            _project = project;

            _buttonOK.Enabled = false;

            _layerControl = new LayerControl();
            _layerControl.Dock = DockStyle.Fill;
            _layerControl.WidthSynced = true;
            _layerControl.HeightSynced = true;
            _layerControl.Alignment = LayerControlAlignment.UpperLeft;
            _layerControl.ControlInitialized += LayerControlInitializedHandler;

            _tileLayer = new TileSetControlLayer(_layerControl);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;
            _tileLayer.ShouldRespondToInput = LayerCondition.Never;

            _previewPanel.Controls.Add(_layerControl);

            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(Handle, 128, 128);
            //_localRegistry = new TileRegistry(gds.GraphicsDevice);
            _localManager = new TilePoolManager();

            _message.Text = "";

            _buttonTransColor.Click += ButtonTransColorClickHandler;
            _checkboxTransColor.Click += CheckboxTransColorClickHandler;
            _layerControl.MouseDown += PreviewControlClickHandler;
        }

        private void LayerControlInitializedHandler (object sender, EventArgs e)
        {
            TilePoolTextureService poolService = new TilePoolTextureService(_localManager, _layerControl.GraphicsDeviceService);
            _layerControl.Services.AddService<TilePoolTextureService>(poolService);

            _layerControl.ControlInitialized -= LayerControlInitializedHandler;
        }

        private void _buttonBrowse_Click (object sender, EventArgs e)
        {
            if (_fileStream != null) {
                _fileStream.Close();
                _fileStream = null;
            }

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "Image Files (*.bmp,*.gif,*.jpg,*.png)|*.bmp;*.gif;*.jpg;*.jpeg;*.png|All files (*.*)|*.*";
            dlg.FilterIndex = 0;
            dlg.RestoreDirectory = true;
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    _fileStream = dlg.OpenFile();
                    if (_fileStream != null) {
                        _textPath.Text = dlg.FileName;

                        FileInfo();

                        LoadFile();
                        CheckValid();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            TilePool pool = LoadFile(_project.TilePoolManager);
            if (pool != null) {
                if (_checkboxTransColor.Checked) {
                    System.Drawing.Color c = _buttonTransColor.Color;
                    //pool.ApplyTransparentColor(new XnaColor(c.R / 255f, c.G / 255f, c.B / 255f));
                }
                _project.TilePools.Add(pool);
            }

            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            if (_fileStream != null) {
                _fileStream.Close();
            }
        }

        private TilePool LoadFile ()
        {
            return LoadFile(_localManager);
        }

        private void FileInfo ()
        {
            TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(_fileStream);

            _width = resource.Width;
            _height = resource.Height;

            _fileStream.Position = 0;
        }

        private TilePool _previewPool;
        private TextureResource _originalResource;

        private TilePool LoadFile (TilePoolManager manager)
        {
            if (_fileStream == null) {
                return null;
            }

            if (_fileStream.Position != 0) {
                _fileStream.Position = 0;
            }

            _tileLayer.Layer = null;

            _localManager.Reset();

            TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(_fileStream);
            TilePool.TileImportOptions options = new TilePool.TileImportOptions()
            {
                TileHeight = (int)_numTileHeight.Value,
                TileWidth = (int)_numTileWidth.Value,
                SpaceX = (int)_numXSpacing.Value,
                SpaceY = (int)_numYSpacing.Value,
                MarginX = (int)_numXMargin.Value,
                MarginY = (int)_numYMargin.Value,
                ImportPolicty = TileImportPolicy.SetUnique,
            };

            _previewPool = _localManager.ImportTilePool(_textName.Text, resource, options);
            /*TilePool preview = TilePool.Import(_textName.Text, manager, _fileStream, 
                (int)_numTileWidth.Value, (int)_numTileHeight.Value, 
                (int)_numXSpacing.Value, (int)_numYSpacing.Value,
                (int)_numXMargin.Value, (int)_numYMargin.Value);*/
            //TileSet1D previewSet = TileSet1D.CreatePoolSet("Preview", preview);

            _originalResource = _previewPool.TileSource.Crop(_previewPool.TileSource.Bounds);

            if (_useTransColor)
                SetTransparentColor();

            _tileLayer.Layer = new TileSetLayer("Preview", _previewPool);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;

            // Update stats

            _countTilesHigh.Text = ((_height + (int)_numYSpacing.Value) / ((int)_numTileHeight.Value + (int)_numYSpacing.Value + (int)_numYMargin.Value)).ToString();
            _countTilesWide.Text = ((_width + (int)_numXSpacing.Value) / ((int)_numTileWidth.Value + (int)_numXSpacing.Value + (int)_numXMargin.Value)).ToString();
            _countUniqueTiles.Text = _previewPool.Count.ToString();

            return _previewPool;
        }

        private void SetTransparentColor ()
        {
            if (_previewPool != null) {
                SetTransparentColor(_previewPool.TileSource);
                _previewPool.ReplaceTexture(_previewPool.TileSource);
            }
        }

        private void SetTransparentColor (TextureResource resource)
        {
            ClearTransparentColor(resource);
            resource.Apply(c => {
                if (c.Equals(_transColor))
                    return Colors.Transparent;
                else
                    return c;
            });
        }

        private void ClearTransparentColor ()
        {
            if (_previewPool != null) {
                ClearTransparentColor(_previewPool.TileSource);
                _previewPool.ReplaceTexture(_previewPool.TileSource);
            }
        }

        private void ClearTransparentColor (TextureResource resource)
        {
            resource.Set(_originalResource, Point.Zero);
        }

        private void ButtonTransColorClickHandler (object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog() {
                SolidColorOnly = true,
                Color = _buttonTransColor.Color,
                FullOpen = true,
            };

            DialogResult result = cd.ShowDialog(this);

            _buttonTransColor.Color = cd.Color;
            _transColor = new Color(cd.Color.R, cd.Color.G, cd.Color.B);

            if (_useTransColor)
                SetTransparentColor();
        }

        private void CheckboxTransColorClickHandler (object sender, EventArgs e)
        {
            _useTransColor = _checkboxTransColor.Checked;
            if (_useTransColor)
                SetTransparentColor();
            else
                ClearTransparentColor();
        }

        private void PreviewControlClickHandler (object sender, MouseEventArgs e)
        {
            XnaColor color = _tileLayer.Control.GetPixel(e.X, e.Y);

            _buttonTransColor.Color = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B);
            _transColor = new Color(color.R, color.G, color.B);

            if (_useTransColor)
                SetTransparentColor();
        }

        private void _numTileHeight_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numTileWidth_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numXSpacing_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numYSpacing_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numXMargin_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numYMargin_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _textName_TextChanged (object sender, EventArgs e)
        {
            CheckValid();
        }

        private void CheckValid ()
        {
            string txt = _textName.Text.Trim();
            if (txt.Length > 0 && !_project.TilePools.Contains(txt) && _fileStream != null) {
                _buttonOK.Enabled = true;
            }
            else {
                _buttonOK.Enabled = false;
            }

            if (_project.TilePools.Contains(txt)) {
                _message.Text = "A resouce with the given name already exists.";
            }
            else {
                _message.Text = "";
            }
        }

        private void TranspButtonPaintHandler (object sender, PaintEventArgs e)
        {
            
        }
    }
}
