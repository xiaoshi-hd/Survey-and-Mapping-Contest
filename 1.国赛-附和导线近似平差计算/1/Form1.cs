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

namespace _1
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
            if (pictureBox1.Width < 5000)
            {
                pictureBox1.Width = Convert.ToInt32(pictureBox1.Width * 1.2);
                pictureBox1.Height = Convert.ToInt32(pictureBox1.Height * 1.2);
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Width > 100)
            {
                pictureBox1.Width = Convert.ToInt32(pictureBox1.Width / 1.2);
                pictureBox1.Height = Convert.ToInt32(pictureBox1.Height / 1.2);
            }
        }
        #endregion
        #region 其他功能
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("国赛程序1-附和导线近似平差计算！！！");
        }
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();//清空窗体中的所有控件
            this.InitializeComponent();//初始化窗体控件
        }
        #endregion
        #region 定义变量
        int k;//存储转角情况
        double bhc;
        double xbhc;
        double ybhc;
        double xybhc;
        List<string> dianhao;//点号
        List<double> guancejiao;//观测角，弧度参与计算
        List<double> fangweijiao;//方位角
        List<double> bianchang;//边长
        List<double> Xzengliang;//X坐标增量
        List<double> Yzengliang;//Y坐标增量
        List<double> Xzuobiao;//X坐标
        List<double> Yzuobiao;//Y坐标
        Bitmap image;
        #endregion
        #region 变量初始化
        public void chushihua()
        {
            dianhao = new List<string>();
            guancejiao = new List<double>();
            fangweijiao = new List<double>();
            bianchang = new List<double>();
            Xzengliang = new List<double>();
            Yzengliang = new List<double>();
            Xzuobiao = new List<double>();
            Yzuobiao = new List<double>();
        }
        #endregion
        #region 文件打开
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            richTextBox1.Text = "";
            pictureBox1.Image = null;
            try
            {
                openFileDialog1.Title = "文件打开";
                openFileDialog1.Filter = "文本文档(*.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        sr.ReadLine();
                        string[] str;
                        List<string[]> arrstr = new List<string[]>();//用于存储已知数据
                        int q = 0;//存储行号
                        double s;//存储角度
                        for (int i = 0; i < 4; i++)//存储已知数据
                        {
                            str = sr.ReadLine().Split(',');
                            arrstr.Add(str);
                        }
                        dataGridView1.Rows.Add();
                        for (int i = 0; i < 2; i++)
                        {
                            dataGridView1.Rows.Add();
                            dataGridView1.Rows[i * 2].Cells[0].Value = arrstr[i][0];
                            dataGridView1.Rows[i * 2].Cells[6].Value = arrstr[i][1];
                            dataGridView1.Rows[i * 2].Cells[7].Value = arrstr[i][2];
                            q++;
                        }
                        while (true)
                        {
                            double jiaodu;
                            str = sr.ReadLine().Split(',');
                            dataGridView1.Rows.Add();
                            dataGridView1.Rows[q].Cells[0].Value = str[0];
                            str = sr.ReadLine().Split(',');
                            jiaodu = Convert.ToDouble(str[2]);
                            s = jiaodu;
                            str = sr.ReadLine().Split(',');
                            if (str.Length == 1)
                                break;
                            jiaodu = Convert.ToDouble(str[2]) - jiaodu;
                            dataGridView1.Rows[q].Cells[1].Value = caculate.dmstojiaodu(jiaodu);
                            dataGridView1.Rows.Add();
                            q = q + 2;
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            dataGridView1.Rows[q - 2 + i * 2].Cells[0].Value = arrstr[i + 2][0];
                            dataGridView1.Rows[q - 2 + i * 2].Cells[6].Value = arrstr[i + 2][1];
                            dataGridView1.Rows[q - 2 + i * 2].Cells[7].Value = arrstr[i + 2][2];
                        }
                        q = 0;
                        dataGridView1.Rows[3].Cells[3].Value = s;//把存储的边长输出
                        while (true)
                        {
                            q = q + 2;
                            str = sr.ReadLine().Split(',');
                            dataGridView1.Rows[3 + q].Cells[3].Value = str[2];
                            if (!sr.EndOfStream)
                            {
                                sr.ReadLine();
                            }
                            else
                            {
                                break;
                            }
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
        #region 平差计算
        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chushihua();
            dataGridView1.AllowUserToAddRows = false;
            #region 数据导入
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i = i + 2)//每隔两个导入一个
                {
                    dianhao.Add(dataGridView1.Rows[i].Cells[0].Value.ToString());
                    if (i != 0 && i != dataGridView1.Rows.Count - 2)
                    {
                        guancejiao.Add(caculate.dmstohudu(dataGridView1.Rows[i].Cells[1].Value.ToString().Replace(" ", "")));
                    }
                    if (i != 0 && i != dataGridView1.Rows.Count - 2 && i != dataGridView1.Rows.Count - 4)
                    {
                        bianchang.Add(Convert.ToDouble(dataGridView1.Rows[i + 1].Cells[3].Value.ToString().Replace(" ", "")));
                    }
                    if (i == 0 || i == 2 || i == dataGridView1.Rows.Count - 4 || i == dataGridView1.Rows.Count - 2)
                    {
                        Xzuobiao.Add(Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value.ToString().Replace(" ", "")));
                        Yzuobiao.Add(Convert.ToDouble(dataGridView1.Rows[i].Cells[7].Value.ToString().Replace(" ", "")));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            //MessageBox.Show(caculate.hudutodms(guancejiao[0]));
            //MessageBox.Show(bianchang[0].ToString());
            //MessageBox.Show(Yzuobiao[0].ToString());
            #endregion
            try
            {
                #region 判断左右角
                if (rdb_left.Checked)
                {
                    k = 1;
                }
                else
                {
                    k = -1;
                }
                #endregion
                #region 方位角计算
                double n = 0;//存储加减360的信息
                List<double> fangweijiaoi = new List<double>();//存储粗略的方位角
                MessageBox.Show(Xzuobiao[0].ToString());
                MessageBox.Show(Yzuobiao[0].ToString());
                MessageBox.Show(Xzuobiao[1].ToString());
                MessageBox.Show(Yzuobiao[1].ToString());
                double fangweijiao1 = caculate.fangwei(Xzuobiao[0], Yzuobiao[0], Xzuobiao[1], Yzuobiao[1]);
                double fangweijiao2 = caculate.fangwei(Xzuobiao[2], Yzuobiao[2], Xzuobiao[3], Yzuobiao[3]);
                MessageBox.Show(caculate.hudutodms(fangweijiao1).ToString());

                fangweijiaoi.Add(fangweijiao1);
                for (int i = 0; i < guancejiao.Count; i++)
                {
                    double fangwei = fangweijiaoi[i] + k * guancejiao[i] - k * Math.PI;
                    if (fangwei > Math.PI * 2)
                    {
                        fangwei = fangwei - Math.PI * 2;
                        n = n - Math.PI * 2;
                    }
                    else if (fangwei < 0)
                    {
                        fangwei = fangwei + Math.PI * 2;
                        n = n + Math.PI * 2;
                    }
                    fangweijiaoi.Add(fangwei);
                }
                //MessageBox.Show(caculate.hudutodms(fangweijiaoi[1]));
                #endregion
                #region 角度近似平差
                bhc = fangweijiao1 + k * guancejiao.Sum() - k * guancejiao.Count * Math.PI - fangweijiao2 + n;
                //MessageBox.Show(caculate.hudutos(bhc).ToString());
                if (caculate.hudutos(bhc) > 24 * Math.Sqrt(guancejiao.Count))
                {
                    MessageBox.Show("角度闭合差超限！");
                }
                else
                {
                    fangweijiao.Add(fangweijiao1);
                    for (int i = 0; i < guancejiao.Count; i++)
                    {
                        double fangwei;
                        fangwei = fangweijiao[i] + k * (guancejiao[i] - bhc / guancejiao.Count) - k * Math.PI;
                        if (fangwei > Math.PI * 2)
                        {
                            fangwei = fangwei - Math.PI * 2;
                        }
                        else if (fangwei < 0)
                        {
                            fangwei = fangwei + Math.PI * 2;
                        }
                        fangweijiao.Add(fangwei);
                    }
                }
                for (int i = 1; i <= guancejiao.Count; i++)//输出改正后观测角
                {
                    dataGridView1.Rows[i * 2].Cells[1].Value = caculate.hudutodms(guancejiao[i - 1] - bhc / guancejiao.Count);
                }
                for (int i = 0; i < fangweijiao.Count; i++)//输出方位角
                {
                    dataGridView1.Rows[i * 2 + 1].Cells[2].Value = caculate.hudutodms(fangweijiao[i]);
                }
                #endregion
                #region 坐标近似平差
                for (int i = 0; i < bianchang.Count; i++)
                {
                    Xzengliang.Add(bianchang[i] * Math.Cos(fangweijiao[i + 1]));
                    Yzengliang.Add(bianchang[i] * Math.Sin(fangweijiao[i + 1]));
                }
                xbhc = Xzuobiao[1] + Xzengliang.Sum() - Xzuobiao[2];
                ybhc = Yzuobiao[1] + Yzengliang.Sum() - Yzuobiao[2];
                xybhc = Math.Sqrt(xbhc * xbhc + ybhc * ybhc);
                if (xybhc / bianchang.Sum() > (1 / 5000d))//5000d必须加d，要不然算出来是0
                {
                    MessageBox.Show("导线全长相对闭合差超限！");
                }
                //MessageBox.Show(xbhc.ToString());
                //MessageBox.Show(ybhc.ToString());
                for (int i = 1; i <= Xzengliang.Count; i++)//显示的是改正后的量
                {
                    dataGridView1.Rows[i * 2 + 1].Cells[4].Value = Math.Round(Xzengliang[i - 1] - xbhc * bianchang[i - 1] / bianchang.Sum(), 4);
                    dataGridView1.Rows[i * 2 + 1].Cells[5].Value = Math.Round(Yzengliang[i - 1] - ybhc * bianchang[i - 1] / bianchang.Sum(), 4);
                }
                for (int i = 0; i < bianchang.Count; i++)
                {
                    Xzuobiao.Insert(i + 2, Xzuobiao[i + 1] + Xzengliang[i] - xbhc * bianchang[i] / bianchang.Sum());
                    Yzuobiao.Insert(i + 2, Yzuobiao[i + 1] + Yzengliang[i] - ybhc * bianchang[i] / bianchang.Sum());
                    dataGridView1.Rows[i * 2 + 4].Cells[6].Value = Math.Round(Xzuobiao[i + 2], 4);
                    dataGridView1.Rows[i * 2 + 4].Cells[7].Value = Math.Round(Yzuobiao[i + 2], 4);
                }
                Xzuobiao.RemoveAt(Xzuobiao.Count - 2);//x最后计算会多出一个
                Yzuobiao.RemoveAt(Yzuobiao.Count - 2);
                #endregion
                #region 数据导出
                richTextBox1.Text = "******************************************************\n*******************附和导线近似平差*******************\n\n";
                richTextBox1.Text += "---------------限差要求---------------\n";
                richTextBox1.Text += "角度闭合差限差：" + Math.Round(24 * Math.Sqrt(guancejiao.Count), 3) + "\n";
                richTextBox1.Text += "导线全长相对闭合差限差" + (1 / 5000d) + "\n\n";
                richTextBox1.Text += "---------------导线基本信息---------------\n";
                richTextBox1.Text += "测站数：" + guancejiao.Count + "\n";
                richTextBox1.Text += "导线全长" + bianchang.Sum() + "\n";
                richTextBox1.Text += "角度闭合差：" + caculate.hudutos(bhc) + "″\n";
                richTextBox1.Text += "各站角度改正值：" + caculate.hudutos(bhc / guancejiao.Count) + "″\n";
                richTextBox1.Text += "X坐标闭合差：" + Math.Round(xbhc, 4) + "\n";
                richTextBox1.Text += "Y坐标闭合差：" + Math.Round(ybhc, 4) + "\n";
                richTextBox1.Text += "导线全长闭合差：" + Math.Round(xybhc / bianchang.Sum(), 8) + "\n\n";
                richTextBox1.Text += "---------------测站点坐标---------------\n";
                richTextBox1.Text += "测站        \tX坐标       \tY坐标\n";
                for (int i = 0; i < Xzuobiao.Count; i++)
                {
                    richTextBox1.Text += string.Format("{0,-8}", dianhao[i]) + "\t";
                    richTextBox1.Text += string.Format("{0,-8}", Math.Round(Xzuobiao[i], 4)) + "\t";
                    richTextBox1.Text += string.Format("{0,-8}", Math.Round(Yzuobiao[i], 4)) + "\n";
                }
                richTextBox1.Text += "---------------角度数据---------------\n";
                richTextBox1.Text += "            \t            \t方位角\n";
                richTextBox1.Text += "测站名      \t观测角\n";
                for (int i = 0; i < fangweijiao.Count; i++)
                {
                    richTextBox1.Text += "            \t            \t" + caculate.hudutodms(fangweijiao[i]) + "\n";
                    if (i != fangweijiao.Count - 1)
                    {
                        richTextBox1.Text += string.Format("{0,-8}", dianhao[i]) + "\t";//输出的是没有改正后的观测角
                        richTextBox1.Text += string.Format("{0,-8}", caculate.hudutodms(guancejiao[i])) + "\n";
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            计算ToolStripMenuItem_Click(sender, e);
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
                int width = (int)(Yzuobiao.Max() - Yzuobiao.Min() + 200);//图幅长
                int heigth = (int)(Xzuobiao.Max() - Xzuobiao.Min() + 200);//图幅宽
                image = new Bitmap(width, heigth);
                Graphics g = Graphics.FromImage(image);
                PointF[] pf = new PointF[Xzuobiao.Count];
                #region 绘制线型
                for (int i = 0; i < Xzuobiao.Count; i++)
                {
                    pf[i].X = (float)(Yzuobiao[i] - Yzuobiao.Min() + 100);
                    pf[i].Y = -(float)(Xzuobiao[i] - Xzuobiao.Max() - 100);
                }
                g.DrawLines(p, pf);
                #endregion
                #region 绘制注记，点
                for (int i = 0; i < Xzuobiao.Count; i++)
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
                g.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
                g.DrawString("Y", new Font("宋体", 30), Brushes.Black, width - 60, heigth - 40);//注记文字是在点位的右上角绘制
                for (int i = 0; i <= (int)(Xzuobiao.Max() - Xzuobiao.Min()); i = i + (int)((Xzuobiao.Max() - Xzuobiao.Min()) / 10))
                {
                    g.DrawString(((int)(Xzuobiao.Min() + i)).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth - 100 - i);
                    g.DrawLine(p, 50, heigth - 100 - i, 65, heigth - 100 - i);
                }
                for (int i = 0; i <= (int)(Yzuobiao.Max() - Yzuobiao.Min()); i = i + (int)((Yzuobiao.Max() - Yzuobiao.Min()) / 10))
                {
                    g.DrawString(((int)(Yzuobiao.Min() + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth - 40);
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

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            绘图ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 计算保存
        private void 表格保存ToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void 报告保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "报告保存";
                saveFileDialog1.Filter = "文本文档(*.txt)|*.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (richTextBox1.Text != "")
                    {
                        this.richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                        this.richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
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

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            报告保存ToolStripMenuItem_Click(sender, e);
        }
        #endregion
        #region 绘图保存
        private void bmp图形ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "bmp图形保存";
                saveFileDialog1.Filter = "位图文件(*.bmp)|*.bmp";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    image.Save(saveFileDialog1.FileName);
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void dxf图形ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "dxf图形保存";
                saveFileDialog1.Filter = "cad交互图形(*.dxf)|*.dxf";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.Write("0\nSECTION\n2\nTABLES\n0\nTABLE\n2\nLAYER\n0\nLAYER\n70\n0\n2\nshiti\n62\n10\n6\nCONTINUOUS\n");
                        sw.Write("0\nLAYER\n70\n0\n2\nzhuji\n62\n50\n6\nCONTINUOUS\n0\nLAYER\n70\n0\n2\nqita\n62\n90\n6\nCONTINUOUS\n0\nENDTAB\n0\nENDSEC\n");
                        sw.Write("0\nSECTION\n2\nENTITIES\n");
                        for (int i = 0; i < Xzuobiao.Count; i++)
                        {
                            sw.Write("0\nPOINT\n8\nshiti\n");
                            sw.Write("10\n" + Yzuobiao[i] + "\n");
                            sw.Write("20\n" + Xzuobiao[i] + "\n");
                        }
                        sw.Write("0\nPOLYLINE\n8\nshiti\n66\n1\n");
                        for (int i = 0; i < Xzuobiao.Count; i++)
                        {
                            sw.Write("0\nVERTEX\n8\nshiti\n");
                            sw.Write("10\n" + Yzuobiao[i] + "\n");
                            sw.Write("20\n" + Xzuobiao[i] + "\n");
                        }
                        sw.Write("0\nSEQEND\n");
                        for (int i = 0; i < Xzuobiao.Count - 1; i++)
                        {
                            if (i == 0 || i == Xzuobiao.Count - 2)
                            {
                                sw.Write("0\nLINE\n8\nqita\n");
                                sw.Write("10\n" + Yzuobiao[i] + "\n");
                                sw.Write("20\n" + Xzuobiao[i] + "\n");
                                sw.Write("11\n" + Yzuobiao[i + 1] + "\n");
                                sw.Write("21\n" + Xzuobiao[i + 1] + "\n");
                            }
                        }
                        for (int i = 0; i < Xzuobiao.Count; i++)
                        {
                            sw.Write("0\nTEXT\n8\nzhuji\n");
                            sw.Write("10\n" + (Yzuobiao[i] - 3) + "\n");
                            sw.Write("20\n" + (Xzuobiao[i] - 3) + "\n");
                            sw.Write("40\n5\n1\n" + dianhao[i] + "\n");
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
