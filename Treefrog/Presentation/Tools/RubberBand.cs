﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amphibian.Drawing;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;

namespace Treefrog.Presentation.Tools
{
    public class DrawEventArgs : EventArgs {
        public SpriteBatch SpriteBatch { get; private set; }

        public DrawEventArgs (SpriteBatch spriteBatch) {
            SpriteBatch = spriteBatch;
        }
    }

    public class RubberBand : IDisposable
    {
        private const int _drawOrder = 10;

        private LayerControl _control;
        //private ITileSource _source;

        private Brush _brush;
        private Pen _pen;

        private Point _start;
        private Point _end;

        private int _snapX;
        private int _snapY;

        private bool _visible;

        public RubberBand (LayerControl control, int snapX, int snapY)
        {
            _control = control;
            //_source = control.TileSource;

            _snapX = snapX;
            _snapY = snapY;

            _visible = true;
            AttachHandlers();
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(_start.X, _start.Y, _end.X - _start.X + 1, _end.Y - _start.Y + 1); }
        }

        protected bool Disposed { get; private set; }

        public int DrawOrder
        {
            get { return _drawOrder; }
        }

        public Brush Brush
        {
            get { return _brush; }
            set { _brush = value; }
        }

        public Pen Pen
        {
            get { return _pen; }
            set { _pen = value; }
        }

        protected virtual void AttachHandlers ()
        {
            _control.DrawExtra += DrawHandler;
        }

        protected virtual void DetachHandlers ()
        {
            _control.DrawExtra -= DrawHandler;
        }

        public void Start (Point start)
        {
            _start = start;
        }

        public void End (Point end)
        {
            _end = end;
        }

        public bool Visible
        {
            get { return _visible; }
            set {
                if (_visible != value) {
                    if (_visible) {
                        DetachHandlers();
                    }
                    else {
                        AttachHandlers();
                    }
                    _visible = value;
                }
            }
        }

        protected virtual void DrawHandler (object sender, DrawLayerEventArgs e)
        {
            if (_brush == null) {
                _brush = new SolidColorBrush(e.SpriteBatch.GraphicsDevice, new Color(.2f, .75f, 1f, .3f));
            }

            Rectangle region = _control.VisibleRegion;

            Vector2 offset = _control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * _control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * _control.Zoom);

            e.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            e.SpriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            int startx = Math.Min(_start.X, _end.X);
            int starty = Math.Min(_start.Y, _end.Y);
            int endx = Math.Max(_start.X, _end.X) + 1;
            int endy = Math.Max(_start.Y, _end.Y) + 1;

            Rectangle box = new Rectangle(
                (int)(startx * _snapX * _control.Zoom),
                (int)(starty * _snapY * _control.Zoom),
                (endx - startx) * (int)(_snapX * _control.Zoom),
                (endy - starty) * (int)(_snapY * _control.Zoom));

            if (_brush != null) {
                //Primitives2D.FillRectangle(e.SpriteBatch, box, _fillBrush);
                Draw2D.FillRectangle(e.SpriteBatch, box, _brush);
            }

            if (_pen != null) {
                //Primitives2D.DrawRectangle(e.SpriteBatch, box, _strokeBrush);
                Draw2D.DrawRectangle(e.SpriteBatch, box, _pen);
            }

            e.SpriteBatch.End();
        }

        #region IDisposable Members

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!this.Disposed) {
                if (disposing) {
                    if (Visible) {
                        DetachHandlers();
                    }
                }

                Disposed = true;
            }
        }

        ~RubberBand ()
        {
            Dispose(false);
        }

        #endregion
    }
}