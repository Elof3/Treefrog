﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.ViewModel.Commands;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.ViewModel.Annotations;
using System.Windows.Input;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model.Support;
using Treefrog.ViewModel.Layers;

namespace Treefrog.ViewModel.Tools
{
    public class TileSelectTool : TilePointerTool
    {
        private ObservableCollection<Annotation> _annots;
        private ITileSelectionLayer _selectLayer;

        public TileSelectTool (CommandHistory history, MultiTileGridLayer layer, ObservableCollection<Annotation> annots, ITileSelectionLayer selectLayer)
            : base(history, layer)
        {
            _annots = annots;
            _selectLayer = selectLayer;
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectTilesSequence(info, viewport);
                    break;
                case PointerEventType.Secondary:
                    DefloatSelection();
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectTilesSequence(info, viewport);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectTilesSequence(info, viewport);
                    break;
            }
        }

        protected override void AutoScrollTick (PointerEventInfo info, ViewportVM viewport)
        {
            switch (_action) {
                case UpdateAction.Box:
                    UpdateDragCommon(info, viewport);
                    break;
                case UpdateAction.Move:
                    UpdateMoveCommon(info, viewport);
                    break;
            }
        }

        private enum UpdateAction
        {
            None,
            Move,
            Box,
        }

        private UpdateAction _action;

        private void FloatSelection ()
        {
            _selectLayer.FloatSelection();
        }

        private void DefloatSelection ()
        {
            CompoundCommand command = new CompoundCommand();
            if (_selectLayer.TileSelection.Floating)
                command.AddCommand(new DefloatTileSelectionCommand(Layer, _selectLayer));
            command.AddCommand(new DeleteTileSelectionCommand(_selectLayer));
            History.Execute(command);
        }

        private void StartSelectTilesSequence (PointerEventInfo info, ViewportVM viewport)
        {
            bool controlKey = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool shiftKey = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            if (shiftKey) {
                StartDragAdd(info, viewport);
                return;
            }
            
            if (controlKey) {
                StartDragRemove(info, viewport);
                return;
            }

            TileCoord location = TileLocation(info);
            if (!_selectLayer.TileSelectionCoverageAt(location)) {
                StartDragNew(info, viewport);
            }
            else {
                StartMove(info, viewport);
            }
        }

        private void UpdateSelectTilesSequence (PointerEventInfo info, ViewportVM viewport)
        {
            switch (_action) {
                case UpdateAction.Move:
                    UpdateMove(info, viewport);
                    break;
                case UpdateAction.Box:
                    UpdateDrag(info, viewport);
                    break;
            }
        }

        private void EndSelectTilesSequence (PointerEventInfo info, ViewportVM viewport)
        {
            switch (_action) {
                case UpdateAction.Move:
                    EndMove(info, viewport);
                    break;
                case UpdateAction.Box:
                    EndDrag(info, viewport);
                    break;
            }
        }

        private Point _initialLocation;
        private TileCoord _initialOffset;

        #region Move Actions

        private void StartMove (PointerEventInfo info, ViewportVM viewport)
        {
            _initialLocation = new Point((int)info.X - Layer.TileWidth / 2, (int)info.Y - Layer.TileHeight / 2);
            _initialOffset = _selectLayer.TileSelectionOffset;

            if (!_selectLayer.TileSelection.Floating) {
                Command command = new FloatTileSelectionCommand(Layer, _selectLayer);
                History.Execute(command);
            }

            _action = UpdateAction.Move;

            StartAutoScroll(info, viewport);
        }

        private void UpdateMove (PointerEventInfo info, ViewportVM viewport)
        {
            UpdateMoveCommon(info, viewport);
            UpdateAutoScroll(info, viewport);
        }

        private void UpdateMoveCommon (PointerEventInfo info, ViewportVM viewport)
        {
            int diffx = (int)info.X - _initialLocation.X;
            int diffy = (int)info.Y - _initialLocation.Y;

            if (diffx == 0 && diffy == 0)
                return;

            int tileDiffX = _initialOffset.X + (int)Math.Floor((double)diffx / Layer.TileWidth);
            int tileDiffY = _initialOffset.Y + (int)Math.Floor((double)diffy / Layer.TileHeight);

            _selectLayer.SetSelectionOffset(new TileCoord(tileDiffX, tileDiffY));
        }

        private void EndMove (PointerEventInfo info, ViewportVM viewport)
        {
            Command command = new MoveTileSelectionCommand(_selectLayer, _initialOffset, _selectLayer.TileSelectionOffset);
            History.Execute(command);

            _action = UpdateAction.None;

            EndAutoScroll(info, viewport);
        }

        #endregion

        #region Select Box Actions

        private enum MergeAction
        {
            New,
            Add,
            Remove,
        }

        private RubberBand _band;
        private SelectionAnnot _selectionAnnot;
        private MergeAction _mergeAction;

        private void ResetSelection (CompoundCommand command)
        {
            if (_selectLayer.HasSelection) {
                if (_selectLayer.TileSelection.Floating)
                    command.AddCommand(new DefloatTileSelectionCommand(Layer, _selectLayer));
                command.AddCommand(new DeleteTileSelectionCommand(_selectLayer));
            }

            command.AddCommand(new CreateTileSelectionCommand(_selectLayer));
        }

        private void StartDragNew (PointerEventInfo info, ViewportVM viewport)
        {
            StartDrag(info, viewport, MergeAction.New);
        }

        private void StartDragAdd (PointerEventInfo info, ViewportVM viewport)
        {
            StartDrag(info, viewport, MergeAction.Add);
        }

        private void StartDragRemove (PointerEventInfo info, ViewportVM viewport)
        {
            if (!_selectLayer.HasSelection)
                return;

            StartDrag(info, viewport, MergeAction.Remove);
        }

        private void StartDrag (PointerEventInfo info, ViewportVM viewport, MergeAction action)
        {
            TileCoord location = TileLocation(info);

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _band = new RubberBand(new Point(location.X, location.Y));
            _selectionAnnot = new SelectionAnnot(new Point((int)info.X, (int)info.Y))
            {
                Fill = new SolidColorBrush(new Color(64, 96, 216, 96)),
            };

            _annots.Add(_selectionAnnot);
            _action = UpdateAction.Box;
            _mergeAction = action;

            StartAutoScroll(info, viewport);
        }

        private void UpdateDrag (PointerEventInfo info, ViewportVM viewport)
        {
            UpdateDragCommon(info, viewport);
            UpdateAutoScroll(info, viewport);
        }

        private void UpdateDragCommon (PointerEventInfo info, ViewportVM viewport)
        {
            TileCoord location = TileLocation(info);

            _band.End = new Point(location.X, location.Y);
            Rectangle selection = _band.Selection;

            _selectionAnnot.Start = new Point(selection.Left * Layer.TileWidth, selection.Top * Layer.TileHeight);
            _selectionAnnot.End = new Point(selection.Right * Layer.TileWidth, selection.Bottom * Layer.TileHeight);
        }

        private void EndDrag (PointerEventInfo info, ViewportVM viewport)
        {
            Rectangle selection = ClampSelection(_band.Selection);

            CompoundCommand command = new CompoundCommand();

            if (_mergeAction == MergeAction.New || !_selectLayer.HasSelection || _selectLayer.TileSelection.Floating)
                ResetSelection(command);

            switch (_mergeAction) {
                case MergeAction.New:
                case MergeAction.Add:
                        ModifyAddTileSelectionCommand addCommand = new ModifyAddTileSelectionCommand(_selectLayer);
                        addCommand.AddLocations(TileCoordsFromRegion(selection));
                        command.AddCommand(addCommand);
                    break;
                case MergeAction.Remove:
                        ModifyRemoveTileSelectionCommand removeCommand = new ModifyRemoveTileSelectionCommand(_selectLayer);
                        removeCommand.AddLocations(TileCoordsFromRegion(selection));
                        command.AddCommand(removeCommand);
                    break;
            }

            if (command.Count > 0)
                History.Execute(command);

            _annots.Remove(_selectionAnnot);
            _action = UpdateAction.None;

            EndAutoScroll(info, viewport);
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
            int x = Math.Max(rect.X, 0);
            int y = Math.Max(rect.Y, 0);

            return new Rectangle(x, y, 
                Math.Min(rect.Width, Layer.TilesWide - x),
                Math.Min(rect.Height, Layer.TilesHigh - y)
                );
        }
    }
}
