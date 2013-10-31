using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TodoList
{
    abstract public class Base : INotifyPropertyChanged, IDataErrorInfo
    {
        //イベント 
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // 抽象クラス
        abstract public string Error { get; }

        protected readonly Dictionary<string, string> _errors =
            new Dictionary<string, string>();
        public string this[string propertyName]
        {
            get
            {
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
                ValidationContext v = new ValidationContext(this, null, null);
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
