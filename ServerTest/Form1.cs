using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DevComponents.DotNetBar;

namespace ServerTest
{
    public partial class Form1 : Office2007Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_bRunFlag = true;

            //启动接收线程
            Thread thread_acceptsocket = new Thread(AcceptThread);
            thread_acceptsocket.Start();

            //启动处理线程
            Thread thread_process = new Thread(ProcessThread);
            thread_process.Start();

        }

        private void Logout(string str)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new EventHandler(delegate
                {
                    richTextBox1.Select(richTextBox1.TextLength, 0);
                    //滚动到控件光标处   
                    richTextBox1.ScrollToCaret();
                    richTextBox1.AppendText(str + "\r\n");

                    //打印到另外一个窗口
                    //FrmLogout.Logout(str + "\r\n");
                    System.Diagnostics.Trace.WriteLine("[DEBUG]" + str);
                }));
            }
        }


    }
}
