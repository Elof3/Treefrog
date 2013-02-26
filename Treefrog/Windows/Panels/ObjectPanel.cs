﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Aux;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Windows.Controllers;
using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using System.Collections.Generic;
using Treefrog.Presentation.Commands;

namespace Treefrog.Windows.Panels
{
    public partial class ObjectPanel : UserControl
    {
        private IObjectPoolCollectionPresenter _controller;
        private UICommandController _commandController;

        private ContextMenuStrip _itemContextMenu;

        public ObjectPanel ()
        {
            InitializeComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemoveObject.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game--minus.png"));
            _buttonAddObject.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game--plus.png"));

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.ObjectProtoImport, _buttonAddObject },
                { CommandKey.ObjectProtoDelete, _buttonRemoveObject },
            });

            _itemContextMenu = CommandMenuBuilder.BuildContextMenu(new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.ObjectProtoProperties,
                },
            }));

            _commandController.MapMenuItems(_itemContextMenu.Items);

            // Wire events

            _listView.ItemSelectionChanged += ListViewSelectionChangedHandler;
            _listView.MouseClick += ListViewMouseClickHandler;
        }

        public void BindController (IObjectPoolCollectionPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.SyncObjectPoolManager -= SyncObjectPoolManagerHandler;
                _controller.SyncObjectPoolActions -= SyncObjectPoolActionsHandler;
                _controller.SyncObjectPoolCollection -= SyncObjectPoolCollectionHandler;
                _controller.SyncObjectPoolControl -= SyncObjectPoolControlHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncObjectPoolManager += SyncObjectPoolManagerHandler;
                _controller.SyncObjectPoolActions += SyncObjectPoolActionsHandler;
                _controller.SyncObjectPoolCollection += SyncObjectPoolCollectionHandler;
                _controller.SyncObjectPoolControl += SyncObjectPoolControlHandler;

                _commandController.BindCommandManager(_controller.CommandManager);
            }
            else {
                _commandController.BindCommandManager(null);
            }
        }

        #region Event Handlers

        private void ListViewSelectionChangedHandler (object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (_controller != null) {
                if (!e.IsSelected)
                    _controller.ActionSelectObject(null);
                else
                    _controller.ActionSelectObject(e.Item.Text);
            }
        }

        private void ListViewMouseClickHandler (object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) {
                ListViewItem item = _listView.GetItemAt(e.X, e.Y);
                if (item != null && item.Selected) {
                    _itemContextMenu.Show(_listView, e.Location);
                }
            }
        }

        private void SyncObjectPoolManagerHandler (object sender, EventArgs e)
        {
            if (_controller != null && _controller.SelectedObjectPool != null) {
                ImageList imgList = BuildImageList(_controller.SelectedObjectPool.Name);
                PopulateList(imgList);
            }
        }

        private void SyncObjectPoolActionsHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                
            }
        }

        private void SyncObjectPoolCollectionHandler (object sender, EventArgs e)
        {
            
        }

        private void SyncObjectPoolControlHandler (object sender, EventArgs e)
        {

        }

        #endregion

        private ImageList BuildImageList (string objectPool)
        {
            if (!_controller.ObjectPoolManager.Pools.Contains(objectPool))
                return null;

            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(64, 64);
            imgList.ColorDepth = ColorDepth.Depth32Bit;

            foreach (ObjectClass obj in _controller.ObjectPoolManager.Pools[objectPool].Objects) {

                imgList.Images.Add(obj.Name, CreateCenteredBitmap(obj.Image, 64, 64));
            }

            return imgList;
        }

        private void PopulateList (ImageList imgList)
        {
            _listView.Clear();
            _listView.LargeImageList = imgList;
            
            foreach (string name in imgList.Images.Keys) {
                _listView.Items.Add(new ListViewItem(name, name));
            }
        }

        private Bitmap CreateCenteredBitmap (TextureResource source, int width, int height)
        {
            using (Bitmap tmp = source.CreateBitmap()) {
                return CreateCenteredBitmap(tmp, width, height);
            }
        }

        private Bitmap CreateCenteredBitmap (Bitmap source, int width, int height)
        {
            Bitmap dest = new Bitmap(width, height, source.PixelFormat);
            int x = Math.Max(0, (width - source.Width) / 2);
            int y = Math.Max(0, (height - source.Height) / 2);
            int w = Math.Min(width, source.Width);
            int h = Math.Min(height, source.Height);

            using (Graphics g = Graphics.FromImage(dest)) {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(source, x, y, w, h);
            }

            return dest;
        }
    }
}
