using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 纵横断面体积计算
{
    class Point1
    {
        public string dianhao;
        public double licheng;
        public double X;
        public double Y;
        public double Z;
    }
    class caculate
    {
        #region 方位角计算
        public static double fangwei(Point1 pt1, Point1 pt2)
        {
            double fangweijiao;
            fangweijiao = 180 - 90 * Math.Abs(pt2.Y - pt1.Y) / (pt2.Y - pt1.Y + Math.Pow(10, -10)) - Math.Atan((pt2.X - pt1.X) / (pt2.Y - pt1.Y + Math.Pow(10, -10))) * 180 / Math.PI;
            return fangweijiao * Math.PI / 180;
        }
        #endregion
        #region 距离计算
        public static double juli(Point1 pt1, Point1 pt2)
        {
            double juli;
            juli = Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            return juli;
        }
        #endregion
    }
}
