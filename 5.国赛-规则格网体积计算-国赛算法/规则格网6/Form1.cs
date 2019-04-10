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

namespace 规则格网6
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
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();
            this.InitializeComponent();
        }
        #endregion
        #region 变量声明
        Point2[,] gedian;//存储格点数据
        List<Pointl> M;//存储散点数据
        List<Pointl> S;//存储凸包点的数据
        double Xmax, Xmin, Ymax, Ymin;//绘制格网用到
        double H0, bian, linyu;//存储基准高程，格网边长
        int a, b;//存储格网个数
        double R0,  V, V1, V2;//存储总体积，填方，挖方
        static Bitmap image1;
        #endregion
        #region 变量初始化
        public void qingkong2()
        {
            M = new List<Pointl>();
            S = new List<Pointl>();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
        }
        #endregion
        #region 文件打开
        private void 文件打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Rows.Clear();
                openFileDialog1.Title = "散点数据文件打开";
                openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using(StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        for (int i = 0; !sr.EndOfStream; i++)
                        {
                            string[] iteam = sr.ReadLine().Split(',');
                            dataGridView1.Rows.Add();
                            dataGridView1.Rows[i].Cells[0].Value = iteam[0];
                            dataGridView1.Rows[i].Cells[1].Value = iteam[1];
                            dataGridView1.Rows[i].Cells[2].Value = iteam[2];
                            dataGridView1.Rows[i].Cells[3].Value = iteam[3];
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
            文件打开ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 计算
        private void 计算ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                qingkong2();
                #region 数据输入+格式判断
                string hang = "";
                try
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        hang = i + "行数据错误";
                        Pointl ff = new Pointl();
                        ff.dianhao = dataGridView1.Rows[i].Cells[0].Value.ToString().Replace(" ", "");
                        ff.X = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value.ToString().Replace(" ", ""));
                        ff.Y = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value.ToString().Replace(" ", ""));
                        ff.H = Convert.ToDouble(dataGridView1.Rows[i].Cells[3].Value.ToString().Replace(" ", ""));
                        M.Add(ff);
                    }
                }
                catch
                {
                    MessageBox.Show(hang);
                    return;
                }
                try
                {
                    hang = "基本信息错误";
                    bian = Convert.ToDouble(txt_bian.Text);
                    H0 = Convert.ToDouble(txt_gaocheng.Text);
                    linyu = Convert.ToDouble(txt_linyu.Text);
                }
                catch
                {
                    MessageBox.Show(hang);
                    return;
                }
                #endregion
                #region 1.凸包生成
                #region 1.1查找基点
                double min = M[0].Y;
                int hao = 0;//存储最小值的点号信息
                for (int i = 0; i < M.Count; i++)
                {
                    if (M[i].Y < min)
                    {
                        min = M[i].Y;
                        hao = i;
                    }
                    else if ((M[i].Y == min) && (M[i].X < M[hao].X))
                    {
                        min = M[i].Y;
                        hao = i;
                    }
                }
                #endregion
                #region 1.2计算夹角并排序
                Pointl f = new Pointl();//基点与一号点互换位置,方便下一步循环
                f = M[0];
                M[0] = M[hao];
                M[hao] = f;
                for (int i = 1; i < M.Count; i++)//计算夹角
                {
                    double R = 0, x1, y1;
                    x1 = M[i].X - M[0].X;
                    y1 = M[i].Y - M[0].Y;
                    if (x1 == 0)
                    { M[i].jiao = Math.PI / 2; }
                    else
                    {
                        R = Math.Atan(Math.Abs(y1) / Math.Abs(x1));
                        if (x1 < 0)
                        { R = Math.PI - R; }
                        M[i].jiao = R;
                    }
                }
                //还需要加一个对于夹角相同时，剔除最近点的过程，但是不加也没关系，就是后面循环会多点，无所谓
                for (int i = 1; i < M.Count; i++)//夹角冒泡排序
                {
                    for (int j = i + 1; j < M.Count; j++)
                    {
                        if (M[i].jiao > M[j].jiao)
                        {
                            f = M[i];
                            M[i] = M[j];
                            M[j] = f;
                        }
                    }
                }
                for (int i = 0; i < M.Count; i++)//排序
                {
                    dataGridView1.Rows[i].Cells[0].Value = M[i].jiao;
                    dataGridView1.Rows[i].Cells[1].Value = M[i].dianhao;
                }
                #endregion
                #region 1.3建立凸包点构成的列表
                S.Add(M[0]);
                S.Add(M[1]);
                S.Add(M[2]);
                int top = 2;
                for (int i = 3; i < M.Count; i++)//建立凸包点列表
                {
                    while ((S[top - 1].X - S[top].X) * (M[i].Y - S[top].Y) - (S[top - 1].Y - S[top].Y) * (M[i].X - S[top].X) > 0)//左转
                    { top--; }
                    if (top < S.Count - 1)
                    { S[top + 1] = M[i]; }
                    else
                    { S.Add(M[i]); }
                    top++;
                }
                S.Add(S[0]);
                #endregion
                #endregion
                #region 2.格网生成
                #region 2.1建立最小外包矩形
                Xmax = S[0].X;//建立外包矩形
                Xmin = S[0].X;
                Ymax = S[0].Y;
                Ymin = S[0].Y;
                for (int i = 0; i < S.Count; i++)
                {
                    Xmax = Xmax < S[i].X ? S[i].X : Xmax;
                    Xmin = Xmin > S[i].X ? S[i].X : Xmin;
                    Ymax = Ymax < S[i].Y ? S[i].Y : Ymax;
                    Ymin = Ymin > S[i].Y ? S[i].Y : Ymin;
                }
                a = (int)((Ymax - Ymin) / bian) + 1;//存储格网点个数，比格网个数多一个
                b = (int)((Xmax - Xmin) / bian) + 1;
                #endregion
                #region 2.2判断网格中心是否在凸包内
                gedian = new Point2[a, b];//初始化格点
                for (int i = 0; i < a; i++)//就算格网按格网点算，多一个，也不会对后面计算带来影响，因为不在凸包内
                    for (int j = 0; j < b; j++)//对每个凸包点都进行遍历
                    {
                        Point2 f1 = new Point2();
                        int nw = 0;
                        f1.dianhao = i + "," + j;
                        //MessageBox.Show(bian.ToString());
                        f1.X = ((j + j + 1) / 2d) * bian + Xmin;//计算格网中心点坐标
                        f1.Y = ((i + i + 1) / 2d) * bian + Ymin;
                        for (int c = 0; c < S.Count - 1; c++)//按顺序遍历S中的凸包点集
                        {
                            double x1 = 0;
                            if ((f1.Y >= S[c].Y && f1.Y <= S[c + 1].Y) || (f1.Y <= S[c].Y && f1.Y >= S[c + 1].Y))//在两个y分量之间
                            {
                                x1 = (S[c + 1].X - S[c].X) * (f1.Y - S[c].Y) / (S[c + 1].Y - S[c].Y) + S[c].X;
                                if (x1 > f1.X)
                                { nw++; }
                            }
                        }
                        f1.NW = nw;
                        gedian[i, j] = f1;
                    }
                #endregion
                #endregion
                #region 3.体积计算
                R0 = linyu * (a + b) / 2;//半径
                int m = 0;//用来生成表格
                V = 0; V1 = 0; V2 = 0;
                for (int i = 0; i < a; i++)//计算体积
                {
                    for (int j = 0; j < b; j++)//对每个格网点进行遍历
                    {
                        if (gedian[i, j].NW % 2 == 1)//在凸包内部
                        {
                            double h1, h2, h3, h4;
                            h1 = Class1.gaocheng(gedian[i, j].X - 0.5 * bian, gedian[i, j].Y - 0.5 * bian, R0, M);//传入格网点坐标和半径以及散点数据，内插高程
                            h2 = Class1.gaocheng(gedian[i, j].X + 0.5 * bian, gedian[i, j].Y - 0.5 * bian, R0, M);
                            h3 = Class1.gaocheng(gedian[i, j].X - 0.5 * bian, gedian[i, j].Y + 0.5 * bian, R0, M);
                            h4 = Class1.gaocheng(gedian[i, j].X + 0.5 * bian, gedian[i, j].Y + 0.5 * bian, R0, M);
                            gedian[i, j].H = ((h1 + h2 + h3 + h4) / 4);
                            gedian[i, j].TU = ((h1 + h2 + h3 + h4) / 4 - H0) * bian * bian;
                            V += gedian[i, j].TU;
                            if (gedian[i, j].TU > 0)
                            { V1 += gedian[i, j].TU; }
                            else
                            { V2 += gedian[i, j].TU; }
                            #region 输出格网店信息
                            dataGridView3.Rows.Add();
                            dataGridView3.Rows[m].Cells[0].Value = gedian[i, j].dianhao;
                            dataGridView3.Rows[m].Cells[1].Value = gedian[i, j].X;
                            dataGridView3.Rows[m].Cells[2].Value = gedian[i, j].Y;
                            dataGridView3.Rows[m].Cells[3].Value = gedian[i, j].H;
                            dataGridView3.Rows[m].Cells[4].Value = gedian[i, j].TU;
                            m++;

                            #endregion
                        }
                    }
                }
                #region 输出凸包点信息
                for (int i = 0; i < S.Count; i++)
                {
                    dataGridView2.Rows.Add();
                    dataGridView2.Rows[i].Cells[0].Value = S[i].dianhao;
                    dataGridView2.Rows[i].Cells[1].Value = S[i].X;
                    dataGridView2.Rows[i].Cells[2].Value = S[i].Y;
                    dataGridView2.Rows[i].Cells[3].Value = S[i].H;
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 生成报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox1.Text = "规则格网体积计算\n";
                richTextBox1.Text += "\t基本信息\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                richTextBox1.Text += "基准高程:" + txt_gaocheng.Text + "\n单位网格边长:" + txt_bian.Text + "\n网格纵向个数:" + b + "\n网格横向个数:" + a + "\n单位网格总数:" + (a * b) + "\n总体积:" + Math.Round(V, 3) + "\n填方体积:" + Math.Round(V2, 3) + "\n挖方体积:" + Math.Round(V1, 3) + "\n";
                richTextBox1.Text += "\n\t凸包点信息\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                Class1.daochutoR(richTextBox1, dataGridView2);
                richTextBox1.Text += "\n\t网格处理数据\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                Class1.daochutoR(richTextBox1, dataGridView3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            计算ToolStripMenuItem1_Click(sender, e);
            生成报告ToolStripMenuItem_Click(sender, e);
        }

        private void 一键处理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            计算ToolStripMenuItem1_Click(sender, e);
            生成报告ToolStripMenuItem_Click(sender, e);
            绘图ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 文件保存
        private void 文件导出ToolStripMenuItem_Click(object sender, EventArgs e)
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
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            文件导出ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 绘图
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pen p = new Pen(Color.Black, 2.5f);
                Pen p2 = new Pen(Color.Blue, 2);
                SolidBrush brush = new SolidBrush(Color.Black);//定义填充
                int width = (int)((Ymax - Ymin) * 30 + 200);//图幅长
                int heigth = (int)((Xmax - Xmin) * 30 + 200);//图幅宽
                image1 = new Bitmap(width, heigth);//显示图形范围
                Graphics g = Graphics.FromImage(image1);
                PointF[] pf1 = new PointF[M.Count];
                for (int i = 0; i < S.Count; i++)
                {
                    pf1[i].X = (float)((S[i].Y - Ymin) * 30 + 100);
                    pf1[i].Y = -(float)((S[i].X - Xmax) * 30 - 100);
                }
                g.FillPolygon(new SolidBrush(Color.Yellow), pf1);
                #region 网格绘制
                for (int i = 0; i < b; i++)
                {
                    for (int j = 0; j < a; j++)
                    {
                        g.DrawLine(p, (int)((j * bian) * 30 + 100), -(int)((i * bian + Xmin - Xmax) * 30 - 100), (int)(((j) * bian) * 30 + 100), -(int)(((i + 1) * bian + Xmin - Xmax) * 30 - 100));
                        g.DrawLine(p, (int)(((j) * bian) * 30 + 100), -(int)(((i + 1) * bian + Xmin - Xmax) * 30 - 100), (int)(((j + 1) * bian) * 30 + 100), -(int)(((i + 1) * bian + Xmin - Xmax) * 30 - 100));
                        g.DrawLine(p, (int)(((j + 1) * bian) * 30 + 100), -(int)(((i + 1) * bian + Xmin - Xmax) * 30 - 100), (int)(((j + 1) * bian) * 30 + 100), -(int)(((i) * bian + Xmin - Xmax) * 30 - 100));
                        g.DrawLine(p, (int)(((j + 1) * bian) * 30 + 100), -(int)(((i) * bian + Xmin - Xmax) * 30 - 100), (int)(((j) * bian) * 30 + 100), -(int)(((i) * bian + Xmin - Xmax) * 30 - 100));
                    }
                }
                #endregion
                #region 点与高程注记
                PointF[] pf = new PointF[M.Count];
                for (int i = 0; i < M.Count; i++)
                {
                    g.DrawEllipse(Pens.Black, (int)((M[i].Y - Ymin + 100) * 30 - 6), -(int)((M[i].X - Xmax - 100) * 30 - 6), 12, 12);
                    g.FillEllipse(new SolidBrush(Color.Black), (int)((M[i].Y - Ymin + 100) * 30 - 6), -(int)((M[i].X - Xmax - 100) * 30 - 6), 12, 12);
                    //g1.DrawString()
                }
                #endregion
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
                pictureBox1.Image = (Image)image1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            绘图ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 图形保存
        private void 图形导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
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
                            #region 网格绘制
                            for (int i = 0; i < b; i++)
                            {
                                for (int j = 0; j < a; j++)
                                {
                                    sw.Write("0\nPOLYLINE\n");//多线段绘制，为一个整体线段
                                    sw.Write("8\n");//图层
                                    sw.Write("0\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                                    sw.Write("66\n");
                                    sw.Write("1\n");

                                    Class1.duoxian(sw, (j * bian + Ymin) * bian, (i * bian + Xmin) * bian);
                                    Class1.duoxian(sw, (j * bian + Ymin) * bian, ((i + 1) * bian + Xmin) * bian);
                                    Class1.duoxian(sw, ((j + 1) * bian + Ymin) * bian, ((i + 1) * bian + Xmin) * bian);
                                    Class1.duoxian(sw, ((j + 1) * bian + Ymin) * bian, (i * bian + Xmin) * bian);
                                    Class1.duoxian(sw, (j * bian + Ymin) * bian, (i * bian + Xmin) * bian);

                                    sw.Write("0\nSEQEND\n");//多线段结束
                                }
                            }
                            #endregion
                            #region 绘凸包
                            sw.Write("0\nPOLYLINE\n");//多线段绘制，为一个整体线段
                            sw.Write("8\n");//图层
                            sw.Write("shiti\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("66\n");
                            sw.Write("1\n");
                            for (int i = 0; i < S.Count; i++)
                            {
                                sw.Write("0\nVERTEX\n");//多线段标识
                                sw.Write("8\n");//图层
                                sw.Write("shiti\n");
                                sw.Write("10\n");//X坐标
                                sw.Write(S[i].Y * bian + "\n");
                                sw.Write("20\n");//Y坐标
                                sw.Write(S[i].X * bian + "\n");
                            }
                            sw.Write("0\nSEQEND\n");//多线段结束
                            #endregion
                            #region 绘点
                            for (int i = 0; i < M.Count; i++)
                            {
                                sw.Write("0\nPOINT\n");//单一点绘制
                                sw.Write("8\n");//图层
                                sw.Write("zhuji\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                                sw.Write("10\n");//X坐标
                                sw.Write(M[i].Y * bian + "\n");
                                sw.Write("20\n");//Y坐标
                                sw.Write(M[i].X * bian + "\n");
                            }
                            #endregion
                            #region 注记
                            for (int i = 0; i < M.Count; i++)
                            {
                                sw.Write("0\nTEXT\n");//单行文字
                                sw.Write("8\n");
                                sw.Write("zhuji\n");
                                sw.Write("10\n");//字体起点X
                                sw.Write(M[i].Y * bian - bian / 5 + "\n");
                                sw.Write("20\n");//字体起点Y
                                sw.Write(M[i].X * bian - bian / 5 + "\n");
                                sw.Write("40\n" + bian / 8 + "\n");//字体高度
                                sw.Write("1\n" + Math.Round(M[i].H, 3) + "\n");//文字内容
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
                        image1.Save(saveFileDialog1.FileName);
                        MessageBox.Show("保存成功！");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}
