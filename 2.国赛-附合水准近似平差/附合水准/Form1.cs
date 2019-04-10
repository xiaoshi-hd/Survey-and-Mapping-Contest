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

namespace 附合水准
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region 变量声明
        Bitmap image;
        double bhc;
        List<string> dianhao;//所有点号
        List<string> dian;//高程点号

        List<double> shijuheleiji;//累计视距
        List<double> shijuf;//站视距
        List<double> shijuzhi;//视距
        List<double> gaochaf;//站高差
        List<double> gaochazhi;//高差

        List<double> gaizhengshu;//高差改正数
        List<double> gaihougaocha;//改正后高差
        List<double> gaocheng;//高程
        List<double> P;
        List<double> Pvv;
        #endregion
        #region 变量初始化
        public void chushihua()
        {
            dianhao = new List<string>();
            dian = new List<string>();

            shijuheleiji = new List<double>();
            shijuf = new List<double>();
            gaochaf = new List<double>();
            shijuzhi = new List<double>();
            gaochazhi = new List<double>();

            gaizhengshu = new List<double>();
            gaihougaocha = new List<double>();
            gaocheng = new List<double>();
            P = new List<double>(); 
            Pvv = new List<double>();
        }
        #endregion
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
                pictureBox1.Width = (int)(pictureBox1.Width * 1.2);
                pictureBox1.Height = (int)(pictureBox1.Height * 1.2);
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
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
        #region 文件打开
        private void 文件导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                txt_qidian.Text = "";//清空所有窗体控件
                txt_zhongdian.Text = "";
                dataGridView2.Rows.Clear();
                dataGridView3.Rows.Clear();
                pictureBox1.Image = null;
                richTextBox1.Text = "";

                openFileDialog1.Title = "附和水准数据打开";
                openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default))
                    {
                        string[] str = sr.ReadLine().Split(',');
                        txt_qidian.Text = str[1];
                        str = sr.ReadLine().Split(',');
                        txt_zhongdian.Text = str[1];
                        int i = 0;
                        sr.ReadLine();
                        while (!sr.EndOfStream)
                        {
                            dataGridView2.Rows.Add();
                            str = sr.ReadLine().Split(',');
                            dataGridView2.Rows[i].Cells[0].Value = i + 1;
                            dataGridView2.Rows[i].Cells[1].Value = str[0];
                            dataGridView2.Rows[i].Cells[2].Value = str[1];
                            dataGridView2.Rows[i].Cells[3].Value = str[2];
                            dataGridView2.Rows[i].Cells[4].Value = str[4];
                            dataGridView2.Rows[i].Cells[5].Value = str[6];
                            dataGridView2.Rows[i].Cells[6].Value = str[8];
                            dataGridView2.Rows[i].Cells[7].Value = str[3];
                            dataGridView2.Rows[i].Cells[8].Value = str[5];
                            dataGridView2.Rows[i].Cells[9].Value = str[7];
                            dataGridView2.Rows[i].Cells[10].Value = str[9];

                            if (Convert.ToDouble(dataGridView2.Rows[i].Cells[3].Value) > 80)
                            { dataGridView2.Rows[i].Cells[3].Style.ForeColor = Color.Red; }
                            if (Convert.ToDouble(dataGridView2.Rows[i].Cells[4].Value) > 80)
                            { dataGridView2.Rows[i].Cells[4].Style.ForeColor = Color.Red; }
                            if (Convert.ToDouble(dataGridView2.Rows[i].Cells[5].Value) > 80)
                            { dataGridView2.Rows[i].Cells[5].Style.ForeColor = Color.Red; }
                            if (Convert.ToDouble(dataGridView2.Rows[i].Cells[6].Value) > 80)
                            { dataGridView2.Rows[i].Cells[6].Style.ForeColor = Color.Red; }
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
            文件导入ToolStripMenuItem_Click(sender,e);
        }
        #endregion
        #region 水准平差
        #region 数据检核+数据导入+数据预处理
        private void 数据处理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chushihua();
            dataGridView2.AllowUserToAddRows = false;
            dataGridView3.AllowUserToAddRows = false;

            try
            {
                gaocheng.Add(Convert.ToDouble(txt_qidian.Text.Replace(" ","")));
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    dataGridView3.Rows.Add();//预处理计算
                    dataGridView3.Rows[i].Cells[0].Value = dataGridView2.Rows[i].Cells[0].Value.ToString().Replace(" ", "");
                    dataGridView3.Rows[i].Cells[1].Value = Math.Round(Convert.ToDouble(dataGridView2.Rows[i].Cells[3].Value.ToString().Replace(" ", "")) - Convert.ToDouble(dataGridView2.Rows[i].Cells[4].Value.ToString().Replace(" ", "")), 3);
                    dataGridView3.Rows[i].Cells[2].Value = Math.Round(Convert.ToDouble(dataGridView2.Rows[i].Cells[6].Value.ToString().Replace(" ", "")) - Convert.ToDouble(dataGridView2.Rows[i].Cells[5].Value.ToString().Replace(" ", "")), 3);
                    dataGridView3.Rows[i].Cells[3].Value = Math.Round((Convert.ToDouble(dataGridView3.Rows[i].Cells[1].Value) + Convert.ToDouble(dataGridView3.Rows[i].Cells[2].Value)) / 2, 3);
                    if (dataGridView2.Rows[i].Cells[1].Value.ToString() != "-1")//按测段计算累计视距差
                    {
                        dataGridView3.Rows[i].Cells[4].Value = dataGridView3.Rows[i].Cells[3].Value;
                    }
                    else
                    {
                        dataGridView3.Rows[i].Cells[4].Value = Math.Round(Convert.ToDouble(dataGridView3.Rows[i - 1].Cells[4].Value) + Convert.ToDouble(dataGridView3.Rows[i].Cells[3].Value), 4);
                    }
                    dataGridView3.Rows[i].Cells[5].Value = Math.Round((Convert.ToDouble(dataGridView2.Rows[i].Cells[7].Value.ToString().Replace(" ", "")) - Convert.ToDouble(dataGridView2.Rows[i].Cells[10].Value.ToString().Replace(" ", ""))), 4);
                    dataGridView3.Rows[i].Cells[6].Value = Math.Round((Convert.ToDouble(dataGridView2.Rows[i].Cells[8].Value.ToString().Replace(" ", "")) - Convert.ToDouble(dataGridView2.Rows[i].Cells[9].Value.ToString().Replace(" ", ""))), 4);
                    dataGridView3.Rows[i].Cells[7].Value = Math.Round((Convert.ToDouble(dataGridView3.Rows[i].Cells[5].Value) - Convert.ToDouble(dataGridView3.Rows[i].Cells[6].Value)), 4);
                    dataGridView3.Rows[i].Cells[8].Value = Math.Round(Convert.ToDouble(dataGridView2.Rows[i].Cells[7].Value.ToString().Replace(" ", "")) - Convert.ToDouble(dataGridView2.Rows[i].Cells[8].Value.ToString().Replace(" ", "")), 4);
                    dataGridView3.Rows[i].Cells[9].Value = Math.Round(Convert.ToDouble(dataGridView2.Rows[i].Cells[10].Value.ToString().Replace(" ", "")) - Convert.ToDouble(dataGridView2.Rows[i].Cells[9].Value.ToString().Replace(" ", "")), 4);
                    dataGridView3.Rows[i].Cells[10].Value = Math.Round((Convert.ToDouble(dataGridView3.Rows[i].Cells[8].Value) + Convert.ToDouble(dataGridView3.Rows[i].Cells[9].Value)) / 2, 4);
                    
                    dianhao.Add(dataGridView2.Rows[i].Cells[1].Value.ToString());//主要变量赋值
                    shijuf.Add(Convert.ToDouble(dataGridView2.Rows[i].Cells[3].Value) * 0.5 + Convert.ToDouble(dataGridView2.Rows[i].Cells[4].Value) * 0.5 + Convert.ToDouble(dataGridView2.Rows[i].Cells[5].Value) * 0.5 + Convert.ToDouble(dataGridView2.Rows[i].Cells[6].Value) * 0.5);
                    gaochaf.Add(Convert.ToDouble(dataGridView3.Rows[i].Cells[10].Value));

                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[1].Value)) > 5)
                    { dataGridView3.Rows[i].Cells[1].Style.ForeColor = Color.Red; }
                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[2].Value)) > 5)
                    { dataGridView3.Rows[i].Cells[2].Style.ForeColor = Color.Red; }
                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[3].Value)) > 5)
                    { dataGridView3.Rows[i].Cells[3].Style.ForeColor = Color.Red; }
                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[4].Value)) > 10)
                    { dataGridView3.Rows[i].Cells[4].Style.ForeColor = Color.Red; }
                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[5].Value)) > 0.003)
                    { dataGridView3.Rows[i].Cells[5].Style.ForeColor = Color.Red; }
                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[6].Value)) > 0.003)
                    { dataGridView3.Rows[i].Cells[6].Style.ForeColor = Color.Red; }
                    if (Math.Abs(Convert.ToDouble(dataGridView3.Rows[i].Cells[7].Value)) > 0.005)
                    { dataGridView3.Rows[i].Cells[7].Style.ForeColor = Color.Red; }
                }
                dianhao.Add(dataGridView2.Rows[dataGridView2.Rows.Count - 1].Cells[2].Value.ToString());
            }
            catch
            {
                MessageBox.Show("请输入正确的数据！");
                return;
            }
        }
        #endregion
        #region 平差计算
        private void 平差计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                bhc = gaochaf.Sum() - (Convert.ToDouble(txt_zhongdian.Text) - Convert.ToDouble(txt_qidian.Text));//观测值减去真实值
                //MessageBox.Show(bhc.ToString());
                double shiju = 0, gaocha = 0;
                dian.Add(dianhao[0]);
                shijuheleiji.Add(0);
                for (int i = 0; i < gaochaf.Count; i++)
                {
                    shiju += shijuf[i];
                    gaocha += gaochaf[i];
                    if (dianhao[i + 1] != "-1")
                    {
                        dian.Add(dianhao[i + 1]);
                        gaochazhi.Add(gaocha);
                        shijuzhi.Add(shiju);
                        shiju = 0;
                        gaocha = 0;
                    }
                }
                for (int i = 0; i < gaochazhi.Count; i++)//改正分配
                {
                    shijuheleiji.Add(shijuheleiji[i] + shijuzhi[i]);
                    gaizhengshu.Add(-bhc * shijuzhi[i] / shijuzhi.Sum());
                    gaihougaocha.Add(gaochazhi[i] + gaizhengshu[i]);
                    gaocheng.Add(gaihougaocha[i] + gaocheng[i]);
                }
                dataGridView1.Rows.Add();
                dataGridView1.Rows[0].Cells[0].Value = dian[0];
                dataGridView1.Rows[0].Cells[5].Value = gaocheng[0];
                for (int i = 0; i < gaochazhi.Count; i++)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i * 2 + 1].Cells[1].Value = Math.Round(shijuzhi[i], 3);
                    dataGridView1.Rows[i * 2 + 1].Cells[2].Value = gaochazhi[i];
                    dataGridView1.Rows[i * 2 + 1].Cells[3].Value = Math.Round(gaizhengshu[i] * 1000, 4);
                    dataGridView1.Rows[i * 2 + 1].Cells[4].Value = Math.Round(gaizhengshu[i] + gaochazhi[i], 4);
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[(i + 1) * 2].Cells[0].Value = dian[i + 1];
                    dataGridView1.Rows[(i + 1) * 2].Cells[5].Value = Math.Round(gaocheng[i + 1], 4);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
        #region 生成报告
        private void 生成报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                double vv = 0;
                double Mu;
                for (int i = 0; i < gaochazhi.Count - 1; i++)
                {
                    P.Add(1000 / shijuheleiji[i + 1] + 1000 / (shijuzhi.Sum() - shijuheleiji[i + 1]));
                    //P.Add(1000 / shijuzhi[i]);
                    vv = vv + P[i] * gaizhengshu[i] * gaizhengshu[i];
                }
                Mu = Math.Sqrt(vv / (gaochazhi.Count - (dian.Count - 2)));
                richTextBox1.Text = "水准平差\n";
                richTextBox1.Text += "\t已知点数据\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                richTextBox1.Text += dian[0] + " " + txt_qidian.Text + "\t" + dian[dian.Count - 1] + " " + txt_zhongdian.Text + "\n";
                richTextBox1.Text += "测站数：" + gaochaf.Count + "\n";
                richTextBox1.Text += "测段数：" + gaochazhi.Count + "\n";
                richTextBox1.Text += "总距离：" + shijuzhi.Sum() + "\n";
                richTextBox1.Text += "高程闭合差(mm)：" + Math.Round(bhc * 1000, 4) + "\n";
                richTextBox1.Text += "\n\t原始数据\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                Class1.daochu1(richTextBox1, dataGridView2);
                richTextBox1.Text += "\n\t预处理数据\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                Class1.daochu1(richTextBox1, dataGridView3);
                richTextBox1.Text += "\n\t高程配赋数据\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                Class1.daochu1(richTextBox1, dataGridView1);
                richTextBox1.Text += "精度数据\n";
                richTextBox1.Text += "--------------------------------------------------------------------------------------------------\n";
                richTextBox1.Text += "单位权中误差： \nMu = ±" + Math.Round(Mu, 4) + "\n";
                richTextBox1.Text += "\n待测点高程中误差：\n";
                for (int i = 0; i < dian.Count - 2; i++)
                {
                    richTextBox1.Text += dian[i + 1] + " = ±" + Math.Round(Mu / Math.Sqrt(P[i]), 4) + "\n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
        #region 绘图
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pen p = new Pen(Color.Black, 2.5f);//高差乘以1000
                SolidBrush brush = new SolidBrush(Color.Black);//定义填充
                int width = (int)(shijuheleiji.Max() - shijuheleiji.Min()) + 200;
                int heigth = (int)(gaocheng.Max() * 1000 - gaocheng.Min() * 1000) + 200;
                image = new Bitmap(width, heigth);//显示图形范围
                Graphics g = Graphics.FromImage(image);
                //绘制线
                PointF[] pf = new PointF[dian.Count];
                for (int i = 0; i < dian.Count; i++)
                {
                    pf[i].X = (float)(shijuheleiji[i] - shijuheleiji.Min() + 100);
                    pf[i].Y = -(float)(gaocheng[i] * 1000 - gaocheng.Max() * 1000 - 100);
                }
                g.DrawLines(p, pf);
                //绘制字体
                for (int i = 0; i < dian.Count; i++)
                {
                    g.FillEllipse(brush, pf[i].X - 5, pf[i].Y - 5, 10, 10);
                    g.DrawString(dian[i], new Font("宋体", 15), Brushes.Red, pf[i].X - 5, pf[i].Y - 5);
                }
                pictureBox1.Image = image;
                //绘制坐标轴
                PointF[] xpt = new PointF[3] { new PointF(50, 35), new PointF(40, 50), new PointF(60, 50) };
                PointF[] ypt = new PointF[3] { new PointF(width - 35, heigth - 50), new PointF(width - 50, heigth - 60), new PointF(width - 50, heigth - 40) };
                g.DrawLine(p, 50, heigth - 50, 50, 50);//画x轴
                g.DrawLine(p, 50, heigth - 50, width - 50, heigth - 50);//画y轴
                g.FillPolygon(brush, xpt);//x轴箭头
                g.FillPolygon(brush, ypt);//y轴箭头
                g.DrawString("X", new Font("宋体", 30), Brushes.Black, 20, 40);
                g.DrawString("Y", new Font("宋体", 30), Brushes.Black, width - 60, heigth - 40);//注记文字是在点位的右上角绘制
                for (int i = 0; i <= (int)((gaocheng.Max() - gaocheng.Min()) * 1000); i = i + (int)((gaocheng.Max() - gaocheng.Min()) * 100))
                {
                    g.DrawString((gaocheng.Min() + i / 1000d).ToString(), new Font("宋体", 20), Brushes.Black, 0, heigth - 100 - i);
                    g.DrawLine(p, 50, heigth - 100 - i, 65, heigth - 100 - i);
                }
                for (int i = 0; i <= (int)(shijuheleiji.Max() - shijuheleiji.Min()); i = i + (int)((shijuheleiji.Max() - shijuheleiji.Min()) / 10))
                {
                    g.DrawString(((int)(shijuheleiji.Min() + i)).ToString(), new Font("宋体", 20), Brushes.Black, 100 + i, heigth - 40);
                    g.DrawLine(p, 100 + i, heigth - 50, 100 + i, heigth - 65);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
        private void 一键生成ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            数据处理ToolStripMenuItem_Click(sender, e);
            平差计算ToolStripMenuItem_Click(sender, e);
            生成报告ToolStripMenuItem_Click(sender, e);
            绘图ToolStripMenuItem_Click(sender, e);
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            一键生成ToolStripMenuItem1_Click(sender, e);
        }
        #endregion
        private void 概图导出ToolStripMenuItem_Click(object sender, EventArgs e)
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
                            sw.Write("2\nTABLES\n");//表段的开始 不需要END结束
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

                            sw.Write("0\nLAYER\n");//注记图层
                            sw.Write("70\n");
                            sw.Write("0\n");
                            sw.Write("2\nzhuji\n");
                            sw.Write("62\n");
                            sw.Write("50\n");
                            sw.Write("6\n");
                            sw.Write("CONTINUOUS\n");
                            #endregion
                            sw.Write("0\nENDTAB\n");//TABLE段结束
                            #endregion
                            sw.Write("0\nENDSEC\n");//第一段结束

                            sw.Write("0\nSECTION\n");//第二个段的开始
                            #region 实体段
                            sw.Write("2\nENTITIES\n");//实体段开始

                            #region 绘制线路
                            sw.Write("0\nPOLYLINE\n");//多线段绘制，为一个整体线段
                            sw.Write("8\n");//图层
                            sw.Write("shiti\n");//没有图层的话会创建一个，有图层可以调用创建的图层，以默认设置
                            sw.Write("66\n");//不太懂，应该是多线个数
                            sw.Write("1\n");
                            for (int i = 0; i < shijuheleiji.Count; i++)
                            {
                                sw.Write("0\nVERTEX\n");//多线段标识
                                sw.Write("8\n");//图层
                                sw.Write("shiti\n");
                                sw.Write("10\n");//X坐标
                                sw.Write(shijuheleiji[i] + "\n");
                                sw.Write("20\n");//Y坐标
                                sw.Write(gaocheng[i] + "\n");
                            }
                            sw.Write("0\nSEQEND\n");//多线段结束
                            #endregion
                            #region 文字注记
                            for (int i = 0; i < shijuheleiji.Count; i++)
                            {
                                sw.Write("0\nTEXT\n");//单行文字
                                sw.Write("8\n");
                                sw.Write("zhuji\n");
                                sw.Write("10\n");//字体起点X
                                sw.Write(shijuheleiji[i] - 5 + "\n");
                                sw.Write("20\n");//字体起点Y
                                sw.Write(gaocheng[i] - 5 + "\n");
                                sw.Write("40\n5\n");//字体高度
                                sw.Write("1\n" + dian[i] + "\n");//文字内容
                            }
                            #endregion

                            #endregion
                            sw.Write("0\nENDSEC\n");//第二段结束
                            sw.Write("0\nEOF\n");//文件结束
                        }
                    }
                    #endregion
                    #region BMP
                    if (saveFileDialog1.FilterIndex == 2)
                    {
                        image.Save(saveFileDialog1.FileName);
                    }
                    #endregion
                    MessageBox.Show("保存成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #region 数据导出
        private void 数据导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Title = "附和水准计算保存";
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
            数据导出ToolStripMenuItem_Click(sender,e);
        }
        #endregion
    }
}
