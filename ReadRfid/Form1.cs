using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.IO.Ports;
using System.Threading;

namespace ReadRfid
{
    public partial class Form1 : Office2007Form
    {
        private SerialPort s1 = new SerialPort();
        private SerialPort s2 = new SerialPort();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "COM1";
            textBox2.Text = "115200";

            textBox3.Text = "COM2";
            textBox4.Text = "115200";

            richTextBox1.Clear();

            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(Run);

            t.Start();            
        }

        private void button2_Click(object sender, EventArgs e)
        {
        
        }


        private bool InitComPort()
        {
            try
            {
                if (string.IsNullOrEmpty(textBox1.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox2.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox3.Text.Trim()) ||
                    string.IsNullOrEmpty(textBox4.Text.Trim()))
                {
                    Log("请将串口信息填写完整");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return false;
            }
        }


        private void Run()
        {

        }


        private void Log(string str)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new EventHandler(delegate
                    {
                        richTextBox1.AppendText(str + "\r\n");
                    }));
            }
        }


    }
}
