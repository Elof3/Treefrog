﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Editor.A.Presentation;
using Editor.Views;
using Editor.A.Controls.Composite;

namespace Editor
{
    public partial class Form1 : Form
    {
        private StandardMenu _menu;
        private StandardToolbar _standardToolbar;
        private TileToolbar _tileToolbar;
        private InfoStatus _infoStatus;

        private EditorPresenter _editor;

        public Form1 ()
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

            _editor.NewDefault();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            /*redoToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.arrow-turn.png"));
            undoToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.arrow-turn-180-left.png"));

            cutToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.scissors.png"));
            copyToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.documents.png"));
            pasteToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.clipboard-paste.png"));
            deleteToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.cross.png"));

            selectAllToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.selection-select.png"));
            selectNoneToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.selection.png"));*/
        }

        private void SyncContentTabsHandler (object sender, EventArgs e)
        {
            tabControlEx1.TabPages.Clear();

            foreach (ILevelPresenter lp in _editor.OpenContent) {
                TabPage page = new TabPage("Level");
                tabControlEx1.TabPages.Add(page);

                LevelPanel lpanel = new LevelPanel();
                lpanel.BindController(lp);
                lpanel.Dock = DockStyle.Fill;

                page.Controls.Add(lpanel);
            }
        }

        private void SyncContentViewHandler (object sender, EventArgs e)
        {
            ILevelPresenter lp = _editor.CurrentLevel;

            foreach (TabPage page in tabControlEx1.TabPages) {
                if (page.Text == lp.LayerControl.Name) {
                    tabControlEx1.SelectedTab = page;
                }
            }

            if (_editor.CanShowLayerPanel)
                layerPane1.BindController(_editor.Presentation.LayerList);

            if (_editor.CanShowTilePoolPanel)
                tilePoolPane1.BindController(_editor.Presentation.TilePoolList);

            if (_editor.CanShowPropertyPanel)
                propertyPane1.BindController(_editor.Presentation.PropertyList);

            _menu.BindController(_editor);
            _tileToolbar.BindController(_editor.Presentation.LevelTools);
            _standardToolbar.BindStandardToolsController(_editor.Presentation.StandardTools);
            _standardToolbar.BindDocumentToolsController(_editor.Presentation.DocumentTools);
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
    }
}
