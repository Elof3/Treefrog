﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Layers;
using Treefrog.Presentation.Controllers;

namespace Treefrog.Windows.Controls.Composite
{
    class InfoStatus
    {
        private StatusStrip _statusBar;

        private ToolStripStatusLabel _statusCoord;
        private ToolStripStatusLabel _statusZoomIn;
        private ToolStripStatusLabel _statusZoomOut;
        private ToolStripStatusLabel _statusZoomText;
        private ToolStripStatusLabel _statusInfo;
        private ToolStripStatusLabel _statusLayer;

        private TrackBar _trackBarZoom;
        private ToolStripItem _trackBarZoomItem;

        private Image _imgMinusCircle;
        private Image _imgMinusCircleClk;
        private Image _imgMinusCircleMo;

        private Image _imgPlusCircle;
        private Image _imgPlusCircleClk;
        private Image _imgPlusCircleMo;

        private IContentInfoPresenter _controller;

        public InfoStatus (StatusStrip statusBar)
        {
            _statusBar = statusBar;
            _statusBar.Items.Clear();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _imgMinusCircle = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.minus-circle16.png"));
            _imgMinusCircleClk = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.minus-circle-clk16.png"));
            _imgMinusCircleMo = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.minus-circle-mo16.png"));

            _imgPlusCircle = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.plus-circle16.png"));
            _imgPlusCircleClk = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.plus-circle-clk16.png"));
            _imgPlusCircleMo = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.plus-circle-mo16.png"));

            // Coordinate

            _statusCoord = new ToolStripStatusLabel();
            _statusCoord.AutoSize = false;
            _statusCoord.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.status-loc.png"));
            _statusCoord.ImageScaling = ToolStripItemImageScaling.None;
            _statusCoord.ImageAlign = ContentAlignment.MiddleLeft;
            _statusCoord.Width = 80;
            _statusCoord.BorderSides = ToolStripStatusLabelBorderSides.Right;

            // Layer

            _statusLayer = new ToolStripStatusLabel();
            _statusLayer.AutoSize = true;
            _statusLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.grid.png"));
            _statusLayer.ImageScaling = ToolStripItemImageScaling.None;
            _statusLayer.ImageAlign = ContentAlignment.MiddleLeft;
            _statusLayer.BorderSides = ToolStripStatusLabelBorderSides.Right;
            _statusLayer.Margin = new Padding(6, 3, 0, 2);

            // Info

            _statusInfo = new ToolStripStatusLabel();
            _statusInfo.Spring = true;
            _statusInfo.BorderSides = ToolStripStatusLabelBorderSides.Right;

            // Statusbar Zoom

            _statusZoomIn = new ToolStripStatusLabel(_imgPlusCircle);
            _statusZoomIn.ImageScaling = ToolStripItemImageScaling.None;

            _statusZoomOut = new ToolStripStatusLabel(_imgMinusCircle);
            _statusZoomOut.ImageScaling = ToolStripItemImageScaling.None;

            _statusZoomText = new ToolStripStatusLabel("100%");
            _statusZoomText.AutoSize = false;
            _statusZoomText.Width = 35;
            _statusZoomText.Margin = new Padding(6, 3, 0, 2);
            _statusZoomText.TextAlign = ContentAlignment.MiddleRight;

            _trackBarZoom = new TrackBar();
            _trackBarZoom.AutoSize = false;
            _trackBarZoom.Height = 22;
            _trackBarZoom.TickStyle = TickStyle.None;
            _trackBarZoom.Anchor = AnchorStyles.Right;
            _trackBarZoom.Minimum = 0;
            _trackBarZoom.Maximum = 7;
            _trackBarZoom.Value = 2;
            _trackBarZoom.BackColor = SystemColors.ControlLightLight;

            _trackBarZoomItem = new ToolStripControlHost(_trackBarZoom);

            _statusZoomIn.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.plus-circle16.png"));
            _statusZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _statusZoomIn.Margin = new Padding(0, 0, 8, 0);

            _statusZoomOut.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.minus-circle16.png"));
            _statusZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;

            // Populate Status Strip

            _statusBar.Items.AddRange(new ToolStripItem[] {
                _statusCoord,
                _statusLayer,
                _statusInfo,
                _statusZoomText,
                _statusZoomOut,
                _trackBarZoomItem,
                _statusZoomIn,
            });

            // Wire Events

            _trackBarZoom.Scroll += trackBarZoom_Scroll;
            _trackBarZoom.ValueChanged += trackBarZoom_ValueChanged;

            _statusZoomOut.MouseEnter += buttonZoomOut_MouseEnter;
            _statusZoomOut.MouseLeave += buttonZoomOut_MouseLeave;
            _statusZoomOut.MouseDown += buttonZoomOut_MouseDown;
            _statusZoomOut.MouseUp += buttonZoomOut_MouseUp;

            _statusZoomIn.MouseEnter += buttonZoomIn_MouseEnter;
            _statusZoomIn.MouseLeave += buttonZoomIn_MouseLeave;
            _statusZoomIn.MouseDown += buttonZoomIn_MouseDown;
            _statusZoomIn.MouseUp += buttonZoomIn_MouseUp;

            ResetComponent();
        }

        public void BindController (IContentInfoPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncContentInfoActions -= SyncActionsHandler;
                _controller.SyncStatusInfo -= SyncStatusInfoHandler;
                _controller.SyncZoomLevel -= SyncZoomHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncContentInfoActions += SyncActionsHandler;
                _controller.SyncStatusInfo += SyncStatusInfoHandler;
                _controller.SyncZoomLevel += SyncZoomHandler;

                _controller.RefreshContentInfo();
            }
            else {
                ResetComponent();
            }
        }

        private void ResetComponent ()
        {
            _statusCoord.Text = "";
            _statusLayer.Text = "";
            _statusZoomText.Text = "100%";
            _trackBarZoom.Value = 2;

            _statusZoomText.Enabled = false;
            _trackBarZoom.Enabled = false;
            _statusZoomIn.Enabled = false;
            _statusZoomOut.Enabled = false;
        }

        private void SyncZoomHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                UpdateZoomState(_controller.Zoom);
            }
        }

        private void SyncActionsHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                _trackBarZoom.Enabled = _controller.CanZoom;
                _statusZoomIn.Enabled = _controller.CanZoom;
                _statusZoomOut.Enabled = _controller.CanZoom;
                _statusZoomText.Enabled = _controller.CanZoom;
            }
        }

        private void SyncStatusInfoHandler (object Sender, EventArgs e)
        {
            if (_controller != null) {
                _statusCoord.Text = _controller.CoordinateString;
                _statusInfo.Text = _controller.InfoString;

                _statusLayer.Text = (_controller.CurrentLayer != null)
                    ? _controller.CurrentLayer.LayerName : "No Layers";

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                if (_controller.CurrentLayer is TileLayerPresenter)
                    _statusLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.grid.png"));
                else if (_controller.CurrentLayer is ObjectLayerPresenter)
                    _statusLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game.png"));
                else
                    _statusLayer.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.selection.png"));
            }
        }

        // Zoom In

        private void buttonZoomIn_MouseEnter (object sender, EventArgs e)
        {
            _statusZoomIn.Image = _imgPlusCircleMo;
        }

        private void buttonZoomIn_MouseLeave (object sender, EventArgs e)
        {
            _statusZoomIn.Image = _imgPlusCircle;
        }

        private void buttonZoomIn_MouseDown (object sender, EventArgs e)
        {
            _statusZoomIn.Image = _imgPlusCircleClk;
        }

        private void buttonZoomIn_MouseUp (object sender, EventArgs e)
        {
            _statusZoomIn.Image = _imgPlusCircleMo;
            if (_trackBarZoom.Value < _trackBarZoom.Maximum) {
                _trackBarZoom.Value++;
            }
        }

        // Zoom Out

        private void buttonZoomOut_MouseEnter (object sender, EventArgs e)
        {
            _statusZoomOut.Image = _imgMinusCircleMo;
        }

        private void buttonZoomOut_MouseLeave (object sender, EventArgs e)
        {
            _statusZoomOut.Image = _imgMinusCircle;
        }

        private void buttonZoomOut_MouseDown (object sender, EventArgs e)
        {
            _statusZoomOut.Image = _imgMinusCircleClk;
        }

        private void buttonZoomOut_MouseUp (object sender, EventArgs e)
        {
            _statusZoomOut.Image = _imgMinusCircleMo;
            if (_trackBarZoom.Value > _trackBarZoom.Minimum) {
                _trackBarZoom.Value--;
            }
        }

        // Zoom Bar

        private void trackBarZoom_Scroll (object sender, EventArgs e)
        {

        }

        private void trackBarZoom_ValueChanged (object sender, EventArgs e)
        {
            if (_controller != null) {
                int index = _trackBarZoom.Value;
                if (index != _controller.Zoom.ZoomIndex) {
                    _controller.Zoom.ZoomIndex = index;
                    _statusZoomText.Text = _controller.Zoom.ZoomText;
                }
            }
        }

        private void UpdateZoomState (ZoomState zoom)
        {
            if (_trackBarZoom != null && _trackBarZoom.Value != zoom.ZoomIndex) {
                _trackBarZoom.Value = zoom.ZoomIndex;
                _statusZoomText.Text = zoom.ZoomText;
            }
        }
    }
}
