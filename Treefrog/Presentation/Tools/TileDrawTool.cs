﻿using System;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Imaging;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;
using System.Collections.Generic;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Presentation.Tools
{
    public class TileDrawTool : TilePointerTool
    {
        private ObservableCollection<Annotation> _annots;

        private ImageAnnot _previewMarker;

        public TileDrawTool (CommandHistory history, MultiTileGridLayer layer, ObservableCollection<Annotation> annots)
            : base(history, layer)
        {
            _annots = annots;
        }

        public LevelInfoPresenter Info { get; set; }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartDrawPathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    StartDrag(info, viewport);
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateDrawPathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    UpdateDrag(info, viewport);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndDrawPathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    EndDrag(info, viewport);
                    break;
            }
        }

        protected override void PointerPositionCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            TileCoord location = TileLocation(info);
            if (!TileInRange(location)) {
                HidePreviewMarker();
                return;
            }

            ShowPreviewMarker(location);
        }

        protected override void PointerLeaveFieldCore ()
        {
            HidePreviewMarker();
        }

        protected override void SourceChangedCore (TileSourceType type)
        {
            ClearPreviewMarker();
            _activeBrush = null;

            if (type == TileSourceType.Brush)
                _activeBrush = ActiveBrush;

            if (Info != null) {
                if (type == TileSourceType.Tile && ActiveTile != null)
                    Info.InfoString = "Source: Tile " + ActiveTile.Uid;
                else if (type == TileSourceType.Brush && ActiveBrush != null)
                    Info.InfoString = "Source: Brush " + ActiveBrush.Name;
            }

        }

        #region Preview Marker

        private bool _previewMarkerVisible;
        private Tile _activeTile;
        private TileBrush _activeBrush;

        private void ShowPreviewMarker (TileCoord location)
        {
            if (ActiveTile == null && _activeBrush == null)
                return;

            if (_selectionAnnot != null)
                return;

            if (_activeBrush == null && ActiveTile != _activeTile) {
                ClearPreviewMarker();
                _activeTile = ActiveTile;
            }

            if (!_previewMarkerVisible) {
                if (_previewMarker == null) {
                    _previewMarker = new ImageAnnot();
                    _previewMarker.BlendColor = new Color(255, 255, 255, 128);
                    if (_activeBrush != null)
                        _previewMarker.Image = BuildBrushMarker();
                    else
                        _previewMarker.Image = BuildTileMarker();
                }

                _annots.Add(_previewMarker);
                _previewMarkerVisible = true;
            }

            if (_previewMarker != null) {
                int x = (int)(location.X * Layer.TileWidth);
                int y = (int)(location.Y * Layer.TileHeight);

                _previewMarker.Position = new Point(x, y);
                //if (_activeBrush != null)
                //    _previewMarker.End = new Point(x + Layer.TileWidth * _activeBrush.TilesWide, y + Layer.TileHeight * _activeBrush.TilesHigh);
                //else
                //    _previewMarker.End = new Point(x + Layer.TileWidth, y + Layer.TileHeight);
            }
            else {
                HidePreviewMarker();
            }
        }

        private TextureResource BuildTileMarker ()
        {
            return ActiveTile.Pool.Tiles.GetTileTexture(ActiveTile.Uid);
        }

        private TextureResource BuildBrushMarker ()
        {
            return _activeBrush.MakePreview();
        }

        private void HidePreviewMarker ()
        {
            _annots.Remove(_previewMarker);
            _previewMarkerVisible = false;
        }

        private void ClearPreviewMarker ()
        {
            HidePreviewMarker();
            _previewMarker = null;
        }

        #endregion

        #region Draw Path Sequence

        private TileReplace2DCommand _drawCommand;

        private void StartDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null && _activeBrush == null)
                return;

            _drawCommand = new TileReplace2DCommand(Layer);
        }

        private void UpdateDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null && _activeBrush == null)
                return;

            TileCoord location = TileLocation(info);
            if (!TileInRange(location))
                return;

            if (_activeBrush != null) {
                Layer.TileAdding += TileAddingHandler;
                Layer.TileRemoving += TileRemovingHandler;

                _activeBrush.ApplyBrush(Layer, location.X, location.Y);

                Layer.TileAdding -= TileAddingHandler;
                Layer.TileRemoving -= TileRemovingHandler;

                /*foreach (LocatedTile tile in _anonBrush.Tiles) {
                    TileCoord tileloc = new TileCoord(location.X + tile.X, location.Y + tile.Y);
                    if (Layer[tileloc] == null || Layer[tileloc].Top != tile.Tile) {
                        _drawCommand.QueueAdd(tileloc, tile.Tile);
                        Layer.AddTile(tileloc.X, tileloc.Y, tile.Tile);
                    }
                }*/
            }
            else if (Layer[location] == null || Layer[location].Top != ActiveTile) {
                if (Layer.CanAddTile(location.X, location.Y, ActiveTile)) {
                    _drawCommand.QueueAdd(location, ActiveTile);
                    Layer.AddTile(location.X, location.Y, ActiveTile);
                }
            }
        }
        
        private void EndDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null && _activeBrush == null)
                return;

            if (!_drawCommand.IsEmpty)
                History.Execute(_drawCommand);
        }

        private void TileAddingHandler (object sender, LocatedTileEventArgs e)
        {
            _drawCommand.QueueAdd(new TileCoord(e.X, e.Y), e.Tile);
        }

        private void TileRemovingHandler (object sender, LocatedTileEventArgs e)
        {
            _drawCommand.QueueRemove(new TileCoord(e.X, e.Y), e.Tile);
        }

        #endregion

        #region Select Region Sequence

        //private StaticTileBrush _anonBrush;

        private RubberBand _band;
        private SelectionAnnot _selectionAnnot;

        private void StartDrag (PointerEventInfo info, ILevelGeometry viewport)
        {
            ClearPreviewMarker();
            _activeBrush = null;

            TileCoord location = TileLocation(info);

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _band = new RubberBand(new Point(location.X, location.Y));
            _selectionAnnot = new SelectionAnnot(new Point((int)info.X, (int)info.Y)) {
                Fill = new SolidColorBrush(new Color(76, 178, 255, 128)),
            };

            _annots.Add(_selectionAnnot);

            //StartAutoScroll(info, viewport);
        }

        private void UpdateDrag (PointerEventInfo info, ILevelGeometry viewport)
        {
            TileCoord location = TileLocation(info);

            _band.End = new Point(location.X, location.Y);
            Rectangle selection = _band.Selection;

            _selectionAnnot.Start = new Point(selection.Left * Layer.TileWidth, selection.Top * Layer.TileHeight);
            _selectionAnnot.End = new Point(selection.Right * Layer.TileWidth, selection.Bottom * Layer.TileHeight);

            //UpdateAutoScroll(info, viewport);
        }

        private void EndDrag (PointerEventInfo info, ILevelGeometry viewport)
        {
            Rectangle selection = ClampSelection(_band.Selection);

            StaticTileBrush anonBrush = new StaticTileBrush("User Selected", Layer.TileWidth, Layer.TileHeight);
            foreach (LocatedTile tile in Layer.TilesAt(_band.Selection))
                anonBrush.AddTile(new TileCoord(tile.X - _band.Selection.Left, tile.Y - _band.Selection.Top), tile.Tile);

            anonBrush.Normalize();

            _activeBrush = anonBrush;

            _annots.Remove(_selectionAnnot);
            _selectionAnnot = null;

            //EndAutoScroll(info, viewport);
        }

        private IEnumerable<TileCoord> TileCoordsFromRegion (Rectangle region)
        {
            for (int y = region.Top; y < region.Bottom; y++)
                for (int x = region.Left; x < region.Right; x++)
                    yield return new TileCoord(x, y);
        }

        #endregion

        private Rectangle ClampSelection (Rectangle rect)
        {
            int x = Math.Max(rect.X, Layer.TileOriginX);
            int y = Math.Max(rect.Y, Layer.TileOriginY);

            return new Rectangle(x, y,
                Math.Min(rect.Width, Layer.TilesWide - x),
                Math.Min(rect.Height, Layer.TilesHigh - y)
                );
        }
    }
}
