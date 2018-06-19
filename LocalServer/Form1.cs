using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Threading;
using System.IO;

namespace LocalServer
{
    public partial class Form1 : Office2007Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            //this.WindowState = FormWindowState.Maximized;
            this.Width = 500;
            this.Height = 500;
            this.WindowState = FormWindowState.Minimized;

            MyData.pixel = Screen.PrimaryScreen.Bounds.Width.ToString() + "x" + Screen.PrimaryScreen.Bounds.Height.ToString();
            //this.Hide();

            MyData.LoadCfg();
            
            InitUI();
        }

        private void InitUI()
        {
            this.WindowState = FormWindowState.Minimized;

            //初始化中间的测试界面
            this.IsMdiContainer = true;
            tableLayoutPanel3.Controls.Clear();
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.ColumnCount = MyData.num;

            for (int i = 0; i < MyData.num; i++)
            {
                tableLayoutPanel3.ColumnStyles.Add(new RowStyle(SizeType.Percent, 100 / MyData.num));
            }

            //初始化每个子窗体
            toolStripMenuItem2.Enabled = true;
            Color[] cl = new Color[2] { Color.FromArgb(255, 192, 192), Color.FromArgb(192, 255, 192) };
            for (int i = 1; i <= MyData.num; i++)
            {
                Form2 f2 = new Form2();
                f2.MdiParent = this;
                tableLayoutPanel3.Controls.Add(f2);
                f2.Dock = DockStyle.Fill;
                f2.SetBkColor(cl[(i + 1) % 2]);
                f2.Show();

                f2.SetNo(i);
                if (MyData.dic_port.ContainsKey(i))
                {                    
                    f2.SetComPort(MyData.dic_port[i].portname, MyData.dic_port[i].baudrate);
                }
                if (MyData.dic_xy.ContainsKey(i))
                {
                    f2.SetButtonXY(MyData.dic_xy[i].button_x, MyData.dic_xy[i].button_y);
                    f2.SetInputXY(MyData.dic_xy[i].input_x, MyData.dic_xy[i].input_y);
                }
                
                if (!f2.CheckReady())
                {
                    toolStripMenuItem2.Enabled = false;
                }
            }

            if (MyData.num == 1)
            {
                左边不出站ToolStripMenuItem.Visible = false;
                右边不出站ToolStripMenuItem.Visible = false;
                不出站ToolStripMenuItem.Visible = true;
            }
            else if (MyData.num == 2)
            {
                左边不出站ToolStripMenuItem.Visible = true;
                右边不出站ToolStripMenuItem.Visible = true;
                不出站ToolStripMenuItem.Visible = false;
            }

            label2.Text = "解决方案：" + MyData.dllclass +
                          ",    人工操作方式： " + ((MyData.startMode == 1) ? "输入框+回车" : "按钮") +
                          ",    通信方式： " + ((MyData.communicateMode == 1) ? "网口" : "串口") +
                          ",    分辨率： " + MyData.pixel +
                          ",    串口回复延时： " + MyData.comDelayTime +
                          ",    图像检测周期： " + MyData.pictureDelayTime;

        }

        private void 登陆ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 注销ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var obj in tableLayoutPanel3.Controls)
            {
                Form2 f2 = (Form2)obj;
                f2.Stop();
            }

            this.Close();
        }

        private void 测试配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestSet ts = new TestSet();

            if (ts.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            MyData.LoadCfg();
            InitUI();
        }

        private void 使用说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 版本号ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            if (mi.Text.Equals("一键启动"))
            {
                this.WindowState = FormWindowState.Minimized;
                foreach (var obj in tableLayoutPanel3.Controls)
                {
                    Form2 f2 = (Form2)obj;
                    f2.Start();
                }

                mi.Text = "一键停止";
                测试配置ToolStripMenuItem.Enabled = false;
            }
            else
            {
                foreach (var obj in tableLayoutPanel3.Controls)
                {
                    Form2 f2 = (Form2)obj;
                    f2.Stop();
                }

                mi.Text = "一键启动";
                测试配置ToolStripMenuItem.Enabled = true;
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void 图片测试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureTest pt = new PictureTest();
            pt.Show();

            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var obj in tableLayoutPanel3.Controls)
            {
                Form2 f2 = (Form2)obj;
                f2.Stop();
            }
        }

        private void 模板制作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeModel mm = new MakeModel();

            mm.ShowDialog();
        }

        private void 左边不出站ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            if (左边不出站ToolStripMenuItem.Text.Equals("左边不出站"))
            {
                左边不出站ToolStripMenuItem.Text = "√左边不出站";
                MyData.bLeftOut = false;
            }
            else
            {
                左边不出站ToolStripMenuItem.Text = "左边不出站";
                MyData.bLeftOut = true;
            }
        }

        private void 右边不出站ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            if (右边不出站ToolStripMenuItem.Text.Equals("右边不出站"))
            {
                右边不出站ToolStripMenuItem.Text = "√右边不出站";
                MyData.bRightOut = false;
            }
            else
            {
                右边不出站ToolStripMenuItem.Text = "右边不出站";
                MyData.bRightOut = true;
            }
        }

        private void 不出站ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            if (不出站ToolStripMenuItem.Text.Equals("不出站"))
            {
                不出站ToolStripMenuItem.Text = "√不出站";
                MyData.bLeftOut = false;
            }
            else
            {
                不出站ToolStripMenuItem.Text = "不出站";
                MyData.bLeftOut = true;
            }
        }

    }
}
