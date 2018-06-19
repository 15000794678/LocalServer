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
using System.Threading;


namespace LocalServer
{
    public partial class TestSet : Office2007Form
    {
        private string[] m_tableTitle = new string[4] {"按钮坐标1",
                                                       "输入坐标1",
                                                       "按钮坐标2",
                                                       "输入坐标2"};
        private Thread t = null;

        public TestSet()
        {
            InitializeComponent();           
        }

        private void TestSet_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = MyData.startMode == 1 ? true : false;
            radioButton2.Checked = !radioButton1.Checked;

            radioButton3.Checked = MyData.communicateMode == 1 ? true : false;
            radioButton4.Checked = !radioButton3.Checked;

            textBox_ip.Text = MyData.tcp_ip;
            textBox_port.Text = MyData.tcp_port.ToString();

            comboBox_num.SelectedIndex = MyData.num - 1;

            dataGridView1.RowCount = MyData.num;
            dataGridView1.ClearSelection();
            for (int i = 0; i < MyData.num; i++)
            {
                if (MyData.dic_port.ContainsKey(i + 1))
                {
                    dataGridView1[0, i].Value = (i + 1).ToString();
                    dataGridView1[1, i].Value = MyData.dic_port[i + 1].portname;
                    dataGridView1[2, i].Value = MyData.dic_port[i + 1].baudrate.ToString();
                }
            }

            textBox_dllfilepath.Text = Application.StartupPath + @"\" + MyData.halconDLLName + ".dll";            

            dataGridView2.RowCount = MyData.num * 2;
            dataGridView2.ClearSelection();
            for (int i = 0; i < MyData.num; i++)
            {                
                dataGridView2[0, i * 2].Value = m_tableTitle[i * 2];
                dataGridView2[1, i * 2].Value = MyData.dic_xy[i + 1].button_x.ToString();
                dataGridView2[2, i * 2].Value = MyData.dic_xy[i + 1].button_y.ToString();
                dataGridView2[0, i * 2 + 1].Value = m_tableTitle[i * 2 + 1];
                dataGridView2[1, i * 2 + 1].Value = MyData.dic_xy[i + 1].input_x.ToString();
                dataGridView2[2, i * 2 + 1].Value = MyData.dic_xy[i + 1].input_y.ToString();  
            }


            //刷新库
            List<string> listclass = FindClass(textBox_dllfilepath.Text);

            comboBox_dllclass.SelectedIndex = -1;
            if (listclass.Count > 0)
            {
                comboBox_dllclass.Items.Clear();
                foreach (string str in listclass)
                {
                    comboBox_dllclass.Items.Add(str);                    
                }
                for (int i = 0; i < comboBox_dllclass.Items.Count; i++)
                {
                    if (comboBox_dllclass.Items[i].ToString().Equals(MyData.dllclass))
                    {
                        comboBox_dllclass.SelectedIndex = i;
                    }
                }
            }

            //开始鼠标任务
            t = new Thread(ShowMousePostion);
            t.Start();
        }

        private void comboBox_num_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.RowCount = comboBox_num.SelectedIndex + 1;

            for (int i = 0; i < comboBox_num.SelectedIndex + 1; i++)
            {
                dataGridView1[0, i].Value = (i + 1).ToString();
                //dataGridView1[1, i].Value = "COM" + (i+1).ToString();
                //dataGridView1[2, i].Value = "115200";
            }

            dataGridView2.RowCount = (comboBox_num.SelectedIndex + 1)*2;
            for (int i = 0; i < (comboBox_num.SelectedIndex + 1) * 2; i++)
            {                
                dataGridView2[0, i].Value = m_tableTitle[i];
                //dataGridView2[1, i].Value = "0";
                //dataGridView2[2, i].Value = "0";                
            }            

            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            if (comboBox_num.SelectedIndex < 0)
            {
                MessageBox.Show("请先设置并行测试数量");
                return;
            }

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1[0, i].Value==null || 
                    string.IsNullOrEmpty(dataGridView1[0, i].Value.ToString()) ||
                    dataGridView1[1, i].Value==null ||
                    string.IsNullOrEmpty(dataGridView1[1, i].Value.ToString()) ||
                    dataGridView1[2, i].Value==null ||
                    string.IsNullOrEmpty(dataGridView1[2, i].Value.ToString()))
                {
                    MessageBox.Show("请先将串口配置信息填写完整!");
                    return;
                }
            }

            if (string.IsNullOrEmpty(textBox_dllfilepath.Text.Trim()) ||
                comboBox_dllclass.SelectedIndex < 0 )
            {
                MessageBox.Show("请先将图像配置信息填写完整!");
                return;
            }

            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                if (dataGridView2[0, i].Value==null ||
                    string.IsNullOrEmpty(dataGridView2[0, i].Value.ToString()) ||
                    dataGridView2[1, i].Value == null ||
                    string.IsNullOrEmpty(dataGridView2[1, i].Value.ToString()) ||
                    dataGridView2[2, i].Value == null ||
                    string.IsNullOrEmpty(dataGridView2[2, i].Value.ToString()))
                {
                    MessageBox.Show("请先将图像配置信息填写完整!");
                    return;
                }
            }

            if (string.IsNullOrEmpty(textBox_ip.Text.Trim()) ||
                string.IsNullOrEmpty(textBox_port.Text.Trim()))
            {
                MessageBox.Show("请先将TCP/IP通信的IP地址和端口号填写完整!");
                return;
            }

            //检查所选择的dll信息是否完整
            if (!CheckDLLValid(textBox_dllfilepath.Text.Trim(), 
                               comboBox_dllclass.Items[comboBox_dllclass.SelectedIndex].ToString().Trim()))
            {
                MessageBox.Show("图像配置：选择的库文件信息不完整，请重新选择库文件！");
                return;
            }

            MyData.startMode = radioButton1.Checked ? 1 : 2;
            MyData.communicateMode = radioButton3.Checked ? 1 : 2;
            MyData.tcp_ip = textBox_ip.Text.Trim();
            MyData.tcp_port = int.Parse(textBox_port.Text.Trim());                 

            MyData.num = comboBox_num.SelectedIndex + 1;
            MyData.dic_port.Clear();
            for (int i=1; i<=MyData.num; i++)
            {
                PortStruct ps = new PortStruct();
                ps.portname = dataGridView1[1, i - 1].Value.ToString();
                ps.baudrate = int.Parse(dataGridView1[2, i-1].Value.ToString());
                MyData.dic_port.Add(i, ps);
            }

            MyData.dllfilepath = textBox_dllfilepath.Text.Trim();
            MyData.dllclass = comboBox_dllclass.Items[comboBox_dllclass.SelectedIndex].ToString();
            
            MyData.dic_xy.Clear();
            for (int i = 1; i <= MyData.num; i++)
            {
                Postion ps = new Postion();
                ps.button_x = int.Parse(dataGridView2[1, (i - 1)*2].Value.ToString());
                ps.button_y = int.Parse(dataGridView2[2, (i - 1) * 2].Value.ToString());
                ps.input_x = int.Parse(dataGridView2[1, (i - 1) * 2 + 1].Value.ToString());
                ps.input_y = int.Parse(dataGridView2[2, (i - 1) * 2 + 1].Value.ToString());
                MyData.dic_xy.Add(i, ps);
            }

            MyData.SaveCfg();

            MessageBox.Show("保存成功！");

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private List<string> FindClass(string filepath)
        {
            Assembly assembly = null;            

            assembly = Assembly.LoadFile(filepath);
            if (assembly == null)
            {
                return null;
            }

            List<string> listclass = new List<string>();
            foreach (Type tp in assembly.GetTypes())
            {
                if (tp.FullName.Contains(MyData.halconDLLName + "."))
                {
                    listclass.Add(tp.Name);
                }
            }

            return listclass;
        }

        private bool CheckDLLValid(string filepath, string classname)
        {
            Assembly assembly = null;
            Type assemblyType = null;
            Object obj = null;
            MethodInfo method = null;

            try
            {
                assembly = Assembly.LoadFile(filepath);
                if (assembly == null)
                {
                    return false;
                }

                obj = assembly.CreateInstance(MyData.halconDLLName + "." + classname);
                if (obj == null)
                {
                    MessageBox.Show("加载库文件：" + classname + "类实例化失败");
                    return false;
                }

                assemblyType = assembly.GetType(MyData.halconDLLName + "." + classname);
                if (assemblyType == null)
                {
                    MessageBox.Show("加载库文件：" + classname + "类获取类型失败");
                    return false;
                }

                //查找类中函数是否存在
                method = assemblyType.GetMethod(MyData.halconCheckModuleName);
                if (method == null)
                {
                    MessageBox.Show("加载库文件：函数" + MyData.halconCheckModuleName + "不存在");
                    return false;
                }

                method = assemblyType.GetMethod(MyData.halconCheckImageStatusName);
                if (method == null)
                {
                    MessageBox.Show("加载库文件：函数" + MyData.halconCheckImageStatusName + "不存在");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
                return false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            //groupBox_tcpip.Enabled = radioButton3.Checked;
            //groupBox_com.Enabled = !radioButton3.Checked;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            //groupBox_tcpip.Enabled = !radioButton4.Checked;
            //groupBox_com.Enabled = radioButton4.Checked;
        }

        private void ShowMousePostion()
        {
            Thread.Sleep(500);

            while (true)
            {
                Point pt = Control.MousePosition;
                if (this.IsHandleCreated)
                {
                    this.Invoke(new EventHandler(delegate
                        {
                            label_X.Text = "X:" +  pt.X.ToString();
                            label_Y.Text = "Y:" + pt.Y.ToString();
                        }));
                }
                Thread.Sleep(100);
            }

        }

        private void TestSet_FormClosed(object sender, FormClosedEventArgs e)
        {
            t.Abort();
        }
    }
}
