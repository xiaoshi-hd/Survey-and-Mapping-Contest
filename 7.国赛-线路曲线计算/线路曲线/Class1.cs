using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 线路曲线
{
    class Class1
    {
        public static double fangwei(double x1, double y1, double x2, double y2)//根据已知数据计算方位角需要
        {
            double fangweijiao;
            fangweijiao = 180 - 90 * Math.Abs(y2 - y1) / ((y2 - y1) + Math.Pow(10, -10)) - Math.Atan((x2 - x1) / ((y2 - y1) + Math.Pow(10, -10))) * 180 / Math.PI;
            return fangweijiao * Math.PI / 180;//返回弧度值
        }
        public static string hudutodms(double hudu)//计算过程中数据导出需要
        {
            double du, d, m, s;
            du = hudu * 180 / Math.PI;
            d = Math.Floor(du);
            m = Math.Floor((du - d) * 60);
            s = Math.Round(((du - d) * 60 - m) * 60, 1);//保留到0.1秒
            return d + "°" + m + "′" + s + "″";
        }
        public static Pointl yuanquxian1(Pointl ZY,double licheng,string dianhao ,double R,double a ,double k )
        {
            Pointl f=new Pointl();
            double jiao=(licheng-ZY.licheng)/R;
            double x1=R*Math.Sin(jiao);
            double y1=R*(1-Math.Cos(jiao));
            f.dianhao = dianhao;
            f.X = ZY.X + x1 * Math.Cos(a) - k*y1 * Math.Sin(a);
            f.Y = ZY.Y + x1 * Math.Sin(a) + k*y1 * Math.Cos(a);
            f.licheng = licheng;
            return f;
        }//圆曲线上半部分计算坐标
        public static Pointl yuanquxian2(Pointl YZ, double licheng, string dianhao, double R, double a, double k)
        {
            Pointl f = new Pointl();
            double jiao = (YZ.licheng - licheng) / R;
            double x1 = R * Math.Sin(jiao);
            double y1 = R * (1 - Math.Cos(jiao));
            f.dianhao = dianhao;
            f.X = YZ.X + x1 * Math.Cos(a) + k * y1 * Math.Sin(a);
            f.Y = YZ.Y + x1 * Math.Sin(a) - k * y1 * Math.Cos(a);
            f.licheng = licheng;
            return f;
        }//圆曲线下半部分计算坐标
        public static Pointl huanquxian1(Pointl ZH, double licheng, string dianhao, double R, double Ls,double a, double k)
        {
            Pointl f = new Pointl();
            double Li = licheng - ZH.licheng;
            double x1 = Li - Math.Pow(Li, 5) / (40 * R * R * Ls * Ls);
            double y1 = Li * Li * Li / (6 * R * Ls);

            f.dianhao = dianhao;
            f.X = ZH.X + x1 * Math.Cos(a) - k * y1 * Math.Sin(a);
            f.Y = ZH.Y + x1 * Math.Sin(a) + k * y1 * Math.Cos(a);
            f.licheng = licheng;
            return f;
        }//缓和曲线计算ZH-HY坐标
        public static Pointl huanquxian2(Pointl ZH, double licheng, string dianhao, double R, double B0, double m, double P, double Ls, double a, double k)
        {
            Pointl f = new Pointl();
            double Li = licheng - ZH.licheng;

            double jiao = B0 + (Li - Ls) / R;
            double x1 = m + R * Math.Sin(jiao);
            double y1 = P + R * (1 - Math.Cos(jiao));

            f.dianhao = dianhao;
            f.X = ZH.X + x1 * Math.Cos(a) - k * y1 * Math.Sin(a);
            f.Y = ZH.Y + x1 * Math.Sin(a) + k * y1 * Math.Cos(a);
            f.licheng = licheng;
            return f;
        }//缓和曲线计算HY-YH坐标
        public static Pointl huanquxian3(Pointl HZ, double licheng, string dianhao, double R, double Ls, double a, double k)
        {
            Pointl f = new Pointl();
            double Li = HZ.licheng-licheng;
            double x1 = Li - Math.Pow(Li, 5) / (40 * R * R * Ls * Ls);
            double y1 = Li * Li * Li / (6 * R * Ls);

            f.dianhao = dianhao;
            f.X = HZ.X + x1 * Math.Cos(a) + k * y1 * Math.Sin(a);
            f.Y = HZ.Y + x1 * Math.Sin(a) - k * y1 * Math.Cos(a);
            f.licheng = licheng;
            return f;
        }//缓和曲线计算YH-HZ坐标
        public static Pointl zhixian(Pointl A,Pointl B,double liecheng,string dianhao)
        {
            Pointl f = new Pointl() ;
            double a = 0;
            f.dianhao = dianhao;
            f.licheng = liecheng;
            a = fangwei(A.X, A.Y, B.X, B.Y);
            f.X = A.X + (liecheng - A.licheng) * Math.Cos(a);
            f.Y = A.Y + (liecheng - A.licheng) * Math.Sin(a);
            return f;
        }//计算直线上点坐标
        public static void daochutoR(RichTextBox T, DataGridView d)
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
        }//导出到文本框中
        
    }
    public class Pointl//用来储存带点号二维点
    {
        public string dianhao;//类的成员
        public double X;
        public double Y;
        public double licheng;
    }
}
