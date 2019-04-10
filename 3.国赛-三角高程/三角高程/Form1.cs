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

namespace 三角高程
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
            dataGridView3.AllowUserToAddRows = false;

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
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width < 5000)
            {
                pictureBox1.Width = Convert.ToInt32(pictureBox1.Width * 1.2);
                pictureBox1.Height = Convert.ToInt32(pictureBox1.Height * 1.2);
            }
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width > 100)
            {
                pictureBox1.Width = Convert.ToInt32(pictureBox1.Width / 1.2);
                pictureBox1.Height = Convert.ToInt32(pictureBox1.Height / 1.2);
            }
        }
        #endregion
        #region 刷新
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            this.Controls.Clear();//清空窗体中的所有控件
            this.InitializeComponent();//初始化窗体控件
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            刷新ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 定义变量
        double BHC,u;//存储闭合差，中误差
        static Bitmap image;
        List<string> dianhao;//存储测站点号
        List<double> juli;//存储距离信息
        List<double> julileiji;//绘图用到的x坐标
        List<double> gaochazhong;//存储高差信息
        List<double> gaizheng;//存储高差改正数
        List<double> gaocheng;//存储点高程信息
        List<double> m;//存储各点的高程中误差
        #endregion
        #region 初始化
        public void chushihua()
        {
            dianhao = new List<string>();
            juli = new List<double>();
            julileiji = new List<double>();
            gaochazhong = new List<double>();
            gaizheng = new List<double>();
            gaocheng = new List<double>();
            m = new List<double>();
            dataGridView2.AllowUserToAddRows = false;
        }
        #endregion
        #region 文件打开
        private void 文件打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            richTextBox1.Text = "";
            pictureBox1.Image = null;

            try
            {
                openFileDialog1.Title = "打开文件";
                openFileDialog1.Filter = "文本文件(*.txt)|*.txt|Excel旧版本文件(*.xls)|*.xls|Excel新版本文件(*.xlsx)|xlsx";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        string[] iteam = sr.ReadLine().Split(',');//用字符数组存储数据，数据可以为空
                        textBox1.Text = iteam[1];
                        sr.ReadLine().Split(',');//跳过一行
                        int i = 0;//定义循环变量
                        while (!sr.EndOfStream)
                        {
                            dataGridView2.Rows.Add();
                            dataGridView2.Rows.Add();
                            iteam = sr.ReadLine().Split(',');
                            dataGridView2.Rows[i].Cells[0].Value = iteam[0];//写测段
                            #region 识别往返测
                            string[] iteam1 = iteam[0].Split('-');//将测段分开,用来识别往返测
                            iteam = sr.ReadLine().Split(',');
                            if (iteam[0] == iteam1[0])
                            {
                                dataGridView2.Rows[i].Cells[1].Value = "往测";
                                dataGridView2.Rows[i + 1].Cells[1].Value = "返测";
                            }
                            else
                            {
                                dataGridView2.Rows[i].Cells[1].Value = "返测";
                                dataGridView2.Rows[i + 1].Cells[1].Value = "往测";
                            }
                            #endregion
                            for (int j = 2; j < iteam.Count(); j++)
                            {
                                dataGridView2.Rows[i].Cells[j].Value = iteam[j];
                            }
                            iteam = sr.ReadLine().Split(',');
                            for (int j = 2; j < iteam.Count(); j++)
                            {
                                dataGridView2.Rows[i + 1].Cells[j].Value = iteam[j];
                            }
                            i += 2;
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
            文件打开ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 计算
        private void 预处理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                chushihua();
                int j = 0;
                julileiji.Add(0);//绘图时用到，起点的坐标设为0
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    double S;//斜距
                    double A;//竖直角
                    double ii;//仪器高
                    double vv;//目标高
                    double D;//平距
                    double qiuqicha;//保证精度储存球气差与平距
                    S = Convert.ToDouble(dataGridView2.Rows[i].Cells[2].Value);
                    A = Class1.dmstohudu(Convert.ToDouble(dataGridView2.Rows[i].Cells[3].Value));
                    ii = Convert.ToDouble(dataGridView2.Rows[i].Cells[4].Value);
                    vv = Convert.ToDouble(dataGridView2.Rows[i].Cells[5].Value);
                    D = S * Math.Cos(A);
                    qiuqicha = Class1.qiuqi(D);
                    dataGridView2.Rows[i].Cells[7].Value = Math.Round(D, 4);
                    dataGridView2.Rows[i].Cells[6].Value = Math.Round(qiuqicha, 4);
                    dataGridView2.Rows[i].Cells[8].Value = Math.Round(D * Math.Sin(A) + ii - vv + qiuqicha, 4);
                    if (i % 2 == 1)//每计算两次求一次平均
                    {
                        string[] iteam2 = Convert.ToString(dataGridView2.Rows[i - 1].Cells[0].Value).Split('-');//为点号赋值
                        dianhao.Add(iteam2[0]);
                        if (i == dataGridView2.Rows.Count - 1)
                        {
                            dianhao.Add(iteam2[1]);
                        }
                        dataGridView2.Rows[i - 1].Cells[9].Value = Math.Round((Convert.ToDouble(dataGridView2.Rows[i - 1].Cells[8].Value) - Convert.ToDouble(dataGridView2.Rows[i].Cells[8].Value)) / 2, 4); ;//计算高差
                        gaochazhong.Add(Convert.ToDouble(dataGridView2.Rows[i - 1].Cells[9].Value));
                        juli.Add((Convert.ToDouble(dataGridView2.Rows[i - 1].Cells[7].Value) + Convert.ToDouble(dataGridView2.Rows[i].Cells[7].Value)) / 2);//计算平距
                        julileiji.Add(julileiji[j] + juli[j]);

                        if ( Math.Abs(Convert.ToDouble(dataGridView2.Rows[i - 1].Cells[8].Value) + Convert.ToDouble(dataGridView2.Rows[i].Cells[8].Value)) < 60 * Math.Sqrt(juli.Sum() / 1000))
                        { dataGridView2.Rows[i - 1].Cells[10].Value = "T"; }
                        else
                        {dataGridView2.Rows[i - 1].Cells[10].Value = "F";}
                        j++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void 平差计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                double C = 1000;
                List<double> P1 = new List<double>();
                List<double> Pvv = new List<double>();

                BHC = gaochazhong.Sum();//闭合水准路线闭合差
                gaocheng.Add(Convert.ToDouble(textBox1.Text));//高程起始点赋值
                for (int i = 0; i < gaochazhong.Count; i++)//平差计算与数值输出
                {
                    gaizheng.Add( - BHC * juli[i] / juli.Sum());
                    gaocheng.Add(gaocheng[i] + gaochazhong[i] + gaizheng[i]);
                    P1.Add(C / juli[i]);
                    Pvv.Add(P1[i] * gaizheng[i] * gaizheng[i]);//中误差计算准备
                    dataGridView3.Rows.Add();
                    dataGridView3.Rows[i].Cells[0].Value = dianhao[i];
                    dataGridView3.Rows[i].Cells[1].Value = Math.Round(juli[i], 4);
                    dataGridView3.Rows[i].Cells[2].Value = Math.Round(gaochazhong[i], 4);
                    dataGridView3.Rows[i].Cells[3].Value = Math.Round(gaizheng[i] * 1000, 1);
                    dataGridView3.Rows[i].Cells[4].Value = Math.Round(gaochazhong[i] + gaizheng[i], 4);
                    dataGridView3.Rows[i].Cells[5].Value = Math.Round(gaocheng[i], 4);
                }
                dataGridView3.Rows.Add();
                dataGridView3.Rows[gaochazhong.Count].Cells[0].Value = dianhao[gaochazhong.Count];
                dataGridView3.Rows[gaochazhong.Count].Cells[5].Value = Math.Round(gaocheng[gaochazhong.Count], 4);
                u = Math.Sqrt(Pvv.Sum() / (gaochazhong.Count - dianhao.Count + 2));//计算单位权中误差

                List<double> P2 = new List<double>();
                for (int i = 0; i < gaochazhong.Count - 1; i++)//精度评定
                {
                    double f1, f2;
                    f1 = C / julileiji[i + 1];
                    f2 = C / (juli.Sum() - julileiji[i + 1]);
                    P2.Add(f1 + f2);
                    m.Add(u / Math.Sqrt(Math.Abs(P2[i])));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void 生成报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox1.Text = "                                          三角高程平差\n";
                richTextBox1.Text += "-------------------------------------------统计数据---------------------------------------------------------\n";
                richTextBox1.Text += "总测段数\t" + gaochazhong.Count + "\n";
                richTextBox1.Text += "路线长度\t" + juli.Sum() + "m\n";
                richTextBox1.Text += "闭合差\t" + Math.Round(BHC * 1000, 1) + "mm\n";
                richTextBox1.Text += "------------------------------------------已知点数据--------------------------------------------------------\n";
                richTextBox1.Text += dianhao[0] + "  " + textBox1.Text + "m\n";
                richTextBox1.Text += "------------------------------------------预处理数据--------------------------------------------------------\n";
                Class1.daochu1(richTextBox1, dataGridView2);
                richTextBox1.Text += "-----------------------------------------高程配赋数据-------------------------------------------------------\n";
                Class1.daochu1(richTextBox1, dataGridView3);
                richTextBox1.Text += "-------------------------------------------精度数据---------------------------------------------------------\n";
                richTextBox1.Text += "单位权中误差\t" + Math.Round(u,4) + "\n\n";
                for (int i = 0; i < gaochazhong.Count - 1; i++)
                {
                    richTextBox1.Text += "待测点中误差： " + dianhao[i + 1] + "\t" + Math.Round(m[i], 4) + "\n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void 一键生成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            预处理ToolStripMenuItem_Click(sender, e);
            平差计算ToolStripMenuItem_Click(sender, e);
            生成报告ToolStripMenuItem_Click(sender, e);
            绘图ToolStripMenuItem_Click(sender, e);
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            预处理ToolStripMenuItem_Click(sender, e);
            平差计算ToolStripMenuItem_Click(sender, e);
            生成报告ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 保存
        private void 文件保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "三角高程计算保存";
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
            文件保存ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 绘图
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pen p = new Pen(Color.Black, 2.5f);//定义画笔
                Pen p2 = new Pen(Color.Blue, 2);
                SolidBrush brush = new SolidBrush(Color.Black);//定义填充
                int width = (int)(julileiji.Max() - julileiji.Min() + 200);//图幅长
                int heigth = (int)(gaocheng.Max() * 1000 - gaocheng.Min() * 1000 + 200);//图幅宽
                image = new Bitmap(width, heigth);
                Graphics g = Graphics.FromImage(image);
                PointF[] pf = new PointF[gaocheng.Count];
                #region 绘制线型
                for (int i = 0; i < gaocheng.Count; i++)
                {
                    pf[i].X = (float)(julileiji[i] - julileiji.Min() + 100);
                    pf[i].Y = -(float)(gaocheng[i] * 1000 - gaocheng.Max() * 1000 - 100);//高程乘以1000
                }
                g.DrawLines(p, pf);
                #endregion
                #region 绘制注记，点
                for (int i = 0; i < gaocheng.Count; i++)
                {
                    g.FillEllipse(brush, pf[i].X - 2.5f, pf[i].Y - 2.5f, 5, 5);
                    g.DrawString(dianhao[i], new Font("宋体", 30), Brushes.Red, pf[i].X - 2.5f, pf[i].Y - 2.5f);
                }
                #endregion
                #region 绘制坐标轴
                PointF[] xpt = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
                PointF[] ypt = new PointF[3] { new PointF(width - 35, heigth - 50), new PointF(width - 50, heigth - 60), new PointF(width - 50, heigth - 40) };
                g.DrawLine(p, 50, heigth - 50, 50, 50);//画x轴
                g.DrawLine(p, 50, heigth - 50, width - 50, heigth - 50);//画y轴
                g.FillPolygon(brush, xpt);//x轴箭头
                g.FillPolygon(brush, ypt);//y轴箭头
                g.DrawString("X/高程", new Font("宋体", 10), Brushes.Black, 0, 40);
                g.DrawString("Y/距离", new Font("宋体", 10), Brushes.Black, width - 60, heigth - 20);//注记文字是在点位的右上角绘制
                for (float i = 0; i <= (float)(gaocheng.Max() * 1000 - gaocheng.Min() * 1000); i = i + (float)((gaocheng.Max() * 1000 - gaocheng.Min() * 1000) / 5))//X
                {
                    g.DrawString(((float)(Math.Round((gaocheng.Min() + i / 1000),4))).ToString(), new Font("宋体", 10), Brushes.Black, 0, heigth - 100 - i);
                    g.DrawLine(p, 50, heigth - 100 - i, 65, heigth - 100 - i);
                }
                for (int i = 0; i <= (int)(julileiji.Max() - julileiji.Min()); i = i + (int)((julileiji.Max() - julileiji.Min()) / 5))//Y
                {
                    g.DrawString(((int)(julileiji.Min() + i)).ToString(), new Font("宋体", 10), Brushes.Black, 100 + i, heigth - 40);
                    g.DrawLine(p, 100 + i, heigth - 50, 100 + i, heigth - 65);
                }
                #endregion
                pictureBox1.Image = (Image)image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            绘图ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 图形保存
        private void 概图保存ToolStripMenuItem_Click(object sender, EventArgs e)
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
                            sw.Write("0\nSECTION\n2\nTABLES\n0\nTABLE\n2\nLAYER\n0\nLAYER\n70\n0\n2\nshiti\n62\n10\n6\nCONTINUOUS\n");
                            sw.Write("0\nLAYER\n70\n0\n2\nzhuji\n62\n50\n6\nCONTINUOUS\n0\nLAYER\n70\n0\n2\nqita\n62\n90\n6\nCONTINUOUS\n0\nENDTAB\n0\nENDSEC\n");
                            sw.Write("0\nSECTION\n2\nENTITIES\n");
                            #region 绘制线路
                            sw.Write("0\nPOLYLINE\n");//多线段绘制，为一个整体线段
                            sw.Write("8\n");//图层
                            sw.Write("shiti\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("66\n");//不太懂，应该是多线个数
                            sw.Write("1\n");
                            for (int i = 0; i < julileiji.Count; i++)
                            {
                                sw.Write("0\nVERTEX\n");//多线段标识
                                sw.Write("8\n");//图层
                                sw.Write("shiti\n");
                                sw.Write("10\n");//X坐标
                                sw.Write(julileiji[i] + "\n");
                                sw.Write("20\n");//Y坐标
                                sw.Write(gaocheng[i] + "\n");
                            }
                            sw.Write("0\nSEQEND\n");//多线段结束
                            #endregion
                            #region 文字注记
                            for (int i = 0; i < julileiji.Count; i++)
                            {
                                sw.Write("0\nTEXT\n");//单行文字
                                sw.Write("8\n");
                                sw.Write("zhuji\n");
                                sw.Write("10\n");//字体起点X
                                sw.Write(julileiji[i] - 5 + "\n");
                                sw.Write("20\n");//字体起点Y
                                sw.Write(gaocheng[i] - 5 + "\n");
                                sw.Write("40\n15\n");//字体高度
                                sw.Write("1\n" + dianhao[i] + "\n");//文字内容
                            }
                            #endregion
                            sw.Write("0\nENDSEC\n0\nEOF\n");
                            MessageBox.Show("保存成功");
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            概图保存ToolStripMenuItem_Click(sender, e);
        }
        #endregion
    }
}
