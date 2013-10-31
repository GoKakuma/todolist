﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TodoList
{
    sealed public class TodoLogic : Base, IDisposable
    {
        #region フィールドメンバー
        private const string CSV_FILTER = "CSV ファイル(*.csv)|*.csv|" + "テキスト ファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
        private readonly TodoDataClassesDataContext _dataContext;
        #endregion
        
        #region コンストラクタ
        public TodoLogic()
        {
            _dataContext = new TodoDataClassesDataContext();
            FillItems();
        }
        #endregion

        #region Dispose メソッド
        public void Dispose()
        {
            _dataContext.Dispose(); // _dataContextオブジェクトの解放
        }
        #endregion

        #region エラープロパティ
        public override string Error
        {
            get
            {
                if (_errors.Count > 0)
                {
                    return "入力されている値が正しくありません";
                }
                if (Id == 0 && IsFinished)
                {
                    return "新規タスクは、最初から「済み」に設定できません";
                }
                return null;
            }
        }
        #endregion

        // コマンドプロパティ
        #region クリアコマンドプロパティ
        private Command _clearCommand;
        public Command ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new Command(ExecuteClearCommand);
                }
                return _clearCommand;
            }
        }
        private void ExecuteClearCommand()
        {
            Keyword = string.Empty;
            FillItems();
        }
        #endregion

        #region 検索コマンドプロパティ
        private Command _searchCommand;
        public Command SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new Command(ExecuteSearchCommand);
                }
                return _searchCommand;
            }
        }

        private void ExecuteSearchCommand()
        {
            FillItems(Keyword);
        }
        #endregion

        #region 新規コマンドプロパティ
        private Command _addNewCommand;
        public Command AddNewCommand
        {
            get
            {
                if (_addNewCommand == null)
                {
                    _addNewCommand = new Command(ExecuteAddNewCommand);
                }
                return _addNewCommand;
            }
        }

        private void ExecuteAddNewCommand()
        {
            Item = Items[0];
        }
        #endregion

        #region 更新コマンドプロパティ
        private Command _updateCommand;
        public Command UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new Command(
                        ExecuteUpdateCommand, CanExecuteUpdateCommand);
                }
                return _updateCommand;
            }
        }

        private void ExecuteUpdateCommand()
        {
            Action action =
                (Id == 0) ?
                    new Action(InsertRecord) :
                    new Action(UpdateRecord);
            action();
        }

        private bool CanExecuteUpdateCommand()
        {
            return string.IsNullOrEmpty(Error);
        }

        private void InsertRecord()
        {
            Todo todo = new Todo();
            GetDataPropertyValues(todo);
            _dataContext.Todo.InsertOnSubmit(todo);
            _dataContext.SubmitChanges();
            Items.Add(todo);
            Item = todo;
        }

        private void UpdateRecord()
        {
            GetDataPropertyValues(Item);
            _dataContext.SubmitChanges();
        }

        private void GetDataPropertyValues(Todo todo)
        {
            todo.Id = Id;
            todo.Title = Title;
            todo.Contents = Contents;
            todo.Limit = Limit;
            todo.IsFinished = IsFinished;
        }
        #endregion

        #region 削除コマンドプロパティ
        private Command _deleteCommand;
        public Command DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new Command(
                        ExecuteDeleteCommand, CanExecuteDeleteCommand);
                }
                return _deleteCommand;
            }
        }

        private void ExecuteDeleteCommand()
        {   
            const string message = "削除して良いですか？";
			if (!Dialog.IsExecuteMessageBox(message)) return;

			_dataContext.Todo.DeleteOnSubmit(Item);
			_dataContext.SubmitChanges();

			int i = Items.IndexOf(Item);
			Items.Remove(Item);

			Item = (i < Items.Count)  ? Items[i] : Items[i - 1];
        }

        private bool CanExecuteDeleteCommand()
        {
            return (Id != 0) && (IsFinished);
        }
        #endregion

        #region 済み削除コマンドプロパティ
        private Command _autoDeleteCommand;
        public Command AutoDeleteCommand
        {
            get
            {
                if (_autoDeleteCommand == null)
                {
                    _autoDeleteCommand = new Command(ExecuteAutoDeleteCommand);
                }
                return _autoDeleteCommand;
            }
        }

        private void ExecuteAutoDeleteCommand()
        {
            const string message = "済みマークの付いているタスクを削除して良いですか？";
            if (!Dialog.IsExecuteMessageBox(message)) return;

            _dataContext.ExecuteCommand("DELETE FROM Task WHERE IsFinished = 1");
            FillItems();
        }
        #endregion

        #region インポートコマンドプロパティ
        private Command _importCommand;
        public Command ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new Command(ExecuteImportCommand);
                }
                return _importCommand;
            }
        }

        private void ExecuteImportCommand()
        {
            string message = "インポートしたいファイルを選択して下さい";
            string filter = CSV_FILTER;
            string filename = Dialog.GetOpenFilename(message, filter);
            if (string.IsNullOrEmpty(filename)) return;
            try
            {
                CsvFile.Import(filename);
                Dialog.ShowInfoMessage("インポート完了！");
                FillItems();
            }
            catch (Exception ex)
            {
                Dialog.ShowErrorMessageBox(ex.Message);
            }
        }
        #endregion

        #region エクスポートコマンドプロパティ
        private Command _executeExportCommand;
        public Command ExportCommand
        {
            get
            {
                if (_executeExportCommand == null)
                {
                    _executeExportCommand = new Command(ExecuteExportCommand);
                }
                return _executeExportCommand;
            }
        }

        private void ExecuteExportCommand()
        {
            string message = "エクスポートしたいファイルを選択して下さい";
            string filter = CSV_FILTER;
            string filename = Dialog.GetSaveFilename(message, filter);
            if (string.IsNullOrEmpty(filename)) return;
            try
            {
                CsvFile.Export(filename);
                Dialog.ShowInfoMessage("エクスポート完了！");
            }
            catch (Exception ex)
            {
                Dialog.ShowErrorMessageBox(ex.Message);
            }
        }
        #endregion

        // データプロパティ

        #region keyword プロパティ
        private string _keyword;
        public string Keyword
        {
            get
            {
                return _keyword;
            }
            set
            {
                _keyword = value;
                OnPropertyChanged("Keyword");
            }
        }
        #endregion

        #region Items プロパティ
        private ObservableCollection<Todo> _items = new ObservableCollection<Todo>();
        public ObservableCollection<Todo> Items
        {
            get
            {
                return _items;
            }
        }
        #endregion

        #region Item プロパティ
        private Todo _item;
        public Todo Item
        {
            get
            {
                return _item;
            }
            set
            {
                if (value == null) return;
                _item = value;
                Id = _item.Id;
                Title = _item.Title;
                Contents = _item.Contents;
                Limit = _item.Limit ?? DateTime.Today;
                IsFinished = _item.IsFinished ?? false;
                OnPropertyChanged("Item");
            }
        }
        #endregion
        
        #region Id プロパティ
        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        #endregion

        #region Title プロパティ
        private string _title;
        [Required(ErrorMessage = "タイトルを入力してください")]
        [StringLength(32, ErrorMessage = "32文字以内で入力してください")]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                UpdateErrors("Title", value);
                OnPropertyChanged("Title");
            }
        }
        #endregion
        
        #region Contents プロパティ
        private string _contents;
        [StringLength(32, ErrorMessage="32文字以内で入力してください")]
        public string Contents
        {
            get
            {
                return _contents;
            }
            set
            {
                _contents = value;
                UpdateErrors("Contents", value);
                OnPropertyChanged("Contents");
            }
        }
        #endregion
        
        #region Limit プロパティ
        private DateTime _limit;
        public DateTime Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                _limit = value;
                OnPropertyChanged("Limit");
            }
        }
        #endregion
        
        #region IsFinished プロパティ
        private bool _isFinished;
        public bool IsFinished
        {
            get
            {
                return _isFinished;
            }
            set
            {
                _isFinished = value;
                OnPropertyChanged("IsFinished");
            }
        }
        #endregion
        
        #region FillItems メソッド
        //メソッド    
        private void FillItems(String keyword = null)
        {
            string k = (keyword ?? string.Empty).Trim();
            var q =
                from p in _dataContext.Todo
                where p.Title.Contains(k)
                orderby p.Id descending
                select p;

            Items.Clear();
            Todo newTodo = new Todo { Title = "(新規)" };
            Items.Add(newTodo);
            foreach (var x in q)
            {
                Items.Add(x);
            }
            Item = Items[0];
        }
        #endregion
    }
}
