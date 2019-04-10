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

namespace _9.国赛_大地主题解算
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
            dataGridView1.AllowUserToAddRows = false;

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
        #endregion
        #region 刷新
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();
            this.InitializeComponent();
        }
        #endregion 
        #region 输入
        private void 正算输入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Columns[0].HeaderText = "起点";
            dataGridView1.Columns[1].HeaderText = "B1";
            dataGridView1.Columns[2].HeaderText = "L1";
            dataGridView1.Columns[3].HeaderText = "A1";
            dataGridView1.Columns[4].HeaderText = "S";
            dataGridView1.Columns[5].HeaderText = "终点";
            dataGridView1.Columns[6].HeaderText = "B2";
            dataGridView1.Columns[7].HeaderText = "L2";
            dataGridView1.Columns[8].HeaderText = "A2";

            dataGridView1.AllowUserToAddRows = true;
        }

        private void 反算输入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Columns[0].HeaderText = "起点";
            dataGridView1.Columns[1].HeaderText = "B1";
            dataGridView1.Columns[2].HeaderText = "L1";
            dataGridView1.Columns[3].HeaderText = "终点";
            dataGridView1.Columns[4].HeaderText = "B2";
            dataGridView1.Columns[5].HeaderText = "L2";
            dataGridView1.Columns[6].HeaderText = "A1";
            dataGridView1.Columns[7].HeaderText = "A2";
            dataGridView1.Columns[8].HeaderText = "S";

            dataGridView1.AllowUserToAddRows = true;
        }
        #endregion
        #region 变量定义
        double a, b, f, e1, e2;//基本椭球参数
        List<double> B1 = new List<double>();
        List<double> B2 = new List<double>();
        List<double> L1 = new List<double>();
        List<double> L2 = new List<double>();
        List<double> A12 = new List<double>();
        List<double> A21 = new List<double>();
        List<double> S = new List<double>();
        Bitmap image;
        int zf;//保存正算还是反算的信息
        #endregion
        #region 变量初始化
        public void initialize()//变量初始化
        {
            B1.Clear();//作用是每次执行计算时，就把变量清空，可以进行连续计算
            L1.Clear();
            B2.Clear();
            L2.Clear();
            A12.Clear();
            A21.Clear();
            S.Clear();
            try//椭球参数计算
            {
                a = Convert.ToDouble(txt_a.Text.Replace(" ", ""));
                f = 1 / Convert.ToDouble(txt_f.Text.Replace(" ", ""));
            }
            catch
            {
                MessageBox.Show("请输入正确的椭球数据！");
            }
            #region 椭球基本参数计算
            b = a * (1 - f);
            e1 = (a * a - b * b) / (a * a);
            e2 = (a * a - b * b) / (b * b);
            //MessageBox.Show(e1.ToString());
            #endregion
        }
        #endregion
        #region 文件打开
        private void 正算打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Rows.Clear();
                正算输入ToolStripMenuItem_Click(sender,e);

                openFileDialog1.Title = "正算打开";
                openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        string[] str = sr.ReadLine().Split(',');
                        txt_a.Text = str[0];
                        txt_f.Text = str[1];
                        int i = 0;
                        while (!sr.EndOfStream)//文本文件结束后不能有回车，否则不是文件末尾
                        {
                            dataGridView1.Rows.Add();
                            str = sr.ReadLine().Split(',');
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

        private void 反算打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Rows.Clear();
                反算输入ToolStripMenuItem_Click(sender, e);

                openFileDialog1.Title = "反算打开";
                openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        string[] str = sr.ReadLine().Split(',');
                        txt_a.Text = str[0];
                        txt_f.Text = str[1];
                        int i = 0;
                        while (!sr.EndOfStream)//文本文件结束后不能有回车，否则不是文件末尾
                        {
                            dataGridView1.Rows.Add();
                            str = sr.ReadLine().Split(',');
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
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            正算打开ToolStripMenuItem_Click(sender, e);
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            反算打开ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 正算
        private void 正算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zf = 1;
            initialize();
            dataGridView1.AllowUserToAddRows = false;

            #region 数据导入
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    B1.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value)));//(-90~90)
                    L1.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value)));//(0~360)
                    A12.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[3].Value)));//(0~360)
                    S.Add(Convert.ToDouble(dataGridView1.Rows[i].Cells[4].Value));
                }
            }
            catch 
            {
                MessageBox.Show("请输入正确的数据！");
                return;
            }
            #endregion
            #region 正算
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                #region 计算归化纬度辅助函数
                double sin_u1,cos_u1, sin_u2, sin_m, cos_m, cot_M, M, tan_l, l, gl;//球面元素变量
                double W1 = Math.Sqrt(1 - e1 * Math.Sin(B1[i]) * Math.Sin(B1[i]));
                sin_u1 = Math.Sin(B1[i]) * Math.Sqrt(1 - e1) / W1;
                cos_u1 = Math.Cos(B1[i]) / W1;
                sin_m = cos_u1 * Math.Sin(A12[i]);
                cos_m = Math.Sqrt(1 - sin_m * sin_m);
                cot_M = (cos_u1 * Math.Cos(A12[i])) / sin_u1;
                M = Math.Atan(1 / cot_M);
                #endregion
                #region 计算球面长度
                double k2, k4, k6, af, bf, cf, d0, d1, ds;//定义迭代变量，求边长归化量
                k2 = e2 * cos_m * cos_m;
                k4 = k2 * k2;
                k6 = k2 * k2 * k2;
                af = (1 - (k2) / 4 + (7 * k4) / 64 - (15 * k6) / 256) / b;//α
                bf = k2 / 4 - k4 / 8 + (37 * k6) / 512;//β
                cf = k4 / 128 - k6 / 128;//γ
                d0 = af * S[i];//迭代初值σ0
                do
                {
                    d1 = af * S[i] + bf * Math.Sin(d0) * Math.Cos(2 * M + d0) + cf * Math.Sin(2 * d0) * Math.Cos(4 * M + 2 * d0); //σ
                    ds = Math.Abs(d1 - d0);
                    d0 = d1;
                }
                while (ds * 206265 > 0.00001);//限差0.00001秒(不太清楚10 -10是啥意思)
                #endregion
                #region 计算经差改正数
                double kk2, kk4, e14, e16, aaf, bbf, ccf;//定义球面计算变量
                kk2 = e1 * cos_m * cos_m;
                kk4 = kk2 * kk2;
                e14 = e1 * e1;
                e16 = e1 * e1 * e1;
                aaf = (e1 / 2 + e14 / 8 + e16 / 16) - e1 * (1 + e1) * kk2 / 16 + 3 * e1 * kk4 / 128;
                bbf = e1 * (1 + e1) * kk2 / 16 - e1 * kk4 / 32;
                ccf = e1 * kk4 / 256;
                //经差改正数
                gl = sin_m * (aaf * d1 + bbf * Math.Sin(d1) * Math.Cos(2 * M + d1) + ccf * Math.Sin(2 * d1) * Math.Cos(4 * M + 2 * d1));
                #endregion
                #region 计算终点大地坐标及大地方位角
                sin_u2 = sin_u1 * Math.Cos(d1) + cos_u1 * Math.Cos(A12[i]) * Math.Sin(d1);
                B2.Add(Math.Atan(sin_u2 / (Math.Sqrt(1 - e1) * Math.Sqrt(1 - sin_u2 * sin_u2))));
                tan_l = Math.Sin(A12[i]) * Math.Sin(d1) / (cos_u1 * Math.Cos(d1) - sin_u1 * Math.Sin(d1) * Math.Cos(A12[i]));
                l = Math.Atan(tan_l);
                if (Math.Sin(A12[i]) > 0)
                {
                    if (tan_l > 0)
                    { l = Math.Abs(l); }
                    else
                    { l = Math.PI - Math.Abs(l); }
                }
                else
                {
                    if (tan_l < 0)
                    { l = - Math.Abs(l); }
                    else
                    { l = Math.Abs(l) - Math.PI; }
                }
                L2.Add(L1[i] + l - gl);
                double tan_a21,a21;
                tan_a21 = cos_u1 * Math.Sin(A12[i]) / (cos_u1 * Math.Cos(d1) * Math.Cos(A12[i]) - sin_u1 * Math.Sin(d1));
                a21 = Math.Atan(tan_a21);
                
                if (Math.Sin(A12[i]) < 0)
                {
                    if (tan_a21 > 0)
                    { a21 = Math.Abs(a21); }
                    else
                    { a21 = Math.PI - Math.Abs(a21); }
                }
                else
                {
                    if (tan_a21 > 0)
                    { a21 = Math.PI + Math.Abs(a21); }
                    else
                    { a21 = Math.PI * 2 - Math.Abs(a21); }
                }
                A21.Add(a21);
                #endregion 
                #region 数据导出
                dataGridView1.Rows[i].Cells[6].Value = Math.Round(Caculates.hudutodms(B2[i]),5);//0.1''
                dataGridView1.Rows[i].Cells[7].Value = Math.Round(Caculates.hudutodms(L2[i]),5);
                dataGridView1.Rows[i].Cells[8].Value = Math.Round(Caculates.hudutodms(A21[i]),5);
                #endregion
            }
            #region 计算报告
            richTextBox1.Text = "**************************************************\n\n";
            richTextBox1.Text += "                  大地主题正算\n\n";
            richTextBox1.Text += "**************************************************\n\n";
            richTextBox1.Text += "----------------------统计数据---------------------\n";
            richTextBox1.Text += "计算点对总数  ：" + dataGridView1.Rows.Count + "\n";
            richTextBox1.Text += "椭球长半轴：  "+ a + "\n";
            richTextBox1.Text += "椭球扁率：  "+ f + "\n\n";
            richTextBox1.Text += "----------------------计算结果---------------------\n";
            richTextBox1.Text += "点名     \t纬度(B)  \t经度(L)  \t大地方位角(A) \t大地线长(S)\n";
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[0].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[1].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[2].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[3].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[4].Value) + "\n";
                
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[5].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[6].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[7].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[8].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[4].Value) + "\n\n";
            }
            #endregion
            #endregion
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            正算ToolStripMenuItem_Click(sender,e);
        }
        #endregion
        #region 反算
        private void 反算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zf = -1;
            initialize();
            dataGridView1.AllowUserToAddRows = false;

            #region 数据导入
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    B1.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value)));//(-90~90)
                    L1.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value)));//(0~360)
                    B2.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[4].Value)));//(0~360)
                    L2.Add(Caculates.dmstohudu(Convert.ToDouble(dataGridView1.Rows[i].Cells[5].Value)));
                }
            }
            catch
            {
                MessageBox.Show("请输入正确的数据！");
                return;
            }
            #endregion
            #region 反算
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                double u1, u2, a1, a2, b1, b2;//辅助量
                double p, q, l0, l, dl, gl = 0, gl0;
                double a12, sin_d, cos_d, d, M, sin_m, cos_m;
                double kk2, kk4, e14, e16, aaf, bbf, ccf;//定义球面计算变量
                double k2, k4, k6, af, bf, cf;//定义迭代变量，求边长归化量
                #region 计算辅助量
                u1 = Math.Atan(Math.Sqrt(1 - e1) * Math.Tan(B1[i]));//归化纬度
                u2 = Math.Atan(Math.Sqrt(1 - e1) * Math.Tan(B2[i]));
                l = L2[i] - L1[i];//椭球面经差
                a1 = Math.Sin(u1) * Math.Sin(u2);
                a2 = Math.Cos(u1) * Math.Cos(u2);
                b1 = Math.Cos(u1) * Math.Sin(u2);
                b2 = Math.Sin(u1) * Math.Cos(u2);
                l0 = l;
                #endregion
                #region 计算起点大地方位角
                do
                {
                    gl0 = gl;
                    p = Math.Cos(u2) * Math.Sin(l0);//第一次计算时先以椭球面经差代替球面经差
                    q = b1 - b2 * Math.Cos(l0);
                    
                    a12 = Math.Atan(p / q);
                    if (p > 0)
                    {
                        if (q > 0)
                        { a12 = Math.Abs(a12); }
                        else
                        { a12 = Math.PI - Math.Abs(a12); }
                    }
                    else
                    {
                        if (q < 0)
                        { a12 = Math.PI + Math.Abs(a12); }
                        else
                        { a12 = Math.PI * 2 - Math.Abs(a12); }
                    }

                    sin_d = p * Math.Sin(a12) + q * Math.Cos(a12);
                    cos_d = a1 + a2 * Math.Cos(l0);
                    d = Math.Atan(sin_d / cos_d);
                    if (cos_d > 0)
                    { d = Math.Abs(d); }
                    else
                    { d = Math.PI - Math.Abs(d); }
                    sin_m = Math.Cos(u1) * Math.Sin(a12);
                    cos_m = Math.Sqrt(1 - sin_m * sin_m);
                    M = Math.Atan(Math.Tan(u1) / Math.Cos(a12));//
                    kk2 = e1 * cos_m * cos_m;
                    kk4 = kk2 * kk2;
                    e14 = e1 * e1;
                    e16 = e1 * e1 * e1;
                    aaf = (e1 / 2 + e14 / 8 + e16 / 16) - e1 * (1 + e1) * kk2 / 16 + 3 * e1 * kk4 / 128;
                    bbf = e1 * (1 + e1) * kk2 / 16 - e1 * kk4 / 32;
                    ccf = e1 * kk4 / 256;
                    gl = sin_m * (aaf * d + bbf * Math.Sin(d) * Math.Cos(2 * M + d) + ccf * Math.Sin(2 * d) * Math.Cos(4 * M + 2 * d));
                    l0 = l + gl;
                    dl = Math.Abs(gl - gl0);
                } while (dl * 206265 > 0.00001);
                //MessageBox.Show(Caculates.hudutodms(a12).ToString());
                A12.Add(a12);
                #endregion
                #region 计算大地线长度
                k2 = e2 * cos_m * cos_m;
                k4 = k2 * k2;
                k6 = k2 * k2 * k2;
                af = (1 - (k2) / 4 + (7 * k4) / 64 - (15 * k6) / 256) / b;//α
                bf = k2 / 4 - k4 / 8 + (37 * k6) / 512;//β
                cf = k4 / 128 - k6 / 128;//γ
                double s = (d - bf * Math.Sin(d) * Math.Cos(2 * M + d) - cf * Math.Sin(2 * d) * Math.Cos(4 * M + 2 * d)) / af;
                //MessageBox.Show(s.ToString());
                S.Add(s);
                #endregion
                #region 计算反方位角
                double a21;
                a21 = Math.Atan(Math.Cos(u1) * Math.Sin(l0) / (b1 * Math.Cos(l0) - b2));
                if (a21 < 0)
                { a21 = a21 + Math.PI * 2; }
                else if (a21 > Math.PI * 2)
                { a21 = a21 - Math.PI * 2; }
                if (a12 < Math.PI && a21 < Math.PI)
                { a21 = a21 + Math.PI; }
                if (a12 > Math.PI && a21 > Math.PI)
                { a21 = a21 - Math.PI; }
                //MessageBox.Show(Caculates.hudutodms(a21).ToString());
                A21.Add(a21);
                #endregion
                #region 数据导出
                dataGridView1.Rows[i].Cells[6].Value = Math.Round(Caculates.hudutodms(A12[i]), 5);//0.1''
                dataGridView1.Rows[i].Cells[7].Value = Math.Round(Caculates.hudutodms(A21[i]), 5);
                dataGridView1.Rows[i].Cells[8].Value = Math.Round(S[i], 3);
                #endregion
            }
            #endregion
            #region 计算报告
            richTextBox1.Text = "**************************************************\n\n";
            richTextBox1.Text += "                  大地主题反算\n\n";
            richTextBox1.Text += "**************************************************\n\n";
            richTextBox1.Text += "----------------------统计数据---------------------\n";
            richTextBox1.Text += "计算点对总数  ：" + dataGridView1.Rows.Count + "\n";
            richTextBox1.Text += "椭球长半轴：  " + a + "\n";
            richTextBox1.Text += "椭球扁率：  " + f + "\n\n";
            richTextBox1.Text += "----------------------计算结果---------------------\n";
            richTextBox1.Text += "点名     \t纬度(B)  \t经度(L)  \t大地方位角(A) \t大地线长(S)\n";
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[0].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[1].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[2].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[6].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[8].Value) + "\n";

                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[3].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[4].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[5].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[7].Value) + "\t";
                richTextBox1.Text += string.Format("{0, -8}", dataGridView1.Rows[i].Cells[8].Value) + "\n\n";
            }
            #endregion
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            反算ToolStripMenuItem_Click(sender,e);
        }
        #endregion
        #region 绘图
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                double xmax, ymax, xmin, ymin;
                if (B1.Max() > B2.Max())
                { xmax = Caculates.hudutodms(B1.Max()) * 100; }//坐标放大100倍
                else
                { xmax = Caculates.hudutodms(B2.Max()) * 100; }
                if (B1.Min() > B2.Min())
                { xmin = Caculates.hudutodms(B2.Min()) * 100; }
                else
                { xmin = Caculates.hudutodms(B1.Min()) * 100; }
                if (L1.Max() > L2.Max())
                { ymax = Caculates.hudutodms(L1.Max()) * 100; }
                else
                { ymax = Caculates.hudutodms(L2.Max()) * 100; }
                if (L1.Min() > L2.Min())
                { ymin = Caculates.hudutodms(L2.Min()) * 100; }
                else
                { ymin = Caculates.hudutodms(L1.Min()) * 100; }
                //MessageBox.Show(xmax.ToString());
                Pen p = new Pen(Color.Black, 10f);//定义画笔
                SolidBrush brush = new SolidBrush(Color.Red);//定义填充
                int width = (int)(ymax - ymin + 400);//图幅长
                int heigth = (int)(xmax - xmin + 400);//图幅宽
                image = new Bitmap(width, heigth);
                Graphics g = Graphics.FromImage(image);
                #region 绘制注记，点
                for (int i = 0; i < B1.Count; i++)
                {
                    //MessageBox.Show((Caculates.hudutodms(L1[i]) * 100 - ymin + 100).ToString());
                    g.FillEllipse(brush, (float)(Caculates.hudutodms(L1[i]) * 100 - ymin + 100) - 50f, -(float)(Caculates.hudutodms(B1[i]) * 100 - xmax - 100) - 50f, 100, 100);
                    g.FillEllipse(brush, (float)(Caculates.hudutodms(L2[i]) * 100 - ymin + 100) - 50f, -(float)(Caculates.hudutodms(B2[i]) * 100 - xmax - 100) - 50f, 100, 100);
                    g.DrawString(dataGridView1.Rows[i].Cells[0].Value.ToString(), new Font("宋体", 150), Brushes.Blue, (float)(Caculates.hudutodms(L1[i]) * 100 - ymin + 100) - 2.5f, -(float)(Caculates.hudutodms(B1[i]) * 100 - xmax - 100) - 2.5f);
                    if (zf == 1)
                    {
                        g.DrawString(dataGridView1.Rows[i].Cells[5].Value.ToString(), new Font("宋体", 150), Brushes.Blue, (float)(Caculates.hudutodms(L2[i]) * 100 - ymin + 100) - 2.5f, -(float)(Caculates.hudutodms(B2[i]) * 100 - xmax - 100) - 2.5f);
                    }
                    else
                    {
                        g.DrawString(dataGridView1.Rows[i].Cells[3].Value.ToString(), new Font("宋体", 150), Brushes.Blue, (float)(Caculates.hudutodms(L2[i]) * 100 - ymin + 100) - 2.5f, -(float)(Caculates.hudutodms(B2[i]) * 100 - xmax - 100) - 2.5f);
                    }
                    g.DrawLine(p, (float)(Caculates.hudutodms(L1[i]) * 100 - ymin + 100), -(float)(Caculates.hudutodms(B1[i]) * 100 - xmax - 100),(float)(Caculates.hudutodms(L2[i]) * 100 - ymin + 100), -(float)(Caculates.hudutodms(B2[i]) * 100 - xmax - 100));
                }
                #endregion
                MessageBox.Show("绘制成功！");
                pictureBox1.Image = (Image)image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            绘图ToolStripMenuItem_Click(sender,e);
        }
        #endregion
        #region 表格保存
        private void 表格保存ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "计算表格保存";
                saveFileDialog1.Filter = "文本文档(*.txt)|*.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        List<string> arrstr = new List<string>();
                        string str = null;
                        for (int i = 0; i < dataGridView1.ColumnCount; i++)
                        {
                            arrstr.Add(string.Format("{0,-8}", dataGridView1.Columns[i].HeaderText));
                        }
                        str = string.Join("\t", arrstr);
                        sw.WriteLine(str);
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            arrstr.Clear();
                            str = null;
                            for (int j = 0; j < dataGridView1.Columns.Count; j++)
                            {
                                arrstr.Add(string.Format("{0,-8}", dataGridView1.Rows[i].Cells[j].Value));
                            }
                            str = string.Join("\t", arrstr);
                            sw.WriteLine(str);
                        }
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
        #region 报告保存
        private void 报告保存ToolStripMenuItem1_Click(object sender, EventArgs e)
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
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            报告保存ToolStripMenuItem1_Click(sender,e);
        }
        #endregion
        #region 图形保存
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
                        for (int i = 0; i < B1.Count; i++)
                        {
                            sw.Write("0\nPOINT\n8\nshiti\n");
                            sw.Write("10\n" + (Caculates.hudutodms(L1[i]) * 100) + "\n");
                            sw.Write("20\n" + (Caculates.hudutodms(B1[i]) * 100) + "\n");
                            sw.Write("0\nPOINT\n8\nshiti\n");
                            sw.Write("10\n" + (Caculates.hudutodms(L2[i]) * 100) + "\n");
                            sw.Write("20\n" + (Caculates.hudutodms(B2[i]) * 100) + "\n");
                        }
                        for (int i = 0; i < B1.Count; i++)
                        {
                            sw.Write("0\nTEXT\n8\nzhuji\n");
                            sw.Write("10\n" + (Caculates.hudutodms(L1[i]) * 100 - 3) + "\n");
                            sw.Write("20\n" + (Caculates.hudutodms(B1[i]) * 100 - 3) + "\n");
                            sw.Write("40\n5\n1\n" + dataGridView1.Rows[i].Cells[0].Value + "\n");
                            sw.Write("0\nTEXT\n8\nzhuji\n");
                            sw.Write("10\n" + (Caculates.hudutodms(L2[i]) * 100 - 3) + "\n");
                            sw.Write("20\n" + (Caculates.hudutodms(B2[i]) * 100 - 3) + "\n");
                            if (zf == 1)
                            {
                                sw.Write("40\n5\n1\n" + dataGridView1.Rows[i].Cells[5].Value + "\n");//正算
                            }
                            else
                            {
                                sw.Write("40\n5\n1\n" + dataGridView1.Rows[i].Cells[3].Value + "\n");//反算
                            }
                        }
                        for (int i = 0; i < B1.Count; i++)
                        {
                            sw.Write("0\nLINE\n8\nqita\n");
                            sw.Write("10\n" + (Caculates.hudutodms(L1[i]) * 100) + "\n");
                            sw.Write("20\n" + (Caculates.hudutodms(B1[i]) * 100) + "\n");
                            sw.Write("11\n" + (Caculates.hudutodms(L2[i]) * 100) + "\n");
                            sw.Write("21\n" + (Caculates.hudutodms(B2[i]) * 100) + "\n");
                        }
                        sw.Write("0\nENDSEC\n0\nEOF");
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
