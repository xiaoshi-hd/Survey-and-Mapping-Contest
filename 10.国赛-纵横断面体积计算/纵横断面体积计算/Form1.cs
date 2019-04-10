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

namespace 纵横断面体积计算
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region 时间控件
        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = DateTime.Now.ToString();
            timer1.Enabled = true;
            timer1.Interval = 1000;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = DateTime.Now.ToString();
        }
        #endregion
        #region 放大缩小
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width < 5000 || pictureBox2.Width < 5000 || pictureBox3.Width < 5000)
            {
                pictureBox1.Width = (int)(pictureBox1.Width * 1.2);
                pictureBox1.Height = (int)(pictureBox1.Height * 1.2);
                pictureBox2.Width = (int)(pictureBox2.Width * 1.2);
                pictureBox2.Height = (int)(pictureBox2.Height * 1.2);
                pictureBox3.Width = (int)(pictureBox3.Width * 1.2);
                pictureBox3.Height = (int)(pictureBox3.Height * 1.2);
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width > 100 || pictureBox2.Width > 100 || pictureBox3.Width > 100)
            {
                pictureBox1.Width = (int)(pictureBox1.Width / 1.2);
                pictureBox1.Height = (int)(pictureBox1.Height / 1.2);
                pictureBox2.Width = (int)(pictureBox2.Width / 1.2);
                pictureBox2.Height = (int)(pictureBox2.Height / 1.2);
                pictureBox3.Width = (int)(pictureBox3.Width / 1.2);
                pictureBox3.Height = (int)(pictureBox3.Height / 1.2);
            }
        }
        #endregion
        #region 刷新
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();
            this.InitializeComponent();
        }
        #endregion
        #region 变量定义
        double gaocheng;//设计高程
        int n;//纵断面段数
        double[] licheng;//纵断面长度
        double[] fangwei;//纵断面方位角和横断面方位角
        Point1[] K;//存储纵断面关键点信息
        Point1[] M;//横断面中心点信息
        Point1[] point1;//用于存储散点数据
        List<Point1> ZDM;//用于存储纵断面内插点信息
        List<Point1> HDM1;//存储单个横断面信息
        List<List<Point1>> HDM;//用于存储横断面内插点信息
        Bitmap image;//存储图片
        Bitmap image1;//存储图片
        Bitmap image2;//存储图片
        #endregion
        #region 文件打开
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            pictureBox1.Image = null;
            richTextBox1.Text = "";

            try
            {
                openFileDialog1.Title = "文件打开";
                openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        string[] str = sr.ReadLine().Split(',');
                        txt_gaocheng.Text = str[1];
                        str = sr.ReadLine().Split(',');
                        txt_dianming.Text = string.Join(",", str);
                        sr.ReadLine();
                        int i = 0;
                        while (!sr.EndOfStream)
                        {
                            str = sr.ReadLine().Split(',');
                            dataGridView1.Rows.Add();
                            for (int j = 0; j < str.Length; j++)
                            {
                                dataGridView1.Rows[i].Cells[j].Value = str[j];
                            }
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            打开ToolStripMenuItem_Click(sender,e);
        }
        #endregion
        #region 计算
        private void 纵断面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZDM = new List<Point1>();//列表必须初始化，数组在使用前必须规定其大小
            dataGridView1.AllowUserToAddRows = false;

            #region 数据导入
            try
            {
                string[] dian;//纵断面关键点
                dian = txt_dianming.Text.Split(',');
                n = dian.Length - 1;//纵断面段数
                gaocheng = Convert.ToDouble(txt_gaocheng.Text);
                point1 = new Point1[dataGridView1.Rows.Count];//先规定点数组大小
                K = new Point1[dian.Length];
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    point1[i] = new Point1();//实例化一个点
                    point1[i].dianhao = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    point1[i].X = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value.ToString().Replace(" ", ""));
                    point1[i].Y = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value.ToString().Replace(" ", ""));
                    point1[i].Z = Convert.ToDouble(dataGridView1.Rows[i].Cells[3].Value.ToString().Replace(" ", ""));
                    for (int j = 0; j < dian.Length; j++)
                    {
                        if (point1[i].dianhao == dian[j])//寻找关键点并储存
                        {
                            K[j] = new Point1();
                            K[j].dianhao = point1[i].dianhao;
                            K[j].X = point1[i].X;
                            K[j].Y = point1[i].Y;
                            K[j].Z = point1[i].Z;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("请输入正确的数据！");
                return;
            }
            //MessageBox.Show(K.Length.ToString());
            #endregion
            try
            {
                #region 纵断面内插点平面坐标
                fangwei = new double[n];//存储纵断面方位角信息
                licheng = new double[n];//存储纵断面距离信息
                K[0].licheng = 0;
                int k = 0;
                for (int i = 0; i < n; i++)//关键点里程
                {
                    fangwei[i] = caculate.fangwei(K[i], K[i + 1]);
                    K[i + 1].licheng = K[i].licheng + caculate.juli(K[i], K[i + 1]);
                    licheng[i] = K[i + 1].licheng;

                    while (true)
                    {
                        k = k + 10;//纵断面内插间距为10米
                        if (k < licheng[i])
                        {
                            Point1 p1 = new Point1();
                            p1.dianhao = "V-" + (k / 10);
                            p1.licheng = k;
                            p1.X = K[i].X + (p1.licheng - K[i].licheng) * Math.Cos(fangwei[i]);
                            p1.Y = K[i].Y + (p1.licheng - K[i].licheng) * Math.Sin(fangwei[i]);
                            ZDM.Add(p1);
                        }
                        else
                        {
                            k = k - 10;
                            break;
                        }
                    }
                }
                //MessageBox.Show(ZDM.Count.ToString());
                #endregion
                #region 纵断面内插点高程
                double d;
                int dianhao = 0;
                for (int i = 0; i < ZDM.Count; i++)//对每个内插点都进行计算
                {
                    double dmin1 = 0;
                    double HD = 0, LD = 0;//计算插值点高程
                    for (int q = 0; q < 5; q++)//寻找最近的5个点
                    {
                        double dmin = 1000000000000;
                        for (int j = 0; j < point1.Length; j++)//遍历所有散点
                        {
                            d = caculate.juli(ZDM[i], point1[j]);
                            if (dmin > d && d > dmin1)
                            {
                                dmin = d;
                                dianhao = j;
                            }
                        }
                        dmin1 = dmin;
                        //MessageBox.Show(dmin.ToString());
                        HD = HD + point1[dianhao].Z / dmin;
                        LD = LD + 1 / dmin;
                    }
                    ZDM[i].Z = HD / LD;
                }
                #endregion
                #region 纵断面面积
                ZDM.Add(K[n]);
                for (int i = n - 1; i > 0; i--)
                {
                    for (int j = ZDM.Count - 1; j > 0; j--)
                    {
                        if (K[i].licheng > ZDM[j].licheng)
                        {
                            ZDM.Insert(j + 1,K[i]);
                            break;
                        }
                    }
                }
                ZDM.Insert(0, K[0]);
                //MessageBox.Show(ZDM.Count.ToString());
                double S = 0;
                for (int i = 0; i < ZDM.Count - 1; i++)
                {
                    S = S + ((ZDM[i].Z + ZDM[i + 1].Z - 2 * gaocheng) * 10 / 2);
                }
                #endregion
                #region 计算报告
                richTextBox1.Text = "纵横断面计算结果\n\n";
                richTextBox1.Text += "纵断面信息\n------------------------------------------------------------\n";
                richTextBox1.Text += "纵断面面积：  " + Math.Round(S, 3) + "\n";
                richTextBox1.Text += "纵断面全长：  " + Math.Round(licheng.Max(), 3) + "\n";
                richTextBox1.Text += "线路主点：\n";
                richTextBox1.Text += "点名    \t里程K(m)    \tX坐标(m)    \tY坐标(m)    \tH坐标(m)\n";
                for (int i = 0; i < ZDM.Count; i++)
                {
                    string str1;
                    string[] str = new string[5];
                    str[0] = string.Format("{0,-8}", ZDM[i].dianhao);
                    str[1] = string.Format("{0,-8}", Math.Round(ZDM[i].licheng, 3));
                    str[2] = string.Format("{0,-8}", Math.Round(ZDM[i].X, 3));
                    str[3] = string.Format("{0,-8}", Math.Round(ZDM[i].Y, 3));
                    str[4] = string.Format("{0,-8}", Math.Round(ZDM[i].Z, 3));
                    str1 = string.Join("\t",str);
                    richTextBox1.Text += str1 + "\n\n";
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 横断面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                HDM = new List<List<Point1>>();
                #region 横断面中心点信息
                M = new Point1[n];
                for (int i = 0; i < n; i++)
                {
                    M[i] = new Point1();
                    M[i].dianhao = "M " + (i + 1);
                    M[i].licheng = 25;
                    M[i].X = (K[i].X + K[i + 1].X) / 2;
                    M[i].Y = (K[i].Y + K[i + 1].Y) / 2;

                    fangwei[i] = fangwei[i] - Math.PI / 2;//跟书上公式不一样
                }
                #endregion 
                //MessageBox.Show(M[0].X.ToString());
                #region 横断面插值坐标
                for (int i = 0; i < n; i++)
                {
                    int k = 0;
                    HDM1 = new List<Point1>();//不知道为什么不能用clear
                    for (int j = -25; j <= 25; j = j + 5)//延伸25米
                    {
                        if (j != 0)
                        {
                            Point1 p = new Point1();
                            p.dianhao = "C" + (j / 5);
                            p.licheng = k;
                            p.X = M[i].X + j * Math.Cos(fangwei[i]);
                            p.Y = M[i].Y + j * Math.Sin(fangwei[i]);
                            HDM1.Add(p);
                        }
                        else 
                        {
                            HDM1.Add(M[i]);
                        }
                        k = k + 5;
                    }
                    //MessageBox.Show(HDM1[10].X.ToString());
                    HDM.Add(HDM1);
                    //MessageBox.Show(HDM[0][10].X.ToString());
                }
                #endregion 
                #region 横断面内插高程
                for (int i = 0; i < n; i++)
                {
                    double d;
                    int dianhao = 0;
                    for (int j = 0; j < HDM[i].Count; j++)//对每个内插点都进行计算
                    {
                        double dmin1 = 0;
                        double HD = 0, LD = 0;//计算插值点高程
                        for (int q = 0; q < 5; q++)//寻找最近的5个点
                        {
                            double dmin = 1000000000000;
                            for (int q1 = 0; q1 < point1.Length; q1++)//遍历所有散点
                            {
                                d = caculate.juli(HDM[i][j], point1[q1]);
                                if (dmin > d && d > dmin1)
                                {
                                    dmin = d;
                                    dianhao = q1;
                                }
                            }
                            dmin1 = dmin;//存储最小值，次小值，方便排序
                            //MessageBox.Show(dmin.ToString());
                            HD = HD + point1[dianhao].Z / dmin;
                            LD = LD + 1 / dmin;
                        }
                        HDM[i][j].Z = HD / LD;
                    }
                }
                //MessageBox.Show(HDM1[10].X.ToString());
                #endregion
                #region 横断面面积
                double[] S = new double[n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < HDM[i].Count - 1; j++)
                    {
                        S[i] = S[i] + ((HDM[i][j].Z + HDM[i][j + 1].Z - 2 * gaocheng) * 5 / 2);
                    }
                }
                #endregion
                #region 计算报告
                richTextBox1.Text += "横断面信息\n------------------------------------------------------------\n";
                for (int i = 0; i < n; i++)
                {
                    richTextBox1.Text += "横断面： " + (i + 1) + "\n------------------------------\n";
                    richTextBox1.Text += "横断面面积：  " + Math.Round(S[i], 3) + "\n";
                    richTextBox1.Text += "横断面全长：  " + 50 + "\n";
                    richTextBox1.Text += "线路主点：\n";
                    richTextBox1.Text += "点名    \t里程K(m)    \tX坐标(m)    \tY坐标(m)    \tH坐标(m)\n";
                    for (int j = 0; j < HDM[i].Count; j++)
                    {
                        string str1;
                        string[] str = new string[5];
                        str[0] = string.Format("{0,-8}", HDM[i][j].dianhao);
                        str[1] = string.Format("{0,-8}", Math.Round(HDM[i][j].licheng, 3));
                        str[2] = string.Format("{0,-8}", Math.Round(HDM[i][j].X, 3));
                        str[3] = string.Format("{0,-8}", Math.Round(HDM[i][j].Y, 3));
                        str[4] = string.Format("{0,-8}", Math.Round(HDM[i][j].Z, 3));
                        str1 = string.Join("\t", str);
                        richTextBox1.Text += str1 + "\n\n";
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 一键计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            纵断面ToolStripMenuItem_Click(sender, e);
            横断面ToolStripMenuItem_Click(sender, e);
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            一键计算ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 绘图
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                #region 绘制散点图
                double xmax1 = point1[0].X, ymax1 = point1[0].Y;//散点图图形范围需要
                double xmin1 = point1[0].X, ymin1 = point1[0].Y;
                for (int i = 0; i < point1.Length; i++)
                {
                    xmax1 = xmax1 > point1[i].X ? xmax1 : point1[i].X;
                    xmin1 = xmin1 < point1[i].X ? xmin1 : point1[i].X;
                    ymax1 = ymax1 > point1[i].Y ? ymax1 : point1[i].Y;
                    ymin1 = ymin1 < point1[i].Y ? ymin1 : point1[i].Y;
                }
                Pen p = new Pen(Color.Black, 2.5f);//定义画笔
                Pen p2 = new Pen(Color.Blue, 2);
                SolidBrush brush = new SolidBrush(Color.Black);//定义填充
                int width1 = (int)(ymax1 - ymin1) * 30 + 200;//图幅长
                int heigth1 = (int)(xmax1 - xmin1) * 30 + 200;//图幅宽
                image = new Bitmap(width1, heigth1);
                Graphics g = Graphics.FromImage(image);
                #region 绘制线型
                for (int i = 0; i < n; i++)
                {
                    g.DrawLine(p, (int)(K[i].Y - ymin1) * 30 + 100, -(int)(K[i].X - xmax1) * 30 + 100, (int)(K[i + 1].Y - ymin1) * 30 + 100, -(int)(K[i + 1].X - xmax1) * 30 + 100);
                    g.DrawLine(p, (int)(HDM[i][0].Y - ymin1) * 30 + 100, -(int)(HDM[i][0].X - xmax1) * 30 + 100, (int)(HDM[i][HDM[i].Count - 1].Y - ymin1) * 30 + 100, -(int)(HDM[i][HDM[i].Count - 1].X - xmax1) * 30 + 100);
                }
                #endregion
                #region 绘制注记，点
                for (int i = 0; i < point1.Length; i++)
                {
                    g.FillEllipse(brush, (float)(point1[i].Y - ymin1) * 30 + 100 - 2.5f, -(float)(point1[i].X - xmax1 ) * 30+100 - 2.5f, 5, 5);
                    g.DrawString(point1[i].dianhao, new Font("宋体", 10), Brushes.Red, (float)(point1[i].Y - ymin1) * 30 + 100 - 2.5f, -(float)(point1[i].X - xmax1) * 30 + 100 - 2.5f);
                }
                #endregion
                #region 绘制坐标轴
                PointF[] xpt = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
                PointF[] ypt = new PointF[3] { new PointF(width1 - 35, heigth1 - 50), new PointF(width1 - 50, heigth1 - 60), new PointF(width1 - 50, heigth1 - 40) };
                g.DrawLine(p, 50, heigth1 - 50, 50, 50);//画x轴
                g.DrawLine(p, 50, heigth1 - 50, width1 - 50, heigth1 - 50);//画y轴
                g.FillPolygon(brush, xpt);//x轴箭头
                g.FillPolygon(brush, ypt);//y轴箭头
                g.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
                g.DrawString("Y", new Font("宋体", 30), Brushes.Black, width1 - 60, heigth1 - 40);//注记文字是在点位的右上角绘制
                for (int i = 0; i <= (xmax1 - xmin1) * 30; i = i + (int)((xmax1 - xmin1) * 30 / 10))
                {
                    g.DrawString(((int)(xmin1 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth1 - 100 - i);
                    g.DrawLine(p, 50, heigth1 - 100 - i, 65, heigth1 - 100 - i);
                }
                for (int i = 0; i <= (int)(ymax1 - ymin1) * 30; i = i + (int)((ymax1 - ymin1) * 30 / 10))
                {
                    g.DrawString(((int)(ymin1 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth1 - 40);
                    g.DrawLine(p, 100 + i, heigth1 - 50, 100 + i, heigth1 - 65);
                }
                #endregion
                pictureBox1.Image = (Image)image;
                #endregion
                #region 绘制纵断面图
                double xmax2 = ZDM[0].Z, ymax2 = ZDM[0].licheng;//纵断面
                double xmin2 = ZDM[0].Z, ymin2 = ZDM[0].licheng;
                for (int i = 0; i < ZDM.Count; i++)
                {
                    xmax2 = xmax2 > ZDM[i].Z ? xmax2 : ZDM[i].Z;
                    xmin2 = xmin2 < ZDM[i].Z ? xmin2 : ZDM[i].Z;
                    ymax2 = ymax2 > ZDM[i].licheng ? ymax2 : ZDM[i].licheng;
                    ymin2 = ymin2 < ZDM[i].licheng ? ymin2 : ZDM[i].licheng;
                }
                //xmin2 = xmin2 < gaocheng ? xmin2 : gaocheng;
                int width2 = (int)(ymax2 - ymin2) * 30 + 200;//图幅长
                int heigth2 = (int)(xmax2 - xmin2) * 30 + 200;//图幅宽
                image1 = new Bitmap(width2, heigth2);
                Graphics g1 = Graphics.FromImage(image1);
                #region 定义充填
                PointF[] pf1 = new PointF[ZDM.Count+3];
                for (int i = 0; i < ZDM.Count; i++)
                {
                    pf1[i].X = (float)((ZDM[i].licheng - ymin2) * 30 + 100);
                    pf1[i].Y = -(float)((ZDM[i].Z - xmax2) * 30 - 100);
                }
                pf1[ZDM.Count].X = (float)((licheng.Max() - ymin2) * 30 + 100);
                pf1[ZDM.Count].Y = -(float)((gaocheng - xmax2) * 30 - 100);
                pf1[ZDM.Count + 1].X = (float)((0 - ymin2) * 30 + 100);
                pf1[ZDM.Count + 1].Y = -(float)((gaocheng - xmax2) * 30 - 100);
                pf1[ZDM.Count + 2].X = (float)((ZDM[0].licheng - ymin2) * 30 + 100);
                pf1[ZDM.Count + 2].Y = -(float)((ZDM[0].Z - xmax2) * 30 - 100);
                g1.FillPolygon(new SolidBrush(Color.Yellow), pf1);
                #endregion
                #region 绘制注记，点
                for (int i = 0; i < ZDM.Count; i++)
                {
                    g1.FillEllipse(brush, (float)(ZDM[i].licheng - ymin2) * 30 + 100 - 2.5f, -(float)(ZDM[i].Z - xmax2) * 30 + 100 - 2.5f, 5, 5);
                    g1.DrawString(ZDM[i].dianhao, new Font("宋体", 10), Brushes.Red, (float)(ZDM[i].licheng - ymin2) * 30 + 100 - 2.5f, -(float)(ZDM[i].Z - xmax2) * 30 + 100 - 2.5f);
                }
                #endregion
                PointF[] xpt1 = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
                PointF[] ypt1 = new PointF[3] { new PointF(width2 - 35, heigth2 - 50), new PointF(width2 - 50, heigth2 - 60), new PointF(width2 - 50, heigth2 - 40) };
                g1.DrawLine(p, 50, heigth2 - 50, 50, 50);//画x轴
                g1.DrawLine(p, 50, heigth2 - 50, width2 - 50, heigth2 - 50);//画y轴
                g1.FillPolygon(brush, xpt);//x轴箭头
                g1.FillPolygon(brush, ypt);//y轴箭头
                g1.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
                g1.DrawString("Y", new Font("宋体", 30), Brushes.Black, width2 - 60, heigth2 - 40);//注记文字是在点位的右上角绘制
                for (int i = 0; i <= (xmax2 - xmin2) * 30; i = i + (int)((xmax2 - xmin2) * 30 / 10))
                {
                    g1.DrawString(((int)(xmin2 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth2 - 100 - i);
                    g1.DrawLine(p, 50, heigth2 - 100 - i, 65, heigth2 - 100 - i);
                }
                for (int i = 0; i <= (int)(ymax2 - ymin2) * 30; i = i + (int)((ymax2 - ymin2) * 30 / 10))
                {
                    g1.DrawString(((int)(ymin2 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth2 - 40);
                    g1.DrawLine(p, 100 + i, heigth2 - 50, 100 + i, heigth2 - 65);
                }
                #endregion
                pictureBox2.Image = (Image)image1;
                #endregion
                #region 绘制横断面图
                #region 图形范围
                List<double> Xmax = new List<double>();
                List<double> Ymax = new List<double>();
                List<double> Xmin = new List<double>();
                List<double> Ymin = new List<double>();
                for (int j = 0; j < n; j++)
                {
                    double xmax3 = HDM[j][0].Z, ymax3 = HDM[j][0].licheng;//横断面
                    double xmin3 = HDM[j][0].Z, ymin3 = HDM[j][0].licheng;
                    for (int i = 0; i < HDM[j].Count; i++)
                    {
                        xmax3 = xmax3 > HDM[j][i].Z ? xmax3 : HDM[j][i].Z;
                        xmin3 = xmin3 < HDM[j][i].Z ? xmin3 : HDM[j][i].Z;
                        ymax3 = ymax3 > HDM[j][i].licheng ? ymax3 : HDM[j][i].licheng;
                        ymin3 = ymin3 < HDM[j][i].licheng ? ymin3 : HDM[j][i].licheng;
                    }
                    Xmax.Add(xmax3);
                    Xmin.Add(xmin3);
                    Ymax.Add(ymax3);
                    Ymin.Add(ymin3);
                }
                int width3 = (int)(100 * n * 30 + 200);//图幅长
                int heigth3 = (int)((Xmax[0] - Xmin[0] + Xmax[1] - Xmin[1]) * 50 + 100 + 200);//图幅宽
                image2 = new Bitmap(width3, heigth3);
                Graphics g2 = Graphics.FromImage(image2);
                #endregion
                #region 断面图1
                #region 定义充填
                PointF[] pf2 = new PointF[HDM[0].Count + 2];
                for (int i = 0; i < HDM[0].Count; i++)
                {
                    pf2[i].X = (float)((HDM[0][i].licheng -Ymin.Min()) * 30 + 100);
                    pf2[i].Y = -(float)((HDM[0][i].Z - Xmax.Max()) * 50 - 100);
                }
                pf2[HDM[0].Count].X = (float)((HDM[0][HDM[0].Count-1].licheng - Ymin.Min()) * 30 + 100);
                pf2[HDM[0].Count].Y = -(float)((gaocheng - Xmax.Max()) * 50 - 100);
                pf2[HDM[0].Count + 1].X = (float)((HDM[0][0].licheng - Ymin.Min()) * 30 + 100);
                pf2[HDM[0].Count + 1].Y = -(float)((gaocheng - Xmax.Max()) * 50 - 100);
                g2.FillPolygon(new SolidBrush(Color.Yellow), pf2);
                #endregion
                #region 绘制注记，点
                for (int i = 0; i < HDM[0].Count; i++)
                {
                    g2.FillEllipse(brush, (float)((HDM[0][i].licheng - Ymin.Min()) * 30 + 100) - 2.5f, -(float)((HDM[0][i].Z - Xmax.Max()) * 50 - 100) - 2.5f, 5, 5);
                    g2.DrawString(HDM[0][i].dianhao, new Font("宋体", 10), Brushes.Red, (float)((HDM[0][i].licheng - Ymin.Min()) * 30 + 100) - 2.5f, -(float)((HDM[0][i].Z - Xmax.Max()) * 50 - 100) - 2.5f);
                }
                #endregion
                #region 绘制坐标轴
                //PointF[] xpt2 = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
                //PointF[] ypt2 = new PointF[3] { new PointF((int)(Ymax[0] - Ymin[0]) * 30 + 200 - 35, heigth2 - 50), new PointF(width2 - 50, heigth2 - 60), new PointF(width2 - 50, heigth2 - 40) };
                //g1.DrawLine(p, 50, heigth2 - 50, 50, 50);//画x轴
                //g1.DrawLine(p, 50, heigth2 - 50, width2 - 50, heigth2 - 50);//画y轴
                //g1.FillPolygon(brush, xpt);//x轴箭头
                //g1.FillPolygon(brush, ypt);//y轴箭头
                //g1.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
                //g1.DrawString("Y", new Font("宋体", 30), Brushes.Black, width2 - 60, heigth2 - 40);//注记文字是在点位的右上角绘制
                //for (int i = 0; i <= (xmax2 - xmin2) * 30; i = i + (int)((xmax2 - xmin2) * 30 / 10))
                //{
                //    g1.DrawString(((int)(xmin2 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth2 - 100 - i);
                //    g1.DrawLine(p, 50, heigth2 - 100 - i, 65, heigth2 - 100 - i);
                //}
                //for (int i = 0; i <= (int)(ymax2 - ymin2) * 30; i = i + (int)((ymax2 - ymin2) * 30 / 10))
                //{
                //    g1.DrawString(((int)(ymin2 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth2 - 40);
                //    g1.DrawLine(p, 100 + i, heigth2 - 50, 100 + i, heigth2 - 65);
                //}
                #endregion
                #endregion
                #region 断面图2
                #region 定义充填
                PointF[] pf3 = new PointF[HDM[0].Count + 2];
                for (int i = 0; i < HDM[1].Count; i++)
                {
                    pf3[i].X = (float)((HDM[1][i].licheng - Ymin.Min()) * 30  + Ymax[0]*30 + 200);
                    pf3[i].Y = -(float)((HDM[1][i].Z - Xmax.Max()) * 50 - 100);
                }
                pf3[HDM[1].Count].X = (float)((HDM[1][HDM[1].Count - 1].licheng - Ymin.Min()) * 30  + Ymax[0] * 30 + 200);
                pf3[HDM[1].Count].Y = -(float)((gaocheng - Xmax.Max()) * 50 - 100);
                pf3[HDM[1].Count + 1].X = (float)((0 - Ymin.Min()) * 30  + Ymax[0] * 30 + 200 );
                pf3[HDM[1].Count + 1].Y = -(float)((gaocheng - Xmax.Max()) * 50 - 100);
                g2.FillPolygon(new SolidBrush(Color.Yellow), pf3);
                #endregion
                #region 绘制注记，点
                for (int i = 0; i < HDM[1].Count; i++)
                {
                    g2.FillEllipse(brush, (float)((HDM[1][i].licheng - Ymin.Min()) * 30 + Ymax[0] * 30 + 200) - 2.5f, -(float)((HDM[1][i].Z - Xmax.Max()) * 50 - 100) - 2.5f, 5, 5);
                    g2.DrawString(HDM[1][i].dianhao, new Font("宋体", 10), Brushes.Red, (float)((HDM[1][i].licheng - Ymin.Min()) * 30 + Ymax[0] * 30 + 200) - 2.5f, -(float)((HDM[1][i].Z - Xmax.Max()) * 50 - 100) - 2.5f);
                }
                #endregion
                #region 绘制坐标轴
                //PointF[] xpt2 = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
                //PointF[] ypt2 = new PointF[3] { new PointF((int)(Ymax[0] - Ymin[0]) * 30 + 200 - 35, heigth2 - 50), new PointF(width2 - 50, heigth2 - 60), new PointF(width2 - 50, heigth2 - 40) };
                //g1.DrawLine(p, 50, heigth2 - 50, 50, 50);//画x轴
                //g1.DrawLine(p, 50, heigth2 - 50, width2 - 50, heigth2 - 50);//画y轴
                //g1.FillPolygon(brush, xpt);//x轴箭头
                //g1.FillPolygon(brush, ypt);//y轴箭头
                //g1.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
                //g1.DrawString("Y", new Font("宋体", 30), Brushes.Black, width2 - 60, heigth2 - 40);//注记文字是在点位的右上角绘制
                //for (int i = 0; i <= (xmax2 - xmin2) * 30; i = i + (int)((xmax2 - xmin2) * 30 / 10))
                //{
                //    g1.DrawString(((int)(xmin2 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth2 - 100 - i);
                //    g1.DrawLine(p, 50, heigth2 - 100 - i, 65, heigth2 - 100 - i);
                //}
                //for (int i = 0; i <= (int)(ymax2 - ymin2) * 30; i = i + (int)((ymax2 - ymin2) * 30 / 10))
                //{
                //    g1.DrawString(((int)(ymin2 + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth2 - 40);
                //    g1.DrawLine(p, 100 + i, heigth2 - 50, 100 + i, heigth2 - 65);
                //}
                #endregion
                #endregion
                pictureBox3.Image = (Image)image2;
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            绘图ToolStripMenuItem_Click(sender,e);
        }
        #region 数据保存
        private void 数据保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "计算报告保存";
                saveFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                    this.richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            数据保存ToolStripMenuItem_Click(sender,e);
        }
        #endregion
        #region 图形保存
        #region bmp保存
        private void bmp图形ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "bmp图形保存";
                saveFileDialog1.Filter = "bmp图形文件(*.bmp)|*.bmp";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    image.Save(saveFileDialog1.FileName);
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void bmp纵断面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "bmp图形保存";
                saveFileDialog1.Filter = "bmp图形文件(*.bmp)|*.bmp";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    image1.Save(saveFileDialog1.FileName);
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        private void bmp横断面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "bmp图形保存";
                saveFileDialog1.Filter = "bmp图形文件(*.bmp)|*.bmp";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    image2.Save(saveFileDialog1.FileName);
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region 道路基本情况图
        private void dxf图形ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "dxf图形保存";
                saveFileDialog1.Filter = "dxf图形文件(*.dxf)|*.dxf";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.Write("0\nSECTION\n2\nTABLES\n0\nTABLE\n2\nLAYER\n0\nLAYER\n70\n0\n2\nshiti\n62\n10\n6\nCONTINUOUS\n");
                        sw.Write("0\nLAYER\n70\n0\n2\nzhuji\n62\n50\n6\nCONTINUOUS\n0\nLAYER\n70\n0\n2\nqita\n62\n90\n6\nCONTINUOUS\n0\nENDTAB\n0\nENDSEC\n");
                        sw.Write("0\nSECTION\n2\nENTITIES\n");
                        #region 绘制点
                        for (int i = 0; i < point1.Length; i++)
                        {
                            sw.Write("0\nPOINT\n8\nshiti\n");
                            sw.Write("10\n" + point1[i].Y + "\n");
                            sw.Write("20\n" + point1[i].X + "\n");
                            sw.Write("30\n" + point1[i].Z + "\n");
                        }
                        for (int i = 0; i < ZDM.Count; i++)
                        {
                            sw.Write("0\nPOINT\n8\nshiti\n");
                            sw.Write("10\n" + ZDM[i].Y + "\n");
                            sw.Write("20\n" + ZDM[i].X + "\n");
                            sw.Write("30\n" + ZDM[i].Z + "\n");
                        }
                        for (int i = 0; i < n; i++)
                        {
                            for (int j = 0; j < HDM[i].Count; j++)
                            {
                                sw.Write("0\nPOINT\n8\nshiti\n");
                                sw.Write("10\n" + HDM[i][j].Y + "\n");
                                sw.Write("20\n" + HDM[i][j].X + "\n");
                                sw.Write("30\n" + HDM[i][j].Z + "\n");
                            }
                        }
                        #endregion
                        #region 绘制注记
                        for (int i = 0; i < point1.Length; i++)
                        {
                            sw.Write("0\nTEXT\n8\nzhuji\n");
                            sw.Write("10\n" + (point1[i].Y - 3) + "\n");
                            sw.Write("20\n" + (point1[i].X - 3) + "\n");
                            sw.Write("40\n1\n1\n" + point1[i].dianhao + "\n");
                        }
                        for (int i = 0; i < ZDM.Count; i++)
                        {
                            sw.Write("0\nTEXT\n8\nzhuji\n");
                            sw.Write("10\n" + ZDM[i].Y + "\n");
                            sw.Write("20\n" + ZDM[i].X + "\n");
                            sw.Write("40\n1\n1\n" + ZDM[i].dianhao + "\n");
                        }
                        for (int i = 0; i < n; i++)
                        {
                            for (int j = 0; j < HDM[i].Count; j++)
                            {
                                sw.Write("0\nTEXT\n8\nzhuji\n");
                                sw.Write("10\n" + (HDM[i][j].Y - 3) + "\n");
                                sw.Write("20\n" + (HDM[i][j].X - 3) + "\n");
                                sw.Write("40\n1\n1\n" + HDM[i][j].dianhao + "\n");
                            }
                        }
                        #endregion
                        #region 绘制线
                        for (int i = 0; i < n; i++)
                        {
                            sw.Write("0\nLINE\n8\nqita\n");
                            sw.Write("10\n" + K[i].Y + "\n");
                            sw.Write("20\n" + K[i].X + "\n");
                            sw.Write("11\n" + K[i + 1].Y + "\n");
                            sw.Write("21\n" + K[i + 1].X + "\n"); 

                            sw.Write("0\nLINE\n8\nqita\n");
                            sw.Write("10\n" + HDM[i][0].Y + "\n");
                            sw.Write("20\n" + HDM[i][0].X + "\n");
                            sw.Write("11\n" + HDM[i][HDM[i].Count - 1].Y + "\n");
                            sw.Write("21\n" + HDM[i][HDM[i].Count - 1].X + "\n");
                        }
                        #endregion
                        sw.Write("0\nENDSEC\n0\nEOF\n");
                    }
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
        #region 纵断面图
        private void dxf纵断面图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "dxf图形保存";
                saveFileDialog1.Filter = "dxf图形文件(*.dxf)|*.dxf";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.Write("0\nSECTION\n2\nTABLES\n0\nTABLE\n2\nLAYER\n0\nLAYER\n70\n0\n2\nshiti\n62\n10\n6\nCONTINUOUS\n");
                        sw.Write("0\nLAYER\n70\n0\n2\nzhuji\n62\n50\n6\nCONTINUOUS\n0\nLAYER\n70\n0\n2\nqita\n62\n90\n6\nCONTINUOUS\n0\nENDTAB\n0\nENDSEC\n");
                        sw.Write("0\nSECTION\n2\nENTITIES\n");
                        for (int i = 0; i < ZDM.Count; i++)
                        {
                            sw.Write("0\nPOINT\n8\nshiti\n");
                            sw.Write("10\n" + ZDM[i].licheng + "\n");
                            sw.Write("20\n" + ZDM[i].Z + "\n");
                        }
                        for (int i = 0; i < ZDM.Count; i++)
                        {
                            sw.Write("0\nTEXT\n8\nzhuji\n");
                            sw.Write("10\n" + ZDM[i].licheng + "\n");
                            sw.Write("20\n" + ZDM[i].Z + "\n");
                            sw.Write("40\n1\n1\n" + ZDM[i].dianhao + "\n");
                        }
                        sw.Write("0\nPOLYLINE\n8\nshiti\n66\n1\n");
                        for (int i = 0; i < ZDM.Count; i++)
                        {
                            sw.Write("0\nVERTEX\n8\nshiti\n");
                            sw.Write("10\n" + ZDM[i].licheng + "\n");
                            sw.Write("20\n" + ZDM[i].Z + "\n");
                        }
                        sw.Write("0\nSEQEND\n");
                        sw.Write("0\nLINE\n8\nqita\n");
                        sw.Write("10\n" + 0 + "\n");
                        sw.Write("20\n" + gaocheng + "\n");
                        sw.Write("11\n" + licheng.Max() + "\n");
                        sw.Write("21\n" + gaocheng + "\n"); 
                        sw.Write("0\nENDSEC\n0\nEOF\n");
                    }
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
        #region 横断面图
        private void dxfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "dxf图形保存";
                saveFileDialog1.Filter = "dxf图形文件(*.dxf)|*.dxf";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.Write("0\nSECTION\n2\nTABLES\n0\nTABLE\n2\nLAYER\n0\nLAYER\n70\n0\n2\nshiti\n62\n10\n6\nCONTINUOUS\n");
                        sw.Write("0\nLAYER\n70\n0\n2\nzhuji\n62\n50\n6\nCONTINUOUS\n0\nLAYER\n70\n0\n2\nqita\n62\n90\n6\nCONTINUOUS\n0\nENDTAB\n0\nENDSEC\n");
                        sw.Write("0\nSECTION\n2\nENTITIES\n");
                        for (int j = 0; j < n; j++)
                        {
                            for (int i = 0; i < HDM[j].Count; i++)
                            {
                                sw.Write("0\nPOINT\n8\nshiti\n");
                                sw.Write("10\n" + (HDM[j][i].licheng + 100 * j) + "\n");
                                sw.Write("20\n" + HDM[j][i].Z + "\n");
                            }
                        }
                        for (int j = 0; j < n; j++)
                        {
                            for (int i = 0; i < HDM[j].Count; i++)
                            {
                                sw.Write("0\nTEXT\n8\nzhuji\n");
                                sw.Write("10\n" + (HDM[j][i].licheng + 100 * j) + "\n");
                                sw.Write("20\n" + HDM[j][i].Z + "\n");
                                sw.Write("40\n1\n1\n" + ZDM[i].dianhao + "\n");
                            }
                        }
                        for (int j = 0; j < n; j++)
                        {
                            sw.Write("0\nPOLYLINE\n8\nshiti\n66\n1\n");
                            for (int i = 0; i < HDM[j].Count; i++)
                            {
                                sw.Write("0\nVERTEX\n8\nshiti\n");
                                sw.Write("10\n" + (HDM[j][i].licheng + 100 * j) + "\n");
                                sw.Write("20\n" + HDM[j][i].Z + "\n");
                            }
                            sw.Write("0\nSEQEND\n");
                        }
                        for (int j = 0; j < n; j++)
                        {
                            sw.Write("0\nLINE\n8\nqita\n");
                            sw.Write("10\n" + (j * 100) + "\n");
                            sw.Write("20\n" + gaocheng + "\n");
                            sw.Write("11\n" + (50 + j * 100) + "\n");
                            sw.Write("21\n" + gaocheng + "\n");
                        }
                        sw.Write("0\nENDSEC\n0\nEOF\n");
                    }
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
    }
}