using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualBasic.FileIO;


namespace TodoList
{
    // TODO クラスの"XML Documentation"がない、クラスとパブリックメソッドには最低でもつけたい
	//TODO クラス名はCsvFileUtil？とか、呼び出し側から自然になるように変更すべき？
    static public class CsvFile
    {
        #region メソッド
        static public void Import(string filename)
        {
            IEnumerable<Todo> todos = GetImportTodos(filename);
            //TODO 全体的に、変数定義をvarで行うべき
            using (TodoDataClassesDataContext d = new  TodoDataClassesDataContext())
            {
                foreach (Todo x in todos) d.Todo.InsertOnSubmit(x);
                d.SubmitChanges();
            }
        }

        static private IEnumerable<Todo> GetImportTodos(string filename)
        {
            //TODO 全体的に、変数定義をvarで行うべき
            List<Todo> list = new List<Todo>();
            Encoding encode = Encoding.GetEncoding("Shift_Jis");
            //TODO 全体的に、変数定義をvarで行うべき
            using (TextFieldParser p = new TextFieldParser(filename, encode))
            {
                p.TextFieldType = FieldType.Delimited;
                p.SetDelimiters(",");
                p.HasFieldsEnclosedInQuotes = true;
                p.TrimWhiteSpace = false;
                while (!p.EndOfData)
                {

                    //TODO a だとわからないので、変数名をちゃんとつける
                    //     オブジェクト指向は、大規模になる（＝チーム開発になる）ので
                    //     ローカル変数であっても名前づけは重要になる（実際私も野口先生に怒られてますが。。）
                    string[] a = p.ReadFields();
                    Todo todo = CreateTodo(a);
                    list.Add(todo);
                }
            }
            return list;
        }

        static private Todo CreateTodo(string[] fields)
        {
            if (fields.Length != 4) throw new ImportDataException();
            var q =
                from x in fields
                where x == null
                select x;

            if (q.Any()) throw new ImportDataException();
            //TODO 全体的に、変数定義をvarで行うべき
            Todo todo = new Todo();

            todo.Title = fields[0].TrimEnd();
            if (todo.Title.Length == 0 || todo.Title.Length > 32)
                throw new ImportDataException();

            todo.Contents = fields[1].TrimEnd();
            if(todo.Contents.Length > 32)
                throw new ImportDataException();

            DateTime d;
            if(!DateTime.TryParse(fields[2], out d ))
                throw new ImportDataException();
            todo.Limit = d;

            bool b;
            if (!bool.TryParse(fields[3], out b))
                throw new ImportDataException();
            todo.IsFinished = b;

            return todo;
        }

        static public void Export(string filename)
        {
            Encoding encode = Encoding.GetEncoding("Shift_Jis");
            //TODO 全体的に、変数定義をvarで行うべき
            using(StreamWriter w = new StreamWriter(filename, false, encode))
            {
                //TODO 全体的に、変数定義をvarで行うべき
                using(TodoDataClassesDataContext d = new TodoDataClassesDataContext())
                {
                    foreach(Todo x in d.Todo)
                    {
                        string s = string.Format(
                            "\"{0}\",\"{1}\",\"{2}\",{3}",
                            x.Title,
                            x.Contents,
                            x.Limit.Value.ToString("yyyy/MM/dd"),
                            x.IsFinished);
                        w.WriteLine(s);
                    }
                }
            }
        }
        #endregion
    }
}
