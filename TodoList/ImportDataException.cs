using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TodoList
{
    //TODO 独自の例外クラス定義は良い、ただApplicationExceptionを継承すべきでは？
    // 参考　http://msdn.microsoft.com/ja-jp/library/ms954599.aspx
    public class ImportDataException : Exception
    {
        public ImportDataException()
            : base("このファイルはインポートできません"){}
    }
}
