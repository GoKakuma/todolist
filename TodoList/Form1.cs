using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TodoList
{
    public partial class Form1 : Form
    {
        #region フィールド変数
        private TodoLogic _logic;
        private WindowsFormsCommandBindings _commandBindings;
        #endregion

        #region コンストラクタ
        public Form1()
        {
            InitializeComponent();
            InitializeLogic();      // オブジェクト作成
            InitializeDataBindings();
            InitializeCommands();
            _commandBindings.RefreshControlEnabled();
        }

        // オブジェクトを作成するメソッド
        private void InitializeLogic()
        {
            _logic = new TodoLogic();
            _logic.PropertyChanged += new PropertyChangedEventHandler(_logic_PropertyChanged);
            Disposed += new EventHandler(Form1_Disposed);
        }

        private void InitializeDataBindings()
        {
            todoBindingSource.DataSource = _logic;
            todoListBindingSource.DataSource = todoBindingSource;
            todoListBindingSource.DataMember = "Items";
            BindingSource b = todoBindingSource;
            searchTextbox.DataBindings.Add("Text", b, "keyword");
            titletextBox.DataBindings.Add("Text", b, "Title");
            DataTextBox.DataBindings.Add("Text", b, "Contents");
            limitDateTimePicker.DataBindings.Add("Value", b, "limit");
            finishCheckBox.DataBindings.Add("Checked", b, "Isfinished", false, DataSourceUpdateMode.OnPropertyChanged);


            listBox.DataSource = todoListBindingSource;
            listBox.DisplayMember = "Title";
            listBox.SelectedIndexChanged += new EventHandler(listBox_SelectedIndexChange);

            errorProvider1.DataSource = todoBindingSource;
        }

		private void InitializeCommands()
		{
			_commandBindings = new WindowsFormsCommandBindings();
			_commandBindings.AddCommand(clearButton, _logic.ClearCommand);
			_commandBindings.AddCommand(searchButton, _logic.SearchCommand);
			_commandBindings.AddCommand(newButton, _logic.AddNewCommand);
			_commandBindings.AddCommand(updateButton, _logic.UpdateCommand);
			_commandBindings.AddCommand(deleteButton, _logic.DeleteCommand);
            _commandBindings.AddCommand(deleteTodoMenuItem, _logic.AutoDeleteCommand);
			_commandBindings.AddCommand(importMenuItem, _logic.ImportCommand);
			_commandBindings.AddCommand(exportMenuItem, _logic.ExportCommand);
		}
        #endregion

        #region イベントハンドラ
        void listBox_SelectedIndexChange(object sender, EventArgs e)
        {
            _logic.Item = listBox.SelectedItem as Todo;
        }

        void Form1_Disposed(object sender, EventArgs e)
        {
            _logic.Dispose();
        }

        void _logic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Item") listBox.SelectedItem = _logic.Item;
            _commandBindings.RefreshControlEnabled();
            errorLabel.Text = _logic.Error;
        }
        #endregion

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
