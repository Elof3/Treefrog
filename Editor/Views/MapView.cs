﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model;

namespace Editor
{
    public partial class MapView : UserControl, IFormView, ITileToolbarSubscriber
    {
        private Project _project;

        // XXX: Move to individual level object
        private CommandHistory _commandHistory;

        private DrawTool _drawTool;
        private EraseTool _eraseTool;
        private FillTool _fillTool;

        public MapView ()
        {
            InitializeComponent();

            _commandHistory = new CommandHistory();
            _commandHistory.HistoryChanged += CommandHistoryChangedHandler;
        }

        public MapView (Project project) 
            : this()
        {
            _project = project;

            // Tools

            _drawTool = new DrawTool(tilesetControl, _tilePoolPane.TileControl, _project.TileSets["Default"], _commandHistory);
            _eraseTool = new EraseTool(tilesetControl, _project.TileSets["Default"], _commandHistory);
            _fillTool = new FillTool(tilesetControl, _tilePoolPane.TileControl, _project.TileSets["Default"], _commandHistory);

            // XXX
            _drawTool.Enabled = true;

            _tilePoolPane.SetupDefault(_project);
        }

        private void CommandHistoryChangedHandler (object sender, CommandHistoryEventArgs e)
        {
            OnCommandHistoryChanged(e);
        }

        #region IFormView Members

        

        public new Control Control
        {
            get { return this; }
        }

        public float Zoom
        {
            get { return tilemapControl.Zoom; }
            set { tilemapControl.Zoom = value; }
        }

        public void Display ()
        {
            OnCommandHistoryChanged(new CommandHistoryEventArgs(_commandHistory));
        }

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        public event EventHandler<CommandHistoryEventArgs> CommandHistoryChanged;

        public void Undo ()
        {
            if (_commandHistory.CanUndo) {
                _commandHistory.Undo();
                OnCommandHistoryChanged();
            }
        }

        public void Redo ()
        {
            if (_commandHistory.CanRedo) {
                _commandHistory.Redo();
                OnCommandHistoryChanged();
            }
        }

        public void Copy ()
        {
            throw new NotImplementedException();
        }

        public void Cut ()
        {
            throw new NotImplementedException();
        }

        public void Paste ()
        {
            throw new NotImplementedException();
        }

        private void OnCommandHistoryChanged ()
        {
            OnCommandHistoryChanged(new CommandHistoryEventArgs(_commandHistory));
        }

        protected virtual void OnCommandHistoryChanged (CommandHistoryEventArgs e)
        {
            if (CommandHistoryChanged != null) {
                CommandHistoryChanged(this, e);
            }
        }

        #endregion

        #region ITileToolbarSubscriber Members

        private TileToolMode _toolMode = TileToolMode.Draw;

        public TileToolMode TileToolMode
        {
            get
            {
                return _toolMode;
            }
            set
            {
                _toolMode = value;

                _drawTool.Enabled = false;
                _eraseTool.Enabled = false;
                _fillTool.Enabled = false;

                switch (_toolMode) {
                    case Editor.TileToolMode.Select:
                    case Editor.TileToolMode.Stamp:
                        tilesetControl.Mode = TileControlMode.Select;
                        break;
                    case Editor.TileToolMode.Draw:
                        tilesetControl.Mode = TileControlMode.Click;
                        _drawTool.Enabled = true;
                        break;
                    case Editor.TileToolMode.Erase:
                        tilesetControl.Mode = TileControlMode.Click;
                        _eraseTool.Enabled = true;
                        break;
                    case Editor.TileToolMode.Fill:
                        tilesetControl.Mode = TileControlMode.Click;
                        _fillTool.Enabled = true;
                        break;
                }
            }
        }

        #endregion
    }
}
