﻿using System;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Windows.Controls.Composite;
using System.Drawing;
using System.ComponentModel;
using Treefrog.Windows.Controllers;
using Treefrog.Presentation.Commands;
using System.Collections.Generic;

namespace Treefrog.Windows.Forms
{
    public partial class Main : Form
    {
        private StandardMenu _menu;
        private StandardToolbar _standardToolbar;
        private TileToolbar _tileToolbar;
        private InfoStatus _infoStatus;

        private UICommandController _commandController;

        private EditorPresenter _editor;

        public Main ()
        {
            InitializeComponent();

            // Toolbars

            _menu = new StandardMenu();

            _standardToolbar = new StandardToolbar();
            _tileToolbar = new TileToolbar();

            toolStripContainer1.TopToolStripPanel.Controls.AddRange(new Control[] {
                _standardToolbar.Strip, 
                _tileToolbar.Strip
            });

            Controls.Add(_menu.Strip);
            MainMenuStrip = _menu.Strip;

            _infoStatus = new InfoStatus(statusBar);

            _editor = new EditorPresenter();
            _editor.SyncContentTabs += SyncContentTabsHandler;
            _editor.SyncContentView += SyncContentViewHandler;
            _editor.SyncModified += SyncProjectModified;
            _editor.PanelActivation += PanelActivated;

            //_editor.CommandManager.Perform(Presentation.Commands.CommandKey.OpenProject);
            _editor.NewDefault();

            tabControlEx1.ContextMenuStrip = CommandMenuBuilder.BuildContextMenu(new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.LevelClose, CommandKey.LevelCloseAllOther,
                },
                new CommandMenuGroup() {
                    CommandKey.LevelRename, CommandKey.LevelResize,
                },
                new CommandMenuGroup() {
                    CommandKey.LevelProperties,
                },
            }));
            tabControlEx1.ContextMenuStrip.Opening += contextMenuStrip1_Opening;

            _commandController = new UICommandController();
            _commandController.BindCommandManager(_editor.CommandManager);
            _commandController.MapMenuItems(tabControlEx1.ContextMenuStrip.Items);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        }

        private void SyncContentTabsHandler (object sender, EventArgs e)
        {
            tabControlEx1.TabPages.Clear();

            foreach (LevelPresenter lp in _editor.OpenContent) {
                TabPage page = new TabPage(lp.Level.Name) {
                    Tag = lp.Level.Uid
                };
                tabControlEx1.TabPages.Add(page);

                LevelPanel lpanel = new LevelPanel();
                lpanel.BindController(lp);
                lpanel.Dock = DockStyle.Fill;

                page.Controls.Add(lpanel);
            }
        }

        private void SyncContentViewHandler (object sender, EventArgs e)
        {
            /*ILevelPresenter lp = _editor.CurrentLevel;

            if (lp != null) {
                foreach (TabPage page in tabControlEx1.TabPages) {
                    if (page.Text == lp.LayerControl.Name) {
                        tabControlEx1.SelectedTab = page;
                    }
                }
            }*/

            if (_editor.CanShowProjectPanel)
                projectPanel1.BindController(_editor);

            if (_editor.CanShowLayerPanel)
                layerPane1.BindController(_editor.Presentation.LayerList);

            if (_editor.CanShowTilePoolPanel)
                tilePoolPane1.BindController(_editor.Presentation.TilePoolList);

            if (_editor.CanShowObjectPoolPanel)
                objectPanel1.BindController(_editor.Presentation.ObjectPoolCollection);

            if (_editor.CanShowPropertyPanel)
                propertyPane1.BindController(_editor.Presentation.PropertyList);

            if (_editor.CanShowTileBrushPanel) {
                tileBrushPanel1.BindController(_editor.Presentation.TileBrushes);
                tileBrushPanel1.BindTileController(_editor.Presentation.TilePoolList);
            }

            _menu.BindController(_editor);
            _menu.BindCommandManager(_editor.CommandManager);
            //_tileToolbar.BindController(_editor.Presentation.LevelTools);
            if (_editor.CurrentLevel != null)
                _tileToolbar.BindCommandManager(_editor.CurrentLevel.CommandManager);
            _standardToolbar.BindCommandManager(_editor.CommandManager);
            _infoStatus.BindController(_editor.Presentation.ContentInfo);
        }

        private void SyncProjectModified (object sender, EventArgs e)
        {
            if (_editor.Modified) {
                base.Text = "Treefrog [*]";
            }
            else {
                base.Text = "Treefrog";
            }
        }

        private void PanelActivated (object sender, PanelEventArgs e)
        {
            if (e.PanelPresenter is IPropertyListPresenter) {
                TabControl control = _tabProperties.Parent as TabControl;
                if (control != null)
                    control.SelectedTab = _tabProperties;
            }
        }

        private void tabControlEx1_Selected (object sender, TabControlEventArgs e)
        {
            if (_editor != null) {
                if (e.TabPage != null)
                    _editor.ActionSelectContent((Guid)e.TabPage.Tag);
            }
        }

        private void contextMenuStrip1_Opening (object sender, CancelEventArgs e)
        {
            Point location = new Point(Cursor.Position.X + tabControlEx1.Location.X, Cursor.Position.Y + tabControlEx1.Location.Y);
            Point p = tabControlEx1.PointToClient(location);
            for (int i = 0; i < tabControlEx1.TabCount; i++) {
                Rectangle r = tabControlEx1.GetTabRect(i);
                if (r.Contains(p)) {
                    tabControlEx1.SelectedIndex = i; // i is the index of tab under cursor
                    return;
                }
            }
            e.Cancel = true;
        }
    }
}
