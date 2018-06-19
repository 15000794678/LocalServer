using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using MakeModel;
using System.IO;

namespace LocalServer
{
    public partial class MakeModel : Office2007Form
    {
        private HalconBoardTestDll hbt = new HalconBoardTestDll();

        public MakeModel()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();            

            ofd.Filter = "BMP files(*.bmp)|*.bmp|bmp files(*.bmp)|*.bmp|All files(*.*)|*.*"; ;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBox_bmp.Text = ofd.FileName;
            pictureBox1.ImageLocation = ofd.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox_bmp.Text.Trim()))
            {
                MessageBox.Show("请先选择要转换的图片");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "NCM files(*.ncm)|*.ncm|ncm files(*.ncm)|*.ncm|All files(*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBox_ncm.Text = sfd.FileName;
            if (hbt.Halcon_WriteModel(textBox_bmp.Text.Trim(), textBox_ncm.Text.Trim()) == 1)
            {
                MessageBox.Show("转换成功");
            }
            else
            {
                MessageBox.Show("转换失败");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "NCM files(*.ncm)|*.ncm|ncm files(*.ncm)|*.ncm|All files(*.*)|*.*";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBox1.Text = ofd.FileName;
            pictureBox1.ImageLocation = ofd.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Application.StartupPath;
            ofd.Filter = "BMP files(*.bmp)|*.bmp|bmp files(*.bmp)|*.bmp|All files(*.*)|*.*"; ;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            textBox2.Text = ofd.FileName;

            pictureBox1.ImageLocation = ofd.FileName;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox1.Text.Trim()) ||
                !File.Exists(textBox2.Text.Trim()))
            {
                MessageBox.Show("请先选择要测试图片的路径和模板路径");
                return;
            }

            int[] res = hbt.Halcon_ReadModel(textBox2.Text.Trim(), textBox1.Text.Trim(), 2, 0.75);
            int num = 0;

            textBox3.Text = "";
            while (num<res.Length)
            {
                textBox3.AppendText(res[num].ToString() + ", ");
                num++;
            }
        }
    }
}
