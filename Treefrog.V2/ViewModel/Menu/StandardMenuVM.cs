﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Treefrog.Framework.Model;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Treefrog.ViewModel.Dialogs;
using Treefrog.Messages;
using Treefrog.ViewModel.Commands;
using System.ComponentModel;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Menu
{
    public class StandardMenuVM : ViewModelBase
    {
        EditorVM _editor;
        CommandHistory _history;

        public StandardMenuVM (EditorVM editor)
        {
            _editor = editor;
            _editor.PropertyChanged += HandleEditorPropertyChanged;
        }

        private void HandleEditorPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "CommandHistory":
                    if (_history != null)
                        _history.HistoryChanged -= HandleHistoryChanged;
                    _history = _editor.CommandHistory;
                    if (_history != null)
                        _history.HistoryChanged += HandleHistoryChanged;

                    if (_undoCommand != null)
                        _undoCommand.RaiseCanExecuteChanged();
                    if (_redoCommand != null)
                        _redoCommand.RaiseCanExecuteChanged();
                    break;
            }
        }

        private void HandleHistoryChanged (object sender, EventArgs e)
        {
            if (_undoCommand != null)
                _undoCommand.RaiseCanExecuteChanged();
            if (_redoCommand != null)
                _redoCommand.RaiseCanExecuteChanged();
        }

        #region Open Project Command

        private RelayCommand _openProjectCommand;

        public ICommand OpenProjectCommand
        {
            get
            {
                if (_openProjectCommand == null)
                    _openProjectCommand = new RelayCommand(OnOpenProject, CanOpenProject);
                return _openProjectCommand;
            }
        }

        private bool CanOpenProject ()
        {
            return true;
        }

        private void OnOpenProject ()
        {
            try {
                IOService service = ServiceProvider.GetService<IOService>();
                if (service != null) {
                    string path = service.OpenFileDialog(new OpenFileOptions()
                    {
                        Filter = "Treefrog Projects|*.tlp|All Files|*",
                        FilterIndex = 0,
                    });

                    _editor.OpenProject(path);
                }
            }
            catch {
                IMessageService service = ServiceProvider.GetService<IMessageService>();
                if (service != null) {
                    service.ShowMessage(new MessageInfo()
                    {
                        Message = "Error opening requested file.",
                        Type = MessageType.Warning
                    });
                }
            }
        }

        #endregion

        #region Save Project Command

        private RelayCommand _saveProjectCommand;

        public ICommand SaveProjectCommand
        {
            get
            {
                if (_saveProjectCommand == null)
                    _saveProjectCommand = new RelayCommand(OnSaveProject, CanSaveProject);
                return _saveProjectCommand;
            }
        }

        private bool CanSaveProject ()
        {
            return _editor.Project != null;
        }

        private void OnSaveProject ()
        {
            if (String.IsNullOrEmpty(_editor.ProjectFile)) {
                IOService service = ServiceProvider.GetService<IOService>();
                if (service != null) {
                    string path = service.SaveFileDialog(new SaveFileOptions()
                    {
                        Filter = "Treefrog Projects|*.tlp|All Files|*",
                        FilterIndex = 0,
                    });

                    _editor.SaveProject(path);
                }
            }
            else {
                _editor.SaveProject(_editor.ProjectFile);
            }
        }

        #endregion

        #region Save Project As Command

        private RelayCommand _saveProjectAsCommand;

        public ICommand SaveProjectAsCommand
        {
            get
            {
                if (_saveProjectAsCommand == null)
                    _saveProjectAsCommand = new RelayCommand(OnSaveProjectAs, CanSaveProjectAs);
                return _saveProjectAsCommand;
            }
        }

        private bool CanSaveProjectAs ()
        {
            return _editor.Project != null;
        }

        private void OnSaveProjectAs ()
        {
            IOService service = ServiceProvider.GetService<IOService>();
            if (service != null) {
                string path = service.SaveFileDialog(new SaveFileOptions()
                {
                    Filter = "Treefrog Projects|*.tlp|All Files|*",
                    FilterIndex = 0,
                });

                _editor.SaveProject(path);
            }
        }

        #endregion

        #region New Level Command

        private RelayCommand _newLevelCommand;

        public ICommand NewLevelCommand
        {
            get
            {
                if (_newLevelCommand == null)
                    _newLevelCommand = new RelayCommand(OnNewLevel, CanNewLevel);
                return _newLevelCommand;
            }
        }

        private bool CanNewLevel ()
        {
            return true;
        }

        private void OnNewLevel ()
        {
            NewLevelDialogVM vm = new NewLevelDialogVM();
            foreach (Level lev in _editor.Project.Project.Levels)
                vm.ReservedNames.Add(lev.Name);

            BlockingDialogMessage message = new BlockingDialogMessage(this, vm);
            Messenger.Default.Send(message);

            if (message.DialogResult == true) {
                Level lev = new Level(vm.LevelName, vm.TileWidth ?? 0, vm.TileHeight ?? 0, vm.LevelWidth ?? 0, vm.LevelHeight ?? 0);
                _editor.Project.Project.Levels.Add(lev);
            }
        }

        #endregion

        #region Undo Command

        private RelayCommand _undoCommand;

        public ICommand UndoCommand
        {
            get
            {
                if (_undoCommand == null)
                    _undoCommand = new RelayCommand(OnUndo, CanUndo);
                return _undoCommand;
            }
        }

        private bool CanUndo ()
        {
            if (_history == null)
                return false;
            return _history.CanUndo;
        }

        private void OnUndo ()
        {
            if (_history == null)
                return;
            _history.Undo();
        }

        #endregion

        #region Redo Command

        private RelayCommand _redoCommand;

        public ICommand RedoCommand
        {
            get
            {
                if (_redoCommand == null)
                    _redoCommand = new RelayCommand(OnRedo, CanRedo);
                return _redoCommand;
            }
        }

        private bool CanRedo ()
        {
            if (_history == null)
                return false;
            return _history.CanRedo;
        }

        private void OnRedo ()
        {
            if (_history == null)
                return;
            _history.Redo();
            _redoCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
