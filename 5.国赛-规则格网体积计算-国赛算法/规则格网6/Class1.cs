using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace 规则格网6
{
    public class Pointl//用来储存带点号的三维点
    {
        public string dianhao;//类的成员
        public double X;
        public double Y;
        public double H;
        public double jiao;//存储和基点的夹角
    }

    public class Point2//用来储存带点号的三维点
    {
        public string dianhao;//类的成员
        public double X;
        public double Y;
        public double H;
        public int NW;//存储单边交点个数
        public double TU;//格网体积
    }
    class Class1
    {
        public static void duoxian(StreamWriter sw, double x,double y)
        {
            sw.Write("0\nVERTEX\n");//多线段标识
            sw.Write("8\n");//图层
            sw.Write("shiti\n");
            sw.Write("10\n");//X坐标
            sw.Write(x + "\n");
            sw.Write("20\n");//Y坐标
            sw.Write(y + "\n");
        }
        public static void daochutoR(RichTextBox T, DataGridView d)//表格数据导出
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
        public static double D(double x1, double y1, double x2, double y2)//计算距离
        {
            double d = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            return d;
        }
        #region 3.1反距离加权法
        public static double gaocheng(double x, double y, double r, List<Pointl> M)//计算高程
        {
            double d = 0, H;
            List<double> hi = new List<double>();
            List<double> di = new List<double>();
            for (int i = 0; i < M.Count; i++)
            {
                d = D(x, y, M[i].X, M[i].Y);
                if (d <= r)
                {
                    hi.Add(M[i].H / d);
                    di.Add(1 / d);
                }
            }
            H = hi.Sum() / di.Sum();
            return H;
        }
        #endregion
    }
}
