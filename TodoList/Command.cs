using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TodoList
{
    // TODO クラスの"XML Documentation"がない、クラスとパブリックメソッドには最低でもつけたい
    public class Command
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // TODO ２つのコンストラクタのnullチェックで同じメッセージを返しているが、受けるときに見分けがつかないのでは？
        public Command(Action<object> execute,
            Func<Object, bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute ?? new Func<object, bool>(x => true);
        }

        public Command(Action execute, Func<bool> canExecute = null )
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = x => execute();
            _canExecute =
                canExecute == null ?
                new Func<object, bool>(x => true) :
                new Func<object, bool>(x => canExecute());
        }

        public void Execute(object value) { _execute(value); }
        public bool CanExecute(object value) { return _canExecute(value); }
    }
}
