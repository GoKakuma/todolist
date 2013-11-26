using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//TODO Todolistは名前を変更すべきと思います。
namespace TodoList
{
    // TODO クラスの"XML Documentation"がない、クラスとパブリックメソッドには最低でもつけたい
    // TODO Baseも名前がよくない。すべてのクラスの基底クラスのように見える。
    //      実際はTodoLogicの親なので、
    abstract public class Base : INotifyPropertyChanged, IDataErrorInfo
    {
        //イベント 
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        //TODO １クラス１ファイルの構成にする
        // 抽象クラス
        abstract public string Error { get; }

        protected readonly Dictionary<string, string> _errors =
            new Dictionary<string, string>();
        public string this[string propertyName]
        {
            get
            {
	    	//TODO 例外処理は要らないのか?
                return
                    _errors.ContainsKey(propertyName) ?
                    _errors[propertyName] :
                    null;
            }
        }

        protected void UpdateErrors(string name, object value)
        {
            try
            {
	    	//TODO 全体的に、変数定義をvarで行うべきと思います（ソースがやたら長くなり、レビュー等に）？
                // >Gokakuma 確認終わったらこのコメント三行を消してください（履歴はGit内で確認できるため、不要なコメントを消すべき）
                //ValidationContext v = new ValidationContext(this, null, null);
                var v = new ValidationContext(this, null, null);
                v.MemberName = name;
                Validator.ValidateProperty(value, v);
                _errors.Remove(name);
            }
            catch (Exception ex)
            {
                _errors[name] = ex.Message;
            }
        }
    }
}
