using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace 三角高程
{
    class Class1
    {
        public static double dmstohudu(double dms)//d.ms的形式
        {
            double d, m, s;
            int i = 1;
            if (dms < 0)
            {
                i = -1;
                dms = Math.Abs(dms);
            }
            d = Math.Floor(dms);
            m = Math.Floor((dms - d) * 100);
            s = ((dms - d) * 100 - m) * 100;
            return i * (d + m / 60 + s / 3600) * Math.PI / 180;
        }
        public static double qiuqi(double D)//计算球气差
        {
            double p,r;
            p = D * D / (2 * 6378137);//地球曲率半径，大气折光系数
            r = (-0.15 * D * D) / (2 * 6378137);
            return p + r;
        }
        public static void daochu1(RichTextBox T, DataGridView d)
        {
            List<string> StrArray = new List<string>();
            string str = null;
            for (int i = 0; i < d.Columns.Count; i++)
            {
                StrArray.Add(string.Format("{0,-12}", d.Columns[i].HeaderText));
            }
            str = string.Join("\t", StrArray);
            T.Text += (str + "\n");
            for (int i = 0; i < d.Rows.Count; i++)
            {
                str = null;
                StrArray.Clear();
                for (int j = 0; j < d.Columns.Count; j++)
                {
                    StrArray.Add(string.Format("{0,-12}", d.Rows[i].Cells[j].Value));
                }
                str += string.Join("\t", StrArray);
                T.Text += (str + "\n");
            }
        }
    }
}
