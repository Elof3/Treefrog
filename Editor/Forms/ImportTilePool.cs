﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Editor.Model;
using Microsoft.Xna.Framework.Graphics;
using Editor.Model.Controls;

namespace Editor.Forms
{
    public partial class ImportTilePool : Form
    {
        Project _project;

        LayerControl _layerControl;
        TileSetControlLayer _tileLayer;

        TileRegistry _localRegistry;
        Stream _fileStream;

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
            _layerControl.Alignment = LayerControlAlignment.UpperLeft;

            _tileLayer = new TileSetControlLayer(_layerControl);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;
            _tileLayer.ShouldRespondToInput = LayerCondition.Never;

            _previewPanel.Controls.Add(_layerControl);

            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(Handle, 128, 128);
            _localRegistry = new TileRegistry(gds.GraphicsDevice);

            _message.Text = "";
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
            TilePool pool = LoadFile(_project.Registry);
            if (pool != null) {
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
            return LoadFile(_localRegistry);
        }

        private void FileInfo ()
        {
            Texture2D source = Texture2D.FromStream(_localRegistry.GraphicsDevice, _fileStream);

            _width = source.Width;
            _height = source.Height;

            _fileStream.Position = 0;
        }

        private TilePool LoadFile (TileRegistry registry)
        {
            if (_fileStream == null) {
                return null;
            }

            if (_fileStream.Position != 0) {
                _fileStream.Position = 0;
            }

            _tileLayer.Layer = null;

            _localRegistry.Reset();

            TilePool preview = TilePool.Import(_textName.Text, registry, _fileStream, 
                (int)_numTileWidth.Value, (int)_numTileHeight.Value, 
                (int)_numXSpacing.Value, (int)_numYSpacing.Value,
                (int)_numXMargin.Value, (int)_numYMargin.Value);
            TileSet1D previewSet = TileSet1D.CreatePoolSet("Preview", preview);

            _tileLayer.Layer = new TileSetLayer("Preview", previewSet);

            // Update stats

            _countTilesHigh.Text = ((_height + (int)_numYSpacing.Value) / ((int)_numTileHeight.Value + (int)_numYSpacing.Value + (int)_numYMargin.Value)).ToString();
            _countTilesWide.Text = ((_width + (int)_numXSpacing.Value) / ((int)_numTileWidth.Value + (int)_numXSpacing.Value + (int)_numXMargin.Value)).ToString();
            _countUniqueTiles.Text = preview.Count.ToString();

            return preview;
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
    }
}
