using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using DevComponents.DotNetBar;

namespace LocalServer
{
    public partial class CaptureImage : Office2007Form
    {
        public CaptureImage()
        {
            InitializeComponent();
        }

        //private void button_captureall_Click(object sender, EventArgs e)
        //{
        //    Image baseImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        //    Graphics g = Graphics.FromImage(baseImage);
        //    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);
        //    g.Dispose();
        //    baseImage.Save(Application.StartupPath + "\\baseImage.jpg", ImageFormat.Jpeg);

        //    if (File.Exists(Application.StartupPath + "\\baseImage.jpg"))
        //    {
        //        pictureBox1.Load(Application.StartupPath + "\\baseImage.jpg");
        //    }
        //}

        private void button_select_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
                if (File.Exists(ofd.FileName))
                {
                    pictureBox1.BackgroundImage = Image.FromFile(ofd.FileName);
                    pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                    //pictureBox1.Load(ofd.FileName);
                    label_w.Text = pictureBox1.BackgroundImage.Size.Width.ToString();
                    label_h.Text = pictureBox1.BackgroundImage.Size.Height.ToString();
                    textBox_x0.Enabled = true;
                    textBox_y0.Enabled = true;
                    textBox_w.Enabled = true;
                    textBox_h.Enabled = true;
                    button_preview.Enabled = true;
                    button_save.Enabled = true;
                }                
            }
        }

        private void ShowRectangle(Pen p, int x, int y, int w, int h)
        {
            Graphics g = pictureBox1.CreateGraphics();            
            g.DrawRectangle(Pens.Red, x, y, w, h);
            // 一定释放！否则容易内存泄露
            g.Dispose();
        }

        private void button_preview_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox_x0.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox_y0.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox_w.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox_h.Text.Trim()))
                {
                    MessageBox.Show("请先将信息填写完整");
                    return;
                }


                int x0, y0, w, h, max_x, max_y;
                
                x0 = int.Parse(textBox_x0.Text.Trim());
                y0 = int.Parse(textBox_y0.Text.Trim());
                w = int.Parse(textBox_w.Text.Trim());
                h = int.Parse(textBox_h.Text.Trim());
                max_x = int.Parse(label_w.Text.Trim());
                max_y = int.Parse(label_h.Text.Trim());
                if (x0 < 0 || y0 < 0 || w < 0 || h < 0 ||
                    x0 > max_x || y0 > max_y || w > max_x || h > max_y ||
                    x0 + w > max_x || y0 + h > max_y)
                {
                    MessageBox.Show("信息不合法，请重新填写");
                    return;
                }
                x0 = pictureBox1.Size.Width * x0 / pictureBox1.BackgroundImage.Size.Width;
                y0 = pictureBox1.Size.Height * y0 / pictureBox1.BackgroundImage.Size.Height;
                w = pictureBox1.Size.Width*w / pictureBox1.BackgroundImage.Size.Width;
                h = pictureBox1.Size.Height*h / pictureBox1.BackgroundImage.Size.Height;
                
                pictureBox1.Refresh();
                ShowRectangle(Pens.Red, x0, y0, w, h);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveImage(string srcpath, string despath, int x0, int y0, int w, int h)
        {
            //原图片文件
            Image fromImage = Image.FromFile(srcpath);
            //创建新图位图
            Bitmap bitmap = new Bitmap(int.Parse(textBox_w.Text.Trim()), int.Parse(textBox_h.Text.Trim()));
            //创建作图区域
            Graphics graphic = Graphics.FromImage(bitmap);
            //截取原图相应区域写入作图区
            graphic.DrawImage(fromImage, 0, 0,
                              new Rectangle(x0, y0, w, h),
                              GraphicsUnit.Pixel);
            //从作图区生成新图
            Image saveImage = Image.FromHbitmap(bitmap.GetHbitmap());
            //保存图片
            saveImage.Save(despath, ImageFormat.Jpeg);
            //释放资源   
            saveImage.Dispose();
            graphic.Dispose();
            bitmap.Dispose();
        }

        private void button_captureimg_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog()!=DialogResult.OK)
                {
                    return;
                }

                if (string.IsNullOrEmpty(textBox_x0.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox_y0.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox_w.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox_h.Text.Trim()))
                {
                    MessageBox.Show("请先将信息填写完整");
                    return;
                }


                int x0, y0, w, h, max_x, max_y;

                x0 = int.Parse(textBox_x0.Text.Trim());
                y0 = int.Parse(textBox_y0.Text.Trim());
                w = int.Parse(textBox_w.Text.Trim());
                h = int.Parse(textBox_h.Text.Trim());
                max_x = int.Parse(label_w.Text.Trim());
                max_y = int.Parse(label_h.Text.Trim());
                if (x0 < 0 || y0 < 0 || w < 0 || h < 0 ||
                    x0 > max_x || y0 > max_y || w > max_x || h > max_y ||
                    x0 + w > max_x || y0 + h > max_y)
                {
                    MessageBox.Show("信息不合法，请重新填写");
                    return;
                }

                SaveImage(textBox1.Text.Trim(), sfd.FileName, x0, y0, w, h);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      
    }
}
