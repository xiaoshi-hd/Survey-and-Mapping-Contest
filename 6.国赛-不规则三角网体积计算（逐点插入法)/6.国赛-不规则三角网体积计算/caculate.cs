using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6.国赛_不规则三角网体积计算
{
    public class Point1
    {
        public string dianhao;//类的成员
        public double X;
        public double Y;
        public double Z;
    }
    public class Line//用来存储线
    {
        public Point1 Begin;
        public Point1 End;
    }
    public class sjx//用来存储三角形的三个点
    {
        public Point1 p1;
        public Point1 p2;
        public Point1 p3;
    }
    class caculate
    {
        #region 三角形外接圆圆心
        public static double X0(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double x0;
            x0 = ((y2 - y1) * (y3 * y3 - y1 * y1 + x3 * x3 - x1 * x1) - (y3 - y1) * (y2 * y2 - y1 * y1 + x2 * x2 - x1 * x1)) / (2 * (x3 - x1) * (y2 - y1) - 2 * (x2 - x1) * (y3 - y1));
            return x0;
        }
        public static double Y0(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double y0;
            y0 = ((x2 - x1) * (x3 * x3 - x1 * x1 + y3 * y3 - y1 * y1) - (x3 - x1) * (x2 * x2 - x1 * x1 + y2 * y2 - y1 * y1)) / (2 * (y3 - y1) * (x2 - x1) - 2 * (y2 - y1) * (x3 - x1));
            return y0;
        }
        #endregion
        #region 三角形外接圆半径
        public static double R(double x0, double y0, double x1, double y1)
        {
            double r;
            r = Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
            return r;
        }
        #endregion
        #region 斜三棱柱计算
        public static double SS(sjx san)
        {
            double s;
            s = Math.Abs((san.p2.X - san.p1.X) * (san.p3.Y - san.p1.Y) - (san.p3.X - san.p1.X) * (san.p2.Y - san.p1.Y)) / 2;
            return s;
        }
        public static double H(sjx san,double gaocheng)
        {
            double h;
            h = (san.p1.Z + san.p2.Z + san.p3.Z) / 3 - gaocheng;
            return h;
        }
        #endregion
    }
}
