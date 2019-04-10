using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace 附合水准
{
    class Class1
    {
        public static void daochu1( RichTextBox T,DataGridView d)
        {
            List<string> StrArray = new List<string>();
            string str = null;
            for (int i = 0; i < d.Columns.Count; i++)
            {
                StrArray.Add(string.Format("{0,-12}", d.Columns[i].HeaderText));
            }
            str = string.Join("", StrArray);
            T.Text += (str + "\n");
            for (int i = 0; i < d.Rows.Count; i++)
            {
                str = null; 
                StrArray.Clear();
                for (int j = 0; j < d.Columns.Count; j++)
                {
                    StrArray.Add(string.Format("{0,-14}", d.Rows[i].Cells[j].Value));
                }
                str += string.Join(" ", StrArray);
                T.Text += (str + "\n");
            }
        }
    }
}
