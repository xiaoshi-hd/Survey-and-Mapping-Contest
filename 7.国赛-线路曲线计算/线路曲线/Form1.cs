using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace 线路曲线
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region 全局变量
        double a1, a2, a3;
        double Xmin, Xmax, Ymax, Ymin;
        double m, P, B;
        double zhuan1, zhuan2;
        double k1, k2;
        double T, L, E, q1, Ls, Th, Lh, Eh, q2;
        double R1, R2;
        double zhuangju;
        List<Pointl> yizhi;
        List<Pointl> yuanzhu;
        List<Pointl> huanzhu;
        List<Pointl> S;
        static Bitmap image;
        #endregion
        public void qingkong1()
        {
            dataGridView1.Rows.Clear();
        }
        public void qingkong2()
        {
            a1 = a2 = a3 = 0;
            Xmin = Xmax = Ymin = Ymax = 0;
            m = P = B = 0;
            zhuan1 = zhuan2 = 0;
            k1 = k2 = 0;
            T = L = E = q1 = Ls = Th = Lh = Eh = q2 = 0;
            R1 = R2 = 0;
            zhuangju = 10;
            yuanzhu = new List<Pointl>();
            huanzhu = new List<Pointl>();
            yizhi = new List<Pointl>();
            S = new List<Pointl>();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();
        }
        private void 文件打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qingkong1();
            openFileDialog1.Title = "散点数据文件打开";
            openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                #region txt文档读取
                if (openFileDialog1.FilterIndex == 1)
                {
                    StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default);
                    for (int i = 0; !sr.EndOfStream; i++)
                    {
                        string[] iteam = sr.ReadLine().Split(',');
                        dataGridView1.Rows.Add();
                        for (int j = 0; j < iteam.Length;j++ )
                        {
                            dataGridView1.Rows[i].Cells[j].Value = iteam[j];
                        }
                    }
                    sr.Close();
                }
                #endregion
            }
        }
        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qingkong2();
            #region 预处理
            for (int i=0;i<dataGridView1.Rows.Count;i++)
            {
                Pointl ff=new Pointl();
                ff.dianhao = dataGridView1.Rows[i].Cells[0].Value.ToString();
                ff.X=Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);     //数据格式不同，颠倒输入
                ff.Y=Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                yizhi.Add(ff);                                                   //输出至已知数组
                Xmax = Xmax < yizhi[i].X ? yizhi[i].X : Xmax;
                Xmin = Xmin > yizhi[i].X ? yizhi[i].X : Xmin;
                Ymax = Ymax < yizhi[i].Y ? yizhi[i].Y : Ymax;
                Ymin = Ymin > yizhi[i].Y ? yizhi[i].Y : Ymin;
            }

            R1 = Convert.ToDouble(dataGridView1.Rows[1].Cells[3].Value);
            R2 = Convert.ToDouble(dataGridView1.Rows[2].Cells[3].Value);
            Ls = Convert.ToDouble(dataGridView1.Rows[2].Cells[4].Value);

            a1 = Class1.fangwei(yizhi[0].X, yizhi[0].Y, yizhi[1].X, yizhi[1].Y);//计算各边方位角
            a2 = Class1.fangwei(yizhi[1].X, yizhi[1].Y, yizhi[2].X, yizhi[2].Y);
            a3 = Class1.fangwei(yizhi[2].X, yizhi[2].Y, yizhi[3].X, yizhi[3].Y);
            zhuan1 = a2 - a1;
            MessageBox.Show(Class1.hudutodms(a1).ToString());
            MessageBox.Show(Class1.hudutodms(a2).ToString());
            MessageBox.Show(Class1.hudutodms(a3).ToString());
            
            if(zhuan1>0)                                                        //计算转角系数
                k1=1;
            else
                k1=-1;
            zhuan1 = Math.Abs(zhuan1);
            zhuan2 = a3 - a2;
            if(zhuan2>0)
                k2=1;
            else
                k2 = -1;
            zhuan2 = Math.Abs(zhuan2);
            #endregion
            #region 圆曲线要素及主点计算
            T = R1 * Math.Tan(zhuan1 / 2);                                                  //圆曲线要素
            L = R1 * zhuan1;
            E = R1 * (1 / Math.Cos(zhuan1 / 2) - 1);
            q1 = 2 * T - L;
            double KJD1 = Math.Sqrt((yizhi[1].X - yizhi[0].X) * (yizhi[1].X - yizhi[0].X) + (yizhi[1].Y - yizhi[0].Y) * (yizhi[1].Y - yizhi[0].Y));
            Pointl f = new Pointl();                                                       //储存点信息，重新使用需再次实例化
            f.dianhao = "ZY";                                           
            f.X = yizhi[1].X - T * Math.Cos(a1);
            f.Y = yizhi[1].Y - T * Math.Sin(a1);
            f.licheng = KJD1 - T;
            yuanzhu.Add(f);
            double KQZ1 = KJD1 - T + L / 2;                                                 //曲中点里程
            yuanzhu.Add(Class1.yuanquxian1(f, KQZ1, "QZ", R1, a1, k1));                     //通过类计算曲中点位信息
            f = new Pointl();
            f.dianhao = "YZ";
            f.X = yizhi[1].X+ T * Math.Cos(a2);
            f.Y = yizhi[1].Y + T * Math.Sin(a2);
            f.licheng = KJD1 - T + L;
            if (2*T-q1!=L)
            {
                MessageBox.Show("数据错误");
                return;
            }
            yuanzhu.Add(f);
            #endregion
            #region 缓和曲线要素及主点计算
            m = Ls / 2 - Ls * Ls * Ls / (240 * R2 * R2);                                    //计算缓和曲线要素
            P = Ls * Ls / (24 * R2);
            B = Ls / (2 * R2);
            Th = m + (R2 + P) * Math.Tan(zhuan2 / 2);
            Lh = R2 * (zhuan2 - 2 * B) + 2 * Ls;
            Eh = (R2 + P) * (1 / Math.Cos(zhuan2 / 2)) - R2;
            q2 = 2 * Th - Lh;
            double KJD2 = yuanzhu[2].licheng + Math.Sqrt((yizhi[2].X - yuanzhu[2].X) * (yizhi[2].X - yuanzhu[2].X) + (yizhi[2].Y - yuanzhu[2].Y) * (yizhi[2].Y - yuanzhu[2].Y));//计算交点2里程
            f = new Pointl();
            f.dianhao = "ZH";
            f.X = yizhi[2].X - Th * Math.Cos(a2);
            f.Y = yizhi[2].Y - Th * Math.Sin(a2);
            f.licheng = KJD2 - Th;
            huanzhu.Add(f);
            double KHY = KJD2 - Th + Ls;
            huanzhu.Add(Class1.huanquxian1(f, KHY, "HY", R2, Ls, a2, k2));
            double KQZ2 = KJD2 - Th + Lh / 2;
            huanzhu.Add(Class1.huanquxian2(f, KQZ2, "QZ", R2, B, m, P, Ls, a2, k2));
            f = new Pointl();
            f.dianhao = "HZ";
            f.X = yizhi[2].X + Th * Math.Cos(a3);
            f.Y = yizhi[2].Y + Th * Math.Sin(a3);
            f.licheng = KJD2 - Th + Lh;
            double KYH = KJD2 - Th + Lh - Ls;
            huanzhu.Add(Class1.huanquxian3(f, KYH, "YH", R2, Ls, a3, k2));
            huanzhu.Add(f);
            #endregion
            #region 主点及要素输出
            for(int i=0;i<yuanzhu.Count;i++)                                       //输出圆曲线主点
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i].Cells[0].Value = yuanzhu[i].dianhao;
                dataGridView2.Rows[i].Cells[1].Value = Math.Round(yuanzhu[i].X,3);
                dataGridView2.Rows[i].Cells[2].Value = Math.Round(yuanzhu[i].Y,3);
                dataGridView2.Rows[i].Cells[3].Value = Math.Round(yuanzhu[i].licheng,3);
            }
            dataGridView2.Rows[0].HeaderCell.Value = "圆曲线";                     //添加行名
            for (int i = 0; i < huanzhu.Count; i++)                               //输出缓和曲线主点
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i + yuanzhu.Count].Cells[0].Value = huanzhu[i].dianhao;
                dataGridView2.Rows[i + yuanzhu.Count].Cells[1].Value = Math.Round(huanzhu[i].X,3);
                dataGridView2.Rows[i + yuanzhu.Count].Cells[2].Value = Math.Round(huanzhu[i].Y,3);
                dataGridView2.Rows[i + yuanzhu.Count].Cells[3].Value = Math.Round(huanzhu[i].licheng, 3);
            }
            dataGridView2.Rows[yuanzhu.Count].HeaderCell.Value = "缓和曲线";
            dataGridView2.RowHeadersWidth = 100;                                    //对行名宽度赋值
            dataGridView3.Rows.Add();                                               //输出曲线要素
            dataGridView3.Rows[0].HeaderCell.Value = "圆曲线";
            dataGridView3.Rows[0].Cells[0].Value = "线路转角α:";
            dataGridView3.Rows[0].Cells[1].Value = Class1.hudutodms(zhuan1);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[1].Cells[0].Value = "切线长T: ";
            dataGridView3.Rows[1].Cells[1].Value = Math.Round(T,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[2].Cells[0].Value = "曲线长L: ";
            dataGridView3.Rows[2].Cells[1].Value = Math.Round(L,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[3].Cells[0].Value = "外矢距E:";
            dataGridView3.Rows[3].Cells[1].Value = Math.Round(E,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[4].Cells[0].Value = "切曲差q:";
            dataGridView3.Rows[4].Cells[1].Value = Math.Round(q1,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[5].HeaderCell.Value = "缓和曲线";
            dataGridView3.Rows[5].Cells[0].Value = "线路转角α:";
            dataGridView3.Rows[5].Cells[1].Value = Class1.hudutodms(zhuan2);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[6].Cells[0].Value = "切线长TH:";
            dataGridView3.Rows[6].Cells[1].Value = Math.Round(Th,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[7].Cells[0].Value = "曲线长LH:";
            dataGridView3.Rows[7].Cells[1].Value = Math.Round(Lh,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[8].Cells[0].Value = "外矢距EH:";
            dataGridView3.Rows[8].Cells[1].Value = Math.Round(Eh,3);
            dataGridView3.Rows.Add();
            dataGridView3.Rows[9].Cells[0].Value = "切曲差q:";
            dataGridView3.Rows[9].Cells[1].Value = Math.Round(q2, 3);
            dataGridView3.RowHeadersWidth = 100;
            #endregion
            #region 里程桩计算
            yizhi[0].licheng = 0;
            yizhi[3].licheng = huanzhu[4].licheng + Math.Sqrt((yizhi[3].X - huanzhu[4].X) * (yizhi[3].X - huanzhu[4].X) + (yizhi[3].Y - huanzhu[4].Y) * (yizhi[3].Y - huanzhu[4].Y));
            for (double licheng = yizhi[0].licheng; licheng < yizhi[3].licheng; licheng += zhuangju)//循环判断点所处线形并赋值
            {
                if (licheng > yizhi[0].licheng && licheng < yuanzhu[0].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.zhixian(yizhi[0], yizhi[1], licheng, dianhao));
                }
                if (licheng > yuanzhu[0].licheng && licheng < yuanzhu[2].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.yuanquxian1(yuanzhu[0], licheng, dianhao, R1, a1, k1));
                }
                if (licheng > yuanzhu[2].licheng && licheng < huanzhu[0].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.zhixian(yuanzhu[2], yizhi[2], licheng, dianhao));
                }
                if (licheng > huanzhu[0].licheng && licheng < huanzhu[1].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.huanquxian1(huanzhu[0], licheng, dianhao, R2, Ls, a2, k2));
                }
                if (licheng > huanzhu[1].licheng && licheng < huanzhu[3].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.huanquxian2(huanzhu[0], licheng, dianhao, R2, B, m, P, Ls, a2, k2));
                }
                if (licheng > huanzhu[3].licheng && licheng < huanzhu[4].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.huanquxian3(huanzhu[4], licheng, dianhao, R2, Ls, a3, k2));
                }
                if (licheng > huanzhu[4].licheng && licheng < yizhi[3].licheng)
                {
                    string dianhao = "K" + licheng;
                    S.Add(Class1.zhixian(huanzhu[4], yizhi[3], licheng, dianhao));
                }
            }
            #endregion
            #region 里程桩输出
            for (int i = 0; i < S.Count; i++)
            {
                dataGridView4.Rows.Add();
                dataGridView4.Rows[i].Cells[0].Value = S[i].dianhao;
                dataGridView4.Rows[i].Cells[1].Value = Math.Round(S[i].X, 3);
                dataGridView4.Rows[i].Cells[2].Value = Math.Round(S[i].Y, 3);
                dataGridView4.Rows[i].Cells[3].Value = Math.Round(S[i].licheng, 3);
            }
            #endregion
            #region 生成计算报告
            richTextBox1.Text += "\n道路曲线要素计算与里程桩计算\n";
            richTextBox1.Text += "\n圆曲线的要素计算成果\n";
            richTextBox1.Text += "\n----------------------------------------\n";
            Class1.daochutoR(richTextBox1, dataGridView2);
            richTextBox1.Text += "\n缓和曲线的要素计算成果\n";
            richTextBox1.Text += "\n----------------------------------------\n";
            Class1.daochutoR(richTextBox1, dataGridView3);
            richTextBox1.Text += "\n里程点计算成果\n";
            richTextBox1.Text += "\n----------------------------------------\n";
            Class1.daochutoR(richTextBox1, dataGridView4);
            #endregion
        }
        private void 文件保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "规则格网计算保存";
                saveFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                    this.richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                    MessageBox.Show("保存成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pen p = new Pen(Color.Black, 2.5f);
            Pen P1 = new Pen(Color.Blue, 2);
            SolidBrush brush = new SolidBrush(Color.Black);
            SolidBrush brush1 = new SolidBrush(Color.Blue);
            SolidBrush brush2 = new SolidBrush(Color.Yellow);
            SolidBrush brush3 = new SolidBrush(Color.Red);
            int width = (int)(Ymax - Ymin + 200);
            int heigth = (int)(Xmax - Xmin + 200);
            image = new Bitmap(width, heigth);
            Graphics g = Graphics.FromImage(image);
            for(int i=0;i<yizhi.Count;i++)
            {
                g.DrawEllipse(p, (int)(yizhi[i].Y - Ymin) - 4 - 100, -(int)(yizhi[i].X - Xmax - 100) - 4, 8, 8);
                g.FillEllipse(brush3, (int)(yizhi[i].Y - Ymin) - 4 - 100, -(int)(yizhi[i].X - Xmax - 100) - 4, 8, 8);
                g.DrawString(yizhi[i].dianhao, new Font("宋体", 20), Brushes.Black, (int)(yizhi[i].Y - Ymin) - 10 - 100, -(int)(yizhi[i].X - Xmax - 10 - 100));
            }
            for (int i = 0; i < yuanzhu.Count; i++)
            {
                g.DrawEllipse(p, (int)(yuanzhu[i].Y - Ymin) - 3 - 100, -(int)(yuanzhu[i].X - Xmax - 100) - 3, 6, 6);
                g.FillEllipse(brush2, (int)(yuanzhu[i].Y - Ymin) - 3 - 100, -(int)(yuanzhu[i].X - Xmax - 100) - 3, 6, 6);
                g.DrawString(yuanzhu[i].dianhao, new Font("宋体", 10), Brushes.Black, (int)(yuanzhu[i].Y - Ymin) - 10 - 100, -(int)(yuanzhu[i].X - Xmax - 100) - 10);
            } 
            for (int i = 0; i < huanzhu.Count; i++)
            {
                g.DrawEllipse(p, (int)(huanzhu[i].Y - Ymin) - 2 - 100, -(int)(huanzhu[i].X - Xmax - 100) - 2, 4, 4);
                g.FillEllipse(brush1, (int)(huanzhu[i].Y - Ymin) - 2 - 100, -(int)(huanzhu[i].X - Xmax - 100) - 2, 4, 4);
                g.DrawString(huanzhu[i].dianhao, new Font("宋体", 10), Brushes.Black, (int)(huanzhu[i].Y - Ymin) - 10 - 100, -(int)(huanzhu[i].X - Xmax - 100) - 10);
            }
            for (int i = 0; i < S.Count; i++)
            {
                g.DrawEllipse(p, (int)(S[i].Y - Ymin) - 1 - 100, -(int)(S[i].X - Xmax - 100) - 1, 2, 2);
                g.FillEllipse(brush, (int)(S[i].Y - Ymin) - 1 - 100, -(int)(S[i].X - Xmax - 100) - 1, 2, 2);
            }

            #region 坐标轴绘制
            PointF[] xpt = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
            PointF[] ypt = new PointF[3] { new PointF(width - 35, heigth - 50), new PointF(width - 50, heigth - 60), new PointF(width - 50, heigth - 40) };
            g.DrawLine(p, 50, heigth - 50, 50, 50);//画x轴
            g.DrawLine(p, 50, heigth - 50, width - 50, heigth - 50);//画y轴
            g.FillPolygon(brush, xpt);//x轴箭头
            g.FillPolygon(brush, ypt);//y轴箭头
            g.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
            g.DrawString("Y", new Font("宋体", 30), Brushes.Black, width - 60, heigth - 40);//注记文字是在点位的右上角绘制
            for (int i = 0; i <= (Xmax - Xmin) * 30; i = i + (int)((Xmax - Xmin) * 30 / 10))
            {
                g.DrawString(((int)(Xmin + i)).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth - 100 - i);
                g.DrawLine(p, 50, heigth - 100 - i, 65, heigth - 100 - i);
            }
            for (int i = 0; i <= (int)(Ymax - Ymin) * 30; i = i + (int)((Ymax - Ymin) * 30 / 10))
            {
                g.DrawString(((int)(Ymin + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth - 40);
                g.DrawLine(p, 100 + i, heigth - 50, 100 + i, heigth - 65);
            }
            #endregion
            pictureBox1.Image = (Image)image;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width < 5000)
            {
                pictureBox1.Width = (int)(pictureBox1.Width * 1.2);
                pictureBox1.Height = (int)(pictureBox1.Height * 1.2);
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width > 100)
            {
                pictureBox1.Width = (int)(pictureBox1.Width / 1.2);
                pictureBox1.Height = (int)(pictureBox1.Height / 1.2);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            文件打开ToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            文件保存ToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            计算ToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            绘图ToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();
            this.InitializeComponent();
        }

        private void 图像保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "保存数据文件";
            saveFileDialog1.Filter = "CAD图形交换文件(*.dxf)|*.dxf|图像文件(*.bmp)|*.bmp";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                #region DXF
                if (saveFileDialog1.FilterIndex == 1)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.Write("0\nSECTION\n");//第一个段的开始
                        #region 表段
                        sw.Write("2\nTABLES\n");//表段的开始
                        sw.Write("0\nTABLE\n");//每个表都由带有标签TABLE的组码0引入
                        #region 图层
                        sw.Write("2\nLAYER\n");//由组码2标识具体表，此处为图层
                        sw.Write("0\nLAYER\n");//每个表条目包括指定条目类型的组码0引入,实体图层
                        sw.Write("70\n");//表中最大条目数
                        sw.Write("0\n");
                        sw.Write("2\nshiti\n");//设置图层名称
                        sw.Write("62\n");//颜色代码
                        sw.Write("10\n");//10表示红色，50表示黄色，170表示蓝色，90表示绿色，130表示青色
                        sw.Write("6\n");//线型名称
                        sw.Write("CONTINUOUS\n");//表示直线

                        sw.Write("0\nLAYER\n");//每个表条目包括指定条目类型的组码0引入,实体图层
                        sw.Write("70\n");//表中最大条目数
                        sw.Write("0\n");
                        sw.Write("2\nzhuji\n");//设置图层名称
                        sw.Write("62\n");//颜色代码
                        sw.Write("50\n");//10表示红色，50表示黄色，170表示蓝色，90表示绿色，130表示青色
                        sw.Write("6\n");//线型名称
                        sw.Write("CONTINUOUS\n");//表示直线
                        #endregion
                        sw.Write("0\nENDTAB\n");//TABLE段结束
                        #endregion
                        sw.Write("0\nENDSEC\n");//第一段结束
                        sw.Write("0\nSECTION\n");//第二个段的开始
                        #region 实体段
                        sw.Write("2\nENTITIES\n");//实体段开始
                        #region 绘点
                        for (int i = 0; i < yizhi.Count; i++)
                        {
                            sw.Write("0\nPOINT\n");//单一点绘制
                            sw.Write("8\n");//图层
                            sw.Write("zhuji\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("10\n");//X坐标
                            sw.Write(yizhi[i].Y + "\n");
                            sw.Write("20\n");//Y坐标
                            sw.Write(yizhi[i].X  + "\n");
                        }
                        for (int i = 0; i < yuanzhu.Count; i++)
                        {
                            sw.Write("0\nPOINT\n");//单一点绘制
                            sw.Write("8\n");//图层
                            sw.Write("zhuji\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("10\n");//X坐标
                            sw.Write(yuanzhu[i].Y + "\n");
                            sw.Write("20\n");//Y坐标
                            sw.Write(yuanzhu[i].X + "\n");
                        }
                        for (int i = 0; i < huanzhu.Count; i++)
                        {
                            sw.Write("0\nPOINT\n");//单一点绘制
                            sw.Write("8\n");//图层
                            sw.Write("zhuji\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("10\n");//X坐标
                            sw.Write(huanzhu[i].Y + "\n");
                            sw.Write("20\n");//Y坐标
                            sw.Write(huanzhu[i].X + "\n");
                        }
                        for (int i = 0; i < S.Count; i++)
                        {
                            sw.Write("0\nPOINT\n");//单一点绘制
                            sw.Write("8\n");//图层
                            sw.Write("zhuji\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("10\n");//X坐标
                            sw.Write(S[i].Y + "\n");
                            sw.Write("20\n");//Y坐标
                            sw.Write(S[i].X + "\n");
                        }
                        #endregion
                        #region 注记
                        for (int i = 0; i < yizhi.Count; i++)
                        {
                            sw.Write("0\nTEXT\n");//单行文字
                            sw.Write("8\n");
                            sw.Write("zhuji\n");
                            sw.Write("10\n");//字体起点X
                            sw.Write(yizhi[i].Y  -  5 + "\n");
                            sw.Write("20\n");//字体起点Y
                            sw.Write(yizhi[i].X  -  5 + "\n");
                            sw.Write("40\n" + 8 + "\n");//字体高度
                            sw.Write("1\n" + Math.Round(yizhi[i].licheng, 3) + "\n");//文字内容
                        }
                        for (int i = 0; i < yuanzhu.Count; i++)
                        {
                            sw.Write("0\nTEXT\n");//单行文字
                            sw.Write("8\n");
                            sw.Write("zhuji\n");
                            sw.Write("10\n");//字体起点X
                            sw.Write(yuanzhu[i].Y - 5 + "\n");
                            sw.Write("20\n");//字体起点Y
                            sw.Write(yuanzhu[i].X - 5 + "\n");
                            sw.Write("40\n" + 8 + "\n");//字体高度
                            sw.Write("1\n" + Math.Round(yuanzhu[i].licheng, 3) + "\n");//文字内容
                        }
                        for (int i = 0; i < huanzhu.Count; i++)
                        {
                            sw.Write("0\nTEXT\n");//单行文字
                            sw.Write("8\n");
                            sw.Write("zhuji\n");
                            sw.Write("10\n");//字体起点X
                            sw.Write(huanzhu[i].Y - 5 + "\n");
                            sw.Write("20\n");//字体起点Y
                            sw.Write(huanzhu[i].X - 5 + "\n");
                            sw.Write("40\n" + 8 + "\n");//字体高度
                            sw.Write("1\n" + Math.Round(huanzhu[i].licheng, 3) + "\n");//文字内容
                        }
                        for (int i = 0; i < S.Count; i++)
                        {
                            sw.Write("0\nTEXT\n");//单行文字
                            sw.Write("8\n");
                            sw.Write("zhuji\n");
                            sw.Write("10\n");//字体起点X
                            sw.Write(S[i].Y - 5 + "\n");
                            sw.Write("20\n");//字体起点Y
                            sw.Write(S[i].X - 5 + "\n");
                            sw.Write("40\n" + 8 + "\n");//字体高度
                            sw.Write("1\n" + Math.Round(S[i].licheng, 3) + "\n");//文字内容
                        }
                        #endregion
                        #endregion
                        sw.Write("0\nENDSEC\n");//第二段结束
                        sw.Write("0\nEOF\n");//文件结束
                        MessageBox.Show("保存成功！");
                    }
                }
                #endregion
                #region BMP
                if (saveFileDialog1.FilterIndex == 2)
                {
                    image.Save(saveFileDialog1.FileName);
                    MessageBox.Show("保存成功！");
                }
                #endregion
            }
        }
    }
}
