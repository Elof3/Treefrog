﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;

namespace Treefrog.Render.Layers
{
    public class GridRenderLayer : RenderLayer
    {
        private const int MaxBrushSize = 256;
        private const int MinRenderThreshold = 16;

        private Texture2D _tileGridBrush;
        //private Texture2D _tileGridBrushRight;
        //private Texture2D _tileGridBrushBottom;

        private int _tileBrushWidth;
        private int _tileBrushHeight;

        private float _effectiveZoom;
        private int _effectiveSpacingX;
        private int _effectiveSpacingY;
        private Treefrog.Framework.Imaging.Color _effectiveColor;

        public GridRenderLayer (GridLayerPresenter model)
            : base(model)
        { }

        protected override void DisposeManaged ()
        {
            DisposeTextures();
            base.DisposeManaged();
        }

        protected override void RenderContent (SpriteBatch spriteBatch)
        {
            if (Model == null || LevelGeometry == null)
                return;

            if (_tileGridBrush == null || ShouldRebuildTileBrush())
                BuildTileBrush(spriteBatch.GraphicsDevice);

            if (_effectiveSpacingX * _effectiveZoom < MinRenderThreshold ||
                _effectiveSpacingY * _effectiveZoom < MinRenderThreshold)
                return;

            Rectangle levelBounds = LevelGeometry.LevelBounds.ToXnaRectangle();
            Rectangle region = LevelGeometry.VisibleBounds.ToXnaRectangle();
            Rectangle tileRegion = new Rectangle(
                (int)Math.Floor(region.X * 1.0 / Model.GridSpacingX),
                (int)Math.Floor(region.Y * 1.0 / Model.GridSpacingY),
                (int)(region.Width + region.X % Model.GridSpacingX + Model.GridSpacingX * 3 - 1) / Model.GridSpacingX,
                (int)(region.Height + region.Y % Model.GridSpacingY + Model.GridSpacingY * 3 - 1) / Model.GridSpacingY
                );
            tileRegion.Width = Math.Min(tileRegion.Width, (int)Math.Ceiling(levelBounds.Width * 1.0 / Model.GridSpacingX));
            tileRegion.Height = Math.Min(tileRegion.Height, (int)Math.Ceiling(levelBounds.Height * 1.0 / Model.GridSpacingY));

            for (int x = tileRegion.Left; x < tileRegion.Right; x += _tileBrushWidth) {
                for (int y = tileRegion.Top; y < tileRegion.Bottom; y += _tileBrushHeight) {
                    Vector2 pos = new Vector2(x * Model.GridSpacingX * (float)ZoomFactor, y * Model.GridSpacingY * (float)ZoomFactor);
                    Rectangle sourceRect = new Rectangle(0, 0,
                        (int)(Math.Min(_tileBrushWidth, tileRegion.Right - x) * Model.GridSpacingX * ZoomFactor),
                        (int)(Math.Min(_tileBrushHeight, tileRegion.Bottom - y) * Model.GridSpacingY * ZoomFactor)
                        );

                    spriteBatch.Draw(_tileGridBrush, pos, sourceRect, Color.White);
                }
            }
        }

        protected new GridLayerPresenter Model
        {
            get { return ModelCore as GridLayerPresenter; }
        }

        private float ZoomFactor
        {
            get { return LevelGeometry != null ? LevelGeometry.ZoomFactor : 1f; }
        }

        private bool ShouldRebuildTileBrush ()
        {
            if (_effectiveZoom != ZoomFactor ||
                _effectiveSpacingX != Model.GridSpacingX ||
                _effectiveSpacingY != Model.GridSpacingY ||
                _effectiveColor != Model.GridColor)
                return true;

            return false;
        }

        private int CalcBrushDimension (int tileDim, int maxDim, double zoomFactor)
        {
            if (tileDim == 0 || zoomFactor == 0)
                return 0;

            int dim = 1;
            while ((dim + 1) * tileDim * zoomFactor <= maxDim) {
                dim++;
            }

            return dim;
        }

        private void BuildTileBrush (GraphicsDevice device)
        {
            DisposeTextures();

            _effectiveZoom = ZoomFactor;
            _effectiveSpacingX = Model.GridSpacingX;
            _effectiveSpacingY = Model.GridSpacingY;
            _effectiveColor = Model.GridColor;

            int tilesAcross = CalcBrushDimension(Model.GridSpacingX, MaxBrushSize, ZoomFactor);
            int tilesDown = CalcBrushDimension(Model.GridSpacingY, MaxBrushSize, ZoomFactor);

            _tileBrushWidth = tilesAcross;
            _tileBrushHeight = tilesDown;

            int zTileWidth = (int)(Model.GridSpacingX * ZoomFactor);
            int zTileHeight = (int)(Model.GridSpacingY * ZoomFactor);

            int x = (int)(tilesAcross * zTileWidth);
            int y = (int)(tilesDown * zTileHeight);

            Color[] colors = new Color[x * y];
            //Color[] right = new Color[1 * y];
            //Color[] bottom = new Color[x * 1];

            for (int h = 0; h < tilesDown; h++) {
                for (int i = 0; i < x; i++) {
                    if (i % 4 != 2) {
                        colors[h * x * zTileHeight + i] = Model.GridColor.ToXnaColor();
                        //bottom[i] = Model.GridColor.ToXnaColor();
                    }
                }
            }

            for (int w = 0; w < tilesAcross; w++) {
                for (int i = 0; i < y; i++) {
                    if (i % 4 != 2) {
                        colors[i * x + w * zTileWidth] = Model.GridColor.ToXnaColor();
                        //right[i] = _gridColor;
                    }
                }
            }

            _tileGridBrush = new Texture2D(device, x, y, false, SurfaceFormat.Color);
            _tileGridBrush.SetData(colors);

            //_tileGridBrushRight = new Texture2D(device, 1, y, false, SurfaceFormat.Color);
            //_tileGridBrushRight.SetData(right);

            //_tileGridBrushBottom = new Texture2D(device, x, 1, false, SurfaceFormat.Color);
            //_tileGridBrushBottom.SetData(bottom);
        }

        private void DisposeTextures ()
        {
            if (_tileGridBrush != null)
                _tileGridBrush.Dispose();
            //if (_tileGridBrushBottom != null)
            //    _tileGridBrushBottom.Dispose();
            //if (_tileGridBrushRight != null)
            //    _tileGridBrushRight.Dispose();

            _tileGridBrush = null;
            //_tileGridBrushBottom = null;
            //_tileGridBrushRight = null;
        }
    }
}
