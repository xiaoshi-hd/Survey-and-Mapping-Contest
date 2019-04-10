using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9.国赛_大地主题解算
{
    class Caculates
    {
        public static double dmstohudu(double dms)
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
        public static double hudutodms(double hudu)
        {
            double d, m, s;
            int i = 1;
            if (hudu < 0)
            {
                i = -1;
                hudu = Math.Abs(hudu);
            }
            double du = hudu * 180 / Math.PI;
            d = Math.Floor(du);
            m = Math.Floor((du - d) * 60);
            s = ((du - d) * 60 - m) * 60;
            return Math.Round(i * (d + m / 100 + s / 10000), 10);//保留秒的小数点后六位
        }
    }
}
