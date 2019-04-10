using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1
{
    class caculate
    {
        #region 方位角
        public static double fangwei(double x1, double y1, double x2, double y2)//根据已知数据计算方位角需要
        {
            double fangweijiao;
            fangweijiao = 180 - 90 * Math.Abs(y2 - y1) / ((y2 - y1) + Math.Pow(10, -10)) - Math.Atan((x2 - x1) / ((y2 - y1) + Math.Pow(10, -10))) * 180 / Math.PI;
            return fangweijiao * Math.PI / 180;//返回弧度值
        }
        #endregion
        #region 度分秒化弧度
        public static double dmstohudu(string dms)//计算过程中数据导入需要
        {
            string[] a = new string[] { "°", "′", "″" };
            string[] jiaodu = dms.Split(a,StringSplitOptions.RemoveEmptyEntries);
            double d, m, s;
            d = Convert.ToDouble(jiaodu[0]);
            m = Convert.ToDouble(jiaodu[1]);
            s = Convert.ToDouble(jiaodu[2]);
            return (d + m / 60 + s / 3600) * Math.PI / 180;
        }
        #endregion
        #region 弧度化度分秒
        public static string hudutodms(double hudu)//计算过程中数据导出需要
        {
            double du, d, m, s;
            du = hudu * 180 / Math.PI;
            d = Math.Floor(du);
            m = Math.Floor((du - d) * 60);
            s = Math.Round(((du - d) * 60 - m) * 60, 1);//保留到0.1秒
            return d + "°" + m + "′" + s + "″";
        }
        #endregion
        #region 弧度化秒
        public static double hudutos(double hudu)//计算过程中计算角度闭合差需要
        {
            double du = hudu * 180 / Math.PI;
            return Math.Round(du * 3600, 3);
        }
        #endregion
        #region 度.分秒化度分秒
        public static string dmstojiaodu(double dms)//d.ms化成dms的形式，用于数据显示
        {
            double d, m, s;
            d = Math.Floor(dms);
            m = Math.Floor((dms - d) * 100);
            s = Math.Round(((dms - d) * 100 - m) * 100, 3);//精确到0.1秒,但是为了精度，还是保留到0.001秒
            return d + "°" + m + "′" + s + "″";
        }
        #endregion 
    }
}
