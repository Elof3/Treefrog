﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Utility;

namespace Treefrog.Render.Layers
{
    public class WorkspaceRenderLayer : RenderLayer
    {
        private Texture2D _pattern;
        private Pen _borderPen;
        private Pen _guidePen;

        private List<PropertyChangedEventHandler> _modelHandlers;

        public WorkspaceRenderLayer (WorkspaceLayerPresenter model)
            : base(model)
        {
            Mode = RenderMode.Sprite | RenderMode.Drawing;

            _modelHandlers = new List<PropertyChangedEventHandler>() {
                model.SubscribeToChange(() => model.BorderColor, BorderColorChanged),
                model.SubscribeToChange(() => model.OriginGuideColor, OriginGuideColorChanged),
                model.SubscribeToChange(() => model.PatternColor1, PatternColorChanged),
                model.SubscribeToChange(() => model.PatternColor2, PatternColorChanged),
            };
        }

        protected override void DisposeManaged ()
        {
            if (_pattern != null)
                _pattern.Dispose();
            if (_borderPen != null)
                _borderPen.Dispose();
            if (_guidePen != null)
                _guidePen.Dispose();

            Model.UnsubscribeFromChange(_modelHandlers);

            base.DisposeManaged();
        }

        protected new WorkspaceLayerPresenter Model
        {
            get { return ModelCore as WorkspaceLayerPresenter; }
        }

        private void BorderColorChanged (object sender)
        {
            if (_borderPen != null)
                _borderPen.Dispose();
            _borderPen = null;
        }

        private void OriginGuideColorChanged (object sender)
        {
            if (_guidePen != null)
                _guidePen.Dispose();
            _guidePen = null;
        }

        private void PatternColorChanged (object sender)
        {
            if (_pattern != null)
                _pattern.Dispose();
            _pattern = null;
        }

        protected override void RenderCore (SpriteBatch spriteBatch)
        {
            Scissor = true;
            Vector2 offset = BeginDraw(spriteBatch, SamplerState.PointWrap);
            RenderContent(spriteBatch);
            EndDraw(spriteBatch, offset);
        }

        protected override void RenderCore (DrawBatch drawBatch)
        {
            Scissor = false;
            base.RenderCore(drawBatch);
        }

        protected override void RenderContent (SpriteBatch spriteBatch)
        {
            if (_pattern == null)
                _pattern = BuildCanvasPattern(spriteBatch.GraphicsDevice);
            if (_borderPen == null)
                _borderPen = new Pen(new SolidColorBrush(Model.BorderColor.ToXnaColor()));
            if (_guidePen == null)
                _guidePen = new Pen(new SolidColorBrush(Model.OriginGuideColor.ToXnaColor()));

            ILevelGeometry geometry = LevelGeometry;
            Rectangle bounds = geometry.VisibleBounds.ToXnaRectangle();

            Rectangle dest = new Rectangle(
                (int)Math.Ceiling(bounds.X * geometry.ZoomFactor),
                (int)Math.Ceiling(bounds.Y * geometry.ZoomFactor),
                (int)(bounds.Width * geometry.ZoomFactor),
                (int)(bounds.Height * geometry.ZoomFactor)
                );

            spriteBatch.Draw(_pattern, dest, dest, Color.White);
        }

        protected override void RenderContent (DrawBatch drawBatch)
        {
            ILevelGeometry geometry = LevelGeometry;
            Rectangle levelBounds = geometry.LevelBounds.ToXnaRectangle();

            Rectangle bounds = new Rectangle(
                (int)Math.Ceiling(levelBounds.X * geometry.ZoomFactor),
                (int)Math.Ceiling(levelBounds.Y * geometry.ZoomFactor),
                (int)(levelBounds.Width * geometry.ZoomFactor),
                (int)(levelBounds.Height * geometry.ZoomFactor)
                );

            if (levelBounds.Left < 0 && levelBounds.Right > 0)
                drawBatch.DrawLine(Pens.Gray, new Vector2(0, bounds.Top), new Vector2(0, bounds.Bottom));
            if (levelBounds.Top < 0 && levelBounds.Bottom > 0)
                drawBatch.DrawLine(Pens.Gray, new Vector2(bounds.Left, 0), new Vector2(bounds.Right, 0));

            drawBatch.DrawRectangle(Pens.Black, bounds);
        }

        private Texture2D BuildCanvasPattern (GraphicsDevice device)
        {
            Color color1 = Model.PatternColor1.ToXnaColor();
            Color color2 = Model.PatternColor2.ToXnaColor();

            byte[] data = new byte[16 * 16 * 4];
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    int index1 = (y * 16 + x) * 4;
                    int index2 = (y * 16 + x + 8) * 4;
                    int index3 = ((y + 8) * 16 + x) * 4;
                    int index4 = ((y + 8) * 16 + x + 8) * 4;

                    SetPixel(data, index1, color1);
                    SetPixel(data, index2, color2);
                    SetPixel(data, index3, color2);
                    SetPixel(data, index4, color1);
                }
            }

            Texture2D tex = new Texture2D(device, 16, 16);
            tex.SetData<byte>(data);

            return tex;
        }

        private void SetPixel (byte[] data, int index, Color color)
        {
            data[index + 0] = color.R;
            data[index + 1] = color.G;
            data[index + 2] = color.B;
            data[index + 3] = color.A;
        }
    }
}
