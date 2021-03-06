﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Treefrog.Extensibility;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Treefrog.Plugins.Tiles.UI;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;

namespace Treefrog.Plugins.Tiles
{
    public class SyncTileBrushEventArgs : EventArgs
    {
        public TileBrush PreviousBrush { get; private set; }

        public SyncTileBrushEventArgs (TileBrush brush)
        {
            PreviousBrush = brush;
        }
    }

    public class TileBrushManagerPresenter : Presenter, ICommandSubscriber
    {
        private EditorPresenter _editor;

        private Guid _selectedBrush;
        private TileBrush _selectedBrushRef;

        public TileBrushManagerPresenter ()
        { }

        protected override void InitializeCore ()
        {
            OnAttach<EditorPresenter>(editor => {
                _editor = editor;
                _editor.SyncCurrentProject += SyncCurrentProjectHandler;
            });

            OnDetach<EditorPresenter>(editor => {
                _editor.SyncCurrentProject -= SyncCurrentProjectHandler;
                _editor = null;
            });

            InitializeCommandManager();
        }

        /*public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }*/

        /*protected virtual void Dispose (bool disposing)
        {
            if (_editor != null) {
                if (disposing) {
                    _editor.SyncCurrentProject -= SyncCurrentProjectHandler;
                }

                _editor = null;
            }
        }*/

        private void SyncCurrentProjectHandler (object sender, SyncProjectEventArgs e)
        {
            SelectBrush(Guid.Empty);

            OnSyncTileBrushManager(EventArgs.Empty);
            OnSyncTileBrushCollection(EventArgs.Empty);
        }

        #region Commands

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.NewStaticTileBrush, CommandCanCreateStaticBrush, CommandCreateStaticBrush);
            _commandManager.Register(CommandKey.NewDynamicTileBrush, CommandCanCreateDynamicBrush, CommandCreateDynamicBrush);
            _commandManager.Register(CommandKey.TileBrushClone, CommandCanCloneBrush, CommandCloneBrush);
            _commandManager.Register(CommandKey.TileBrushDelete, CommandCanDeleteBrush, CommandDeleteBrush);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanCreateStaticBrush ()
        {
            return TileBrushManager != null && TileBrushManager.StaticBrushCollections.Count() > 0;
        }

        private void CommandCreateStaticBrush ()
        {
            if (CommandCanCreateStaticBrush()) {
                using (StaticBrushForm form = new StaticBrushForm()) {
                    PresenterManager manager = new PresenterManager();
                    manager.Register<EditorPresenter>(_editor);

                    using (TilePoolListPresenter tilePoolList = new TilePoolListPresenter()) {
                        tilePoolList.Initialize(manager);
                        tilePoolList.BindTilePoolManager(_editor.Project.TilePoolManager);
                        form.BindTileController(tilePoolList);

                        foreach (TileBrush item in TileBrushManager.Brushes)
                            form.ReservedNames.Add(item.Name);

                        if (form.ShowDialog() == DialogResult.OK) {
                            if (TileBrushManager.StaticBrushCollections.Count(c => c.GetBrush(form.Brush.Uid) != null) == 0)
                                TileBrushManager.DefaultStaticBrushCollection.Brushes.Add(form.Brush);

                            OnSyncTileBrushCollection(EventArgs.Empty);
                            SelectBrush(form.Brush.Uid);
                            OnTileBrushSelected(EventArgs.Empty);
                        }
                    }
                }
            }
        }

        private bool CommandCanCreateDynamicBrush ()
        {
            return TileBrushManager != null && TileBrushManager.DynamicBrushCollections.Count() > 0;
        }

        private void CommandCreateDynamicBrush ()
        {
            if (CommandCanCreateDynamicBrush()) {
                using (DynamicBrushForm form = new DynamicBrushForm()) {
                    PresenterManager manager = new PresenterManager();
                    manager.Register<EditorPresenter>(_editor);

                    using (TilePoolListPresenter tilePoolList = new TilePoolListPresenter()) {
                        tilePoolList.Initialize(manager);
                        tilePoolList.BindTilePoolManager(_editor.Project.TilePoolManager);
                        form.BindTileController(tilePoolList);

                        foreach (TileBrush item in TileBrushManager.Brushes)
                            form.ReservedNames.Add(item.Name);

                        if (form.ShowDialog() == DialogResult.OK) {
                            if (TileBrushManager.DynamicBrushCollections.Count(c => c.GetBrush(form.Brush.Uid) != null) == 0)
                                TileBrushManager.DefaultDynamicBrushCollection.Brushes.Add(form.Brush);

                            OnSyncTileBrushCollection(EventArgs.Empty);
                            SelectBrush(form.Brush.Uid);
                            OnTileBrushSelected(EventArgs.Empty);
                        }
                    }
                }
            }
        }

        private bool CommandCanCloneBrush ()
        {
            return SelectedBrush != null;
        }

        private void CommandCloneBrush ()
        {
            if (CommandCanCloneBrush()) {
                string name = FindCloneBrushName(SelectedBrush.Name);

                Guid newBrushId = Guid.Empty;
                if (SelectedBrush is DynamicTileBrush) {
                    DynamicTileBrush oldBrush = SelectedBrush as DynamicTileBrush;
                    DynamicTileBrush newBrush = new DynamicTileBrush(name, oldBrush.TileWidth, oldBrush.TileHeight, oldBrush.BrushClass);
                    for (int i = 0; i < oldBrush.BrushClass.SlotCount; i++)
                        newBrush.SetTile(i, oldBrush.GetTile(i));

                    TileBrushManager.DefaultDynamicBrushCollection.Brushes.Add(newBrush);
                    newBrushId = newBrush.Uid;
                }
                else if (SelectedBrush is StaticTileBrush) {
                    StaticTileBrush oldBrush = SelectedBrush as StaticTileBrush;
                    StaticTileBrush newBrush = new StaticTileBrush(name, oldBrush.TileWidth, oldBrush.TileHeight);
                    foreach (LocatedTile tile in oldBrush.Tiles)
                        newBrush.AddTile(tile.Location, tile.Tile);
                    newBrush.Normalize();

                    TileBrushManager.DefaultStaticBrushCollection.Brushes.Add(newBrush);
                    newBrushId = newBrush.Uid;
                }
                else
                    return;

                OnSyncTileBrushCollection(EventArgs.Empty);
                SelectBrush(newBrushId);
                OnTileBrushSelected(EventArgs.Empty);
            }
        }

        private bool CommandCanDeleteBrush ()
        {
            return SelectedBrush != null;
        }

        private void CommandDeleteBrush ()
        {
            if (CommandCanDeleteBrush()) {
                if (SelectedBrush is DynamicTileBrush) {
                    foreach (var collection in TileBrushManager.DynamicBrushCollections)
                        collection.Brushes.Remove(SelectedBrush.Uid);
                }
                else if (SelectedBrush is StaticTileBrush) {
                    foreach (var collection in TileBrushManager.StaticBrushCollections)
                        collection.Brushes.Remove(SelectedBrush.Uid);
                }

                OnSyncTileBrushCollection(EventArgs.Empty);
                SelectBrush(Guid.Empty);
            }
        }

        #endregion

        public ITileBrushManager TileBrushManager
        {
            get { return _editor.Project.TileBrushManager; }
        }

        public TileBrush SelectedBrush
        {
            get { return _selectedBrushRef; }
        }

        public event EventHandler SyncTileBrushManager;

        public event EventHandler SyncTileBrushCollection;

        public event EventHandler<SyncTileBrushEventArgs> SyncCurrentBrush;

        public event EventHandler TileBrushSelected;

        protected virtual void OnSyncTileBrushManager (EventArgs e)
        {
            if (SyncTileBrushManager != null)
                SyncTileBrushManager(this, e);
        }

        protected virtual void OnSyncTileBrushCollection (EventArgs e)
        {
            if (SyncTileBrushCollection != null)
                SyncTileBrushCollection(this, e);
        }

        protected virtual void OnSyncCurrentBrush (SyncTileBrushEventArgs e)
        {
            CommandManager.Invalidate(CommandKey.TileBrushDelete);
            CommandManager.Invalidate(CommandKey.TileBrushClone);

            if (SyncCurrentBrush != null)
                SyncCurrentBrush(this, e);
        }

        protected virtual void OnTileBrushSelected (EventArgs e)
        {
            if (TileBrushSelected != null)
                TileBrushSelected(this, e);
        }

        public void ActionSelectBrush (Guid brushId)
        {
            SelectBrush(brushId);
            OnTileBrushSelected(EventArgs.Empty);
        }

        public void ActionEditBrush (Guid brushId)
        {
            TileBrush brush = TileBrushManager.GetBrush(brushId) as TileBrush;
            if (brush == null)
                return;

            if (brush is DynamicTileBrush) {
                using (DynamicBrushForm form = new DynamicBrushForm(brush as DynamicTileBrush)) {
                    PresenterManager manager = new PresenterManager();
                    manager.Register<EditorPresenter>(_editor);

                    using (TilePoolListPresenter tilePoolList = new TilePoolListPresenter()) {
                        tilePoolList.Initialize(manager);
                        tilePoolList.BindTilePoolManager(_editor.Project.TilePoolManager);
                        form.BindTileController(tilePoolList);

                        foreach (TileBrush item in TileBrushManager.Brushes)
                            if (item.Name != brush.Name)
                                form.ReservedNames.Add(item.Name);

                        if (form.ShowDialog() == DialogResult.OK) {
                            OnSyncTileBrushCollection(EventArgs.Empty);
                            SelectBrush(form.Brush.Uid);
                            OnTileBrushSelected(EventArgs.Empty);
                        }
                    }
                }
            }
            else if (brush is StaticTileBrush) {
                using (StaticBrushForm form = new StaticBrushForm(brush as StaticTileBrush)) {
                    PresenterManager manager = new PresenterManager();
                    manager.Register<EditorPresenter>(_editor);

                    using (TilePoolListPresenter tilePoolList = new TilePoolListPresenter()) {
                        tilePoolList.Initialize(manager);
                        tilePoolList.BindTilePoolManager(_editor.Project.TilePoolManager);
                        form.BindTileController(tilePoolList);

                        foreach (TileBrush item in TileBrushManager.Brushes)
                            if (item.Name != brush.Name)
                                form.ReservedNames.Add(item.Name);

                        if (form.ShowDialog() == DialogResult.OK) {
                            OnSyncTileBrushCollection(EventArgs.Empty);
                            SelectBrush(form.Brush.Uid);
                            OnTileBrushSelected(EventArgs.Empty);
                        }
                    }
                }
            }
        }

        public void RefreshTileBrushCollection ()
        {
            OnSyncTileBrushCollection(EventArgs.Empty);
        }

        private void SelectBrush (Guid brushId)
        {
            if (_selectedBrush == brushId)
                return;

            TileBrush prevBrush = _selectedBrushRef;
            TileBrush newBrush = TileBrushManager.GetBrush(brushId);

            _selectedBrushRef = newBrush;

            OnSyncCurrentBrush(new SyncTileBrushEventArgs(prevBrush));
        }

        private string FindCloneBrushName (string basename)
        {
            List<string> names = new List<string>();
            foreach (TileBrush brush in TileBrushManager.Brushes) {
                names.Add(brush.Name);
            }

            int i = 0;
            while (true) {
                string name = basename + " (" + ++i + ")";
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }
    }
}
