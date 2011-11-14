﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Drawing;
using Editor.A.Presentation;

namespace Editor
{
    class StandardToolbar
    {
        private ToolStrip _strip;

        private ToolStripButton _tbNewProject;
        private ToolStripDropDownButton _tbNewItem;
        private ToolStripButton _tbOpen;
        private ToolStripButton _tbSave;

        private ToolStripButton _tbCut;
        private ToolStripButton _tbCopy;
        private ToolStripButton _tbPaste;

        private ToolStripButton _tbUndo;
        private ToolStripButton _tbRedo;

        private ToolStripButton _tbTileMode;
        private ToolStripButton _tbSetMode;
        private ToolStripButton _tbMapMode;

        private Assembly _assembly;

        private IStandardToolsPresenter _stdController;

        public StandardToolbar ()
        {
            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _tbNewProject = CreateButton("New Project (Ctrl+N)", "Editor.Icons._16.applications-blue--asterisk.png");
            _tbNewItem = CreateDropDownButton("Add New Item", "Editor.Icons._16.map--asterisk.png");
            _tbNewItem.DropDownItems.AddRange(new ToolStripItem[] {
                DropDownMenuItem("Add New Level", "Editor.Icons._16.map--asterisk.png"),
                DropDownMenuItem("Add New Tile Pool", "Editor.Icons.color-swatch16.png"),
            });
            _tbOpen = CreateButton("Open Project (Ctrl+O)", "Editor.Icons.folder-horizontal-open16.png");
            _tbSave = CreateButton("Save Project (Ctrl+S)", "Editor.Icons.disk16.png");
            _tbCut = CreateButton("Cut (Ctrl+X)", "Editor.Icons.scissors16.png");
            _tbCopy = CreateButton("Copy (Ctrl+C)", "Editor.Icons.document-copy16.png");
            _tbPaste = CreateButton("Paste (Ctrl+V)", "Editor.Icons.clipboard-paste16.png");
            _tbUndo = CreateButton("Undo (Ctrl+Z)", "Editor.Icons.arrow-turn-180-left16.png");
            _tbRedo = CreateButton("Redo (Ctrl+Y)", "Editor.Icons.arrow-turn16.png");
            _tbTileMode = CreateButton("Tile Authoring Mode (1)", "Editor.Icons.color-swatch16.png");
            _tbSetMode = CreateButton("TileSet Mode (2)", "Editor.Icons.table16.png");
            _tbMapMode = CreateButton("Map Mode (3)", "Editor.Icons.map16.png");

            _strip = new ToolStrip();
            _strip.Items.AddRange(new ToolStripItem[] {
                _tbNewProject, _tbNewItem, _tbOpen, _tbSave, new ToolStripSeparator(),
                _tbCut, _tbCopy, _tbPaste, new ToolStripSeparator(),
                _tbUndo, _tbRedo, new ToolStripSeparator(),
                _tbTileMode, _tbSetMode, _tbMapMode
            });

            ResetStandardComponent();

            _tbOpen.Click += ButtonOpenClickHandler;
        }

        public void BindStandardToolsController (IStandardToolsPresenter controller)
        {
            if (_stdController == controller) {
                return;
            }

            if (_stdController != null) {
                _stdController.SyncStandardToolsActions -= SyncStandardToolsActionsHandler;
            }

            _stdController = controller;

            if (_stdController != null) {
                _stdController.SyncStandardToolsActions += SyncStandardToolsActionsHandler;

                _stdController.RefreshStandardTools();
            }
            else {
                ResetStandardComponent();
            }
        }

        private void ResetStandardComponent ()
        {
            _tbNewProject.Enabled = false;
            _tbNewItem.Enabled = false;
            _tbOpen.Enabled = false;
            _tbSave.Enabled = false;
        }

        public ToolStrip Strip
        {
            get { return _strip; }
        }

        private void ButtonOpenClickHandler (object sender, EventArgs e)
        {
            if (_stdController != null) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Open Project Files";
                ofd.Filter = "Treefrog Project Files|*.tlp";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    _stdController.ActionOpenProject(ofd.FileName);
                }
            }
        }

        private void SyncStandardToolsActionsHandler (object sender, EventArgs e)
        {
            if (_stdController != null) {
                _tbNewProject.Enabled = _stdController.CanCreateProject;
                _tbOpen.Enabled = _stdController.CanOpenProject;
                _tbSave.Enabled = _stdController.CavSaveProject;
            }
        }

        private ToolStripButton CreateButton (string text, string resource)
        {
            ToolStripButton button = new ToolStripButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new System.Drawing.Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }

        private ToolStripSplitButton CreateSplitButton (string text, string resource)
        {
            ToolStripSplitButton button = new ToolStripSplitButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }

        private ToolStripDropDownButton CreateDropDownButton (string text, string resource)
        {
            ToolStripDropDownButton button = new ToolStripDropDownButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }

        private ToolStripMenuItem DropDownMenuItem (string text, string resource)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();

            item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            item.Text = text;
            item.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return item;
        }
    }
}
