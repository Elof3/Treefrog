﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Aux;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Controllers;
using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.Windows.Panels
{
    public partial class ObjectPanel : UserControl
    {
        private ObjectPoolCollectionPresenter _controller;
        private UICommandController _commandController;

        private ContextMenuStrip _itemContextMenu;

        public ObjectPanel ()
        {
            InitializeComponent();

            // Load form elements

            _buttonRemoveObject.Image = Properties.Resources.GameMinus;
            _buttonAddObject.Image = Properties.Resources.GamePlus;

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.ObjectProtoImport, _buttonAddObject },
                { CommandKey.ObjectProtoDelete, _buttonRemoveObject },
            });

            _itemContextMenu = CommandMenuBuilder.BuildContextMenu(new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.ObjectProtoEdit,
                },
                new CommandMenuGroup() {
                    CommandKey.ObjectProtoClone,
                    CommandKey.ObjectProtoDelete,
                    CommandKey.ObjectProtoRename,
                },
                new CommandMenuGroup() {
                    CommandKey.ObjectProtoProperties,
                },
            }));

            _commandController.MapMenuItems(_itemContextMenu.Items);

            // Wire events

            _listView.ItemSelectionChanged += ListViewSelectionChangedHandler;
            _listView.MouseClick += ListViewMouseClickHandler;
        }

        public void BindController (ObjectPoolCollectionPresenter controller)
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
                    _controller.ActionSelectObject(Guid.Empty);
                else
                    _controller.ActionSelectObject((Guid)e.Item.Tag);
            }
        }

        private void ListViewMouseClickHandler (object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                ListViewItem item = _listView.GetItemAt(e.X, e.Y);
                if (item != null)
                    _controller.ActionSelectObject((Guid)item.Tag);
            }
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
                Dictionary<string, Image> imgList = BuildImageList(_controller.SelectedObjectPool.Uid);
                PopulateList(imgList);
            }
            else {
                _listView.Clear();
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

        private Dictionary<string, Image> BuildImageList (Guid objectPoolUid)
        {
            if (!_controller.ObjectPoolManager.Pools.Contains(objectPoolUid))
                return null;

            Dictionary<string, Image> imgList = new Dictionary<string, Image>();

            foreach (ObjectClass obj in _controller.ObjectPoolManager.Pools[objectPoolUid].Objects) {
                if (obj.Image != null) {
                    Bitmap image = CreateCenteredBitmap(obj.Image, 64, 64);
                    image.Tag = obj.Uid;
                    imgList.Add(obj.Name, image);
                }
            }

            return imgList;
        }

        private void PopulateList (Dictionary<string, Image> list)
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(64, 64);
            imgList.ColorDepth = ColorDepth.Depth32Bit;

            foreach (var item in list)
                imgList.Images.Add(item.Key, item.Value);

            _listView.Clear();
            _listView.LargeImageList = imgList;
            
            foreach (var item in list) {
                _listView.Items.Add(new ListViewItem(item.Key, item.Key) { Tag = item.Value.Tag });
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
            if (source == null)
                return null;

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

        private void _listView_DoubleClick (object sender, EventArgs e)
        {
            if (_controller != null) {
                foreach (ListViewItem item in _listView.SelectedItems) {
                    _controller.ActionEditObject((Guid)item.Tag);
                    return;
                }
            }
        }
    }
}
