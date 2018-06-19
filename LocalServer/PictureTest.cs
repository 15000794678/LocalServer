using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Reflection;
using System.IO;
using System.Threading;

namespace LocalServer
{
    public partial class PictureTest : Office2007Form
    {
        public PictureTest()
        {
            InitializeComponent();
        }

        private void button_select_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Application.StartupPath;
            ofd.Filter = "BMP files(*.bmp)|*.bmp|bmp files(*.bmp)|*.bmp|All files(*.*)|*.*"; ;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            textBox1.Text = ofd.FileName;

            pictureBox1.ImageLocation = ofd.FileName;
            TestPicture(ofd.FileName);
        }

        private void TestPicture(string filename)
        {
            Assembly assembly = null;
            Type assemblyType = null;
            Object obj = null;
            MethodInfo method = null;
            try
            {
                assembly = Assembly.LoadFile(Application.StartupPath + @"\" + MyData.halconDLLName + ".dll");
                if (assembly == null)
                {
                    MessageBox.Show("加载DLL失败：" + MyData.halconDLLName + ".dll");
                    return;
                }

                obj = assembly.CreateInstance(MyData.halconDLLName + "." + MyData.dllclass);
                if (obj == null)
                {
                    MessageBox.Show("Class实例化失败：" + MyData.halconDLLName + "." + MyData.dllclass);
                    return;
                }

                assemblyType = assembly.GetType(MyData.halconDLLName + "." + MyData.dllclass);
                if (assemblyType == null)
                {
                    MessageBox.Show("获取Type失败：" + MyData.halconDLLName + "." + MyData.dllclass);
                    return;
                }

                //加载Halcon_CheckModule，并检查
                method = assemblyType.GetMethod(MyData.halconCheckModuleName);
                if (method == null)
                {
                    MessageBox.Show("加载函数" + MyData.halconCheckModuleName + "失败");
                    return;
                }
                object[] parm = new object[1];
                parm[0] = Application.StartupPath + @"\model\" + MyData.dllclass.ToLower() + @"\" + MyData.pixel + @"\";
                int a = (int)method.Invoke(obj, parm);
                if (a != 0)
                {
                    MessageBox.Show("模板检查失败，返回：" + a.ToString());
                    return;
                }
                //MessageBox.Show("模板检查成功!");

                //加载Halcon_CheckImageStatus函数
                method = assemblyType.GetMethod(MyData.halconCheckImageStatusName);
                if (method == null)
                {
                    MessageBox.Show("加载函数" + MyData.halconCheckImageStatusName + "失败");
                    return;
                }

                //调用函数
                //object[] parm = new object[1];
                parm[0] = filename;

                //判断是否处于结束态， 调用HALCON函数
                int[] res = (int[])(method.Invoke(obj, parm));
                //MessageBox.Show("返回：[" + res[0].ToString() + ", " + res[1].ToString() + ", " + res[2].ToString() + "]");
                label2.BackColor = Color.Red;
                label2.Text = "返回：[" + res[0].ToString() + ", " + res[1].ToString() + ", " + res[2].ToString() + "]";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_capture_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            
            Thread t = new Thread(Run);
            t.Start();
        }

        private void Run()
        {           
            DateTime dt = DateTime.Now;
            string filename = Application.StartupPath + @"\" +
            //                  dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString() + "_" + 
            //                  dt.Hour.ToString() + dt.Minute.ToString() + dt.Second.ToString() + ".bmp";
                              string.Format("{0,0:D4}{1,0:D2}{2,0:D2}_{3,0:D2}{4,0:D2}{5,0:D2}.bmp", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            Thread.Sleep(300);
            if (!PrintImage.PrintBitImage(filename))
            {               
                MessageBox.Show("截图失败");
                return;
            }

            if (File.Exists(filename))
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke(new EventHandler(delegate
                        {
                            //this.WindowState = FormWindowState.Normal;
                            pictureBox1.ImageLocation = filename;
                            textBox_capture.Text = filename;
                        }));
                }
            }
            else
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke(new EventHandler(delegate
                    {
                        //this.WindowState = FormWindowState.Normal;
                        pictureBox1.ImageLocation = null;
                        textBox_capture.Text = filename;
                    }));
                }
            }

            Application.DoEvents();
        }

        private void button_rename_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_capture.Text.Trim()))
            {
                MessageBox.Show("要重命名的文件不存在");
                return;
            }
            if (!File.Exists(textBox_capture.Text.Trim()))
            {
                MessageBox.Show("要重命名的文件不存在");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Application.StartupPath;
            sfd.Filter = "BMP files(*.bmp)|*.bmp|bmp files(*.bmp)|*.bmp|All files(*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            FileInfo fi = new FileInfo(textBox_capture.Text.Trim());
            fi.MoveTo(sfd.FileName);

            textBox_capture.Text = sfd.FileName;
        }
    }
}
