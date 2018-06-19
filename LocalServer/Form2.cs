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
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace LocalServer
{
    public partial class Form2 : Office2007Form
    {
        private string bmpfile = string.Empty;

        //截屏参数
        //private Bitmap bmp = null;
        //private Graphics g = null;
        
        //串口参数
        private SerialPort sp = new SerialPort();
        private Object m_objComLock = new object();  //接收到信息修改的缓存锁，全局变量共享
        private Object m_objTxLock = new object();  //串口发送锁，单资源多个线程共享
        private string m_strComPort = string.Empty;
        private int m_iBaudRate = 115200;
        private string m_strCmd = string.Empty;
        private string m_strOneCmd = string.Empty;

        //MAC号码
        private string m_strMac = string.Empty;
        private string m_strMac2 = string.Empty;

        //当前Panel标号
        private int m_iNo = -1;

        //运行标志
        private bool m_bRunFlag = false;

        //运行状态
        private enum TestState { INIT, READY, RUN, PASS, FAIL, ABORT };
        private TestState m_RunStatus = TestState.INIT;

        //启动测试信号量
        //开始启动信号量，启动完成信号量，启动失败信号量，检查是否测试完成信号量
        private AutoResetEvent m_EventStart = new AutoResetEvent(false);
        private AutoResetEvent m_EventStartDone = new AutoResetEvent(false); //启动测试成功信号量
        private AutoResetEvent m_EventStartFail = new AutoResetEvent(false); //启动测试失败信号量
        private AutoResetEvent m_EventStartWait = new AutoResetEvent(false);//ATE处于测试中，需要重发
        private AutoResetEvent m_EventStartCheck = new AutoResetEvent(false);//设置开始检测ATE测试完成的标志位

        //加速信号量
        //private AutoResetEvent m_EventSpeedUp = new AutoResetEvent(false);

        //测试Reset按钮
        private AutoResetEvent m_EventResetButton = new AutoResetEvent(false); //Reset按钮测试
        private AutoResetEvent m_EventResetDone = new AutoResetEvent(false); //Reset按钮已经按下


        //private AutoResetEvent m_EventCheck = new AutoResetEvent(false);
        private AutoResetEvent m_EventCheckDone = new AutoResetEvent(false);

        private static object m_objDll = new object();

        #region 鼠标事件

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 通过句柄，窗体显示函数
        /// </summary>
        /// <param name="hWnd">窗体句柄</param>
        /// <param name="cmdShow">显示方式</param>
        /// <returns>返工成功与否</returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindowAsync", SetLastError = true)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("user32")]
        public extern static void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);
        const int MOUSEEVENTF_MOVE = 0x0001;//移动鼠标 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;//模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTUP = 0x0004;//模拟鼠标左键抬起
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        #endregion

        //存储按钮和输入框中心点坐标
        private Postion pos = new Postion();

        //反射
        private Assembly assembly = null;
        private Type assemblyType = null;
        private Object obj = null;
        private MethodInfo method = null;
        private Dictionary<string, MethodInfo> m_dicMethod = new Dictionary<string, MethodInfo>();

        public Form2()
        {
            InitializeComponent();            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
            //        Screen.PrimaryScreen.Bounds.Height);
            //g = Graphics.FromImage(bmp);
        }

        #region 代码作用：创建窗体时的输入参数
        //设置序号
        public void SetNo(int no)
        {
            m_iNo = no;
            bmpfile = Application.StartupPath + @"\capture" + m_iNo.ToString() + ".bmp";
            label1.Text = no.ToString() + ". ";
        }
        //设置背景色
        public void SetBkColor(Color cl)
        {
            label1.BackColor = cl;
        }
        //设置波特率
        public void SetComPort(string comport, int baudrate)
        {
            m_strComPort = comport;
            m_iBaudRate = baudrate;
            label1.Text += comport + ", " + baudrate.ToString();
        }
        //设置点击坐标
        public void SetButtonXY(int x, int y)
        {
            pos.button_x = x;
            pos.button_y = y;
            label1.Text += "\r\n(" + x.ToString() + ", " + y.ToString() + ")  ";
        }
        //设置输入坐标
        public void SetInputXY(int x, int y)
        {
            pos.input_x = x;
            pos.input_y = y;
            label1.Text += "(" + x.ToString() + ", " + y.ToString() + ")";
        }
        #endregion

        //加载，检查dll, 
        public bool LoadDLL()
        {
            try
            {
                m_dicMethod.Clear();
                lock (m_objDll)
                {
                    assembly = Assembly.LoadFile(Application.StartupPath + @"\" + MyData.halconDLLName + ".dll");
                    if (assembly == null)
                    {
                        Log("加载DLL失败：" + MyData.halconDLLName + ".dll");
                        return false;
                    }

                    obj = assembly.CreateInstance(MyData.halconDLLName + "." + MyData.dllclass);
                    if (obj == null)
                    {
                        Log("Class实例化失败：" + MyData.halconDLLName + "." + MyData.dllclass);
                        return false;
                    }

                    assemblyType = assembly.GetType(MyData.halconDLLName + "." + MyData.dllclass);
                    if (assemblyType == null)
                    {
                        Log("获取Type失败：" + MyData.halconDLLName + "." + MyData.dllclass);
                        return false;
                    }
                    m_dicMethod.Add(MyData.halconCheckModuleName, method);

                    //加载Halcon_CheckModule，并检查
                    method = assemblyType.GetMethod(MyData.halconCheckModuleName);
                    if (method == null)
                    {
                        Log("加载函数" + MyData.halconCheckModuleName + "失败");
                        return false;
                    }
                    object[] parm = new object[1];
                    parm[0] = Application.StartupPath + @"\model\" + MyData.dllclass.ToLower() + @"\" + MyData.pixel + @"\";
                    int a = (int)method.Invoke(obj, parm);
                    if (a != 0)
                    {
                        Log("模板检查失败，返回：" + a.ToString());
                        return false;
                    }
                    Log("模板检查成功!");                    

                    //加载Halcon_CheckImageStatus函数
                    method = assemblyType.GetMethod(MyData.halconCheckImageStatusName);
                    if (method == null)
                    {
                        Log("加载函数" + MyData.halconCheckImageStatusName + "失败");
                        return false;
                    }
                    m_dicMethod.Add(MyData.halconCheckImageStatusName, method);
                        
                    if (MyData.dllclass.Equals("Reset"))
                    {
                        //加载Halcon_CheckLightStatus函数
                        method = assemblyType.GetMethod(MyData.halconCheckLightStatusName);
                        if (method == null)
                        {
                            Log("加载函数" + MyData.halconCheckLightStatusName + "失败");
                            return false;
                        }
                        m_dicMethod.Add(MyData.halconCheckLightStatusName, method);

                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return false;
            }
        }

        //检查状态
        public bool CheckReady()
        {
            Thread t = new Thread(DoCheck);
            t.Start();

            return true;
        }

        private void DoCheck()
        {
            int r = 0;
            bool result = true;

            Log("检查DLL的合法性");
            if (!LoadDLL())
            {
                Log("不合法");
                ShowStatus(TestState.ABORT);
                result =  false;
            }

            if (result)
            {
                Log("检查ATE软件的初始态");
                r = GetImageStatus();
                if (r != (int)(UIState.INIT) &&
                    r != (int)(UIState.PASS) &&
                    r != (int)(UIState.FAIL))
                {
                    Log("状态异常");
                    ShowStatus(TestState.ABORT);
                    result = false;
                }
                else
                {
                    if (r == (int)(UIState.INIT))
                    {
                        m_RunStatus = TestState.INIT;
                    }
                    else if (r == (int)(UIState.PASS))
                    {
                        m_RunStatus = TestState.PASS;
                    }
                    else if (r == (int)(UIState.FAIL))
                    {
                        m_RunStatus = TestState.FAIL;
                    }
                }
            }

            if (result)
            {
                if (MyData.communicateMode == 1)
                {
                    Log("检查TCPIP服务器状态");
                    if (!ConnectTcp())
                    {
                        Log("连接TCPIP服务器失败");
                        ShowStatus(TestState.ABORT);
                        result = false;
                    }

                    DisconnectTcp();
                }
                else
                {
                    Log("检查串口状态");
                    if (!OpenPort())
                    {
                        Log("失败");
                        ShowStatus(TestState.ABORT);
                        result = false;
                    }

                    ClosePort();
                }
            }

            if (result)
            {
                m_EventCheckDone.Set();
            }
        }

        //启动线程
        public void Start()
        {
            m_RunStatus = TestState.INIT;
            m_bRunFlag = true;

            m_EventStart.Reset();
            m_EventStartDone.Reset();
            m_EventStartFail.Reset();
            m_EventStartWait.Reset();
            m_EventStartCheck.Reset();
            m_EventResetButton.Reset();
            m_EventResetDone.Reset();

            //m_EventSpeedUp.Reset();

            m_EventTcpConnect.Reset();
            m_EventTcpSend.Reset();

            Thread t1 = new Thread(MonitorScreen);
            t1.Start();

            if (MyData.communicateMode == 1)
            {
                Thread t2 = new Thread(ProcessTCP);
                t2.Start();
            }
            else
            {
                Thread t3 = new Thread(ProcessCom);
                t3.Start();
            }

            //if (MyData.SpeedUpEnable==1)
            //{
            //    Thread t4 = new Thread(SpeedUpThread);
            //    t4.Start();
            //}
        }

        //停止线程
        public void Stop()
        {
            if (m_bRunFlag == true)
            {
                m_bRunFlag = false;
                m_EventStart.Set();
                m_EventStartDone.Set();
                m_EventStartCheck.Set();
                m_EventStartFail.Set();
                m_EventStartWait.Set();
                m_EventResetButton.Set();
                m_EventResetDone.Set();

                //m_EventSpeedUp.Set();

                m_EventTcpConnect.Set();
                m_EventTcpSend.Set();

            }
        }

        private void ShowStatus(TestState ts)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new EventHandler(delegate
                    {
                        if (ts==TestState.INIT ||
                            ts==TestState.READY)
                        {
                            richTextBox1.BackColor = Color.White;
                        }
                        else if (ts == TestState.RUN)
                        {
                            richTextBox1.BackColor = Color.Yellow;
                        }
                        else if (ts == TestState.PASS)
                        {
                            richTextBox1.BackColor = Color.Lime;
                        }
                        else if (ts == TestState.FAIL)
                        {
                            richTextBox1.BackColor = Color.Red;
                        }
                        else if (ts == TestState.ABORT)
                        {
                            richTextBox1.BackColor = Color.Red;
                        }
                    }));
            }
        }

        private void Log(string str)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new EventHandler(delegate
                    {
                        richTextBox1.AppendText(str + "\r\n");
                        richTextBox1.ScrollToCaret();
                        System.Diagnostics.Trace.WriteLine("[DEBUG]:" + str);

                        //创建文件夹
                        string dir = Application.StartupPath + "\\log";
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        DateTime dt = DateTime.Now;
                        string filename = dir + "\\" + 
                                string.Format("{0,0:D4}_{1,0:D2}_{2,0:D2}_", dt.Year, dt.Month, dt.Day) + m_iNo + ".txt";                       
                        File.AppendAllText(filename, str + "\r\n", Encoding.Default);                       

                    }));
            }
        }

        private void SaveLog()
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new EventHandler(delegate
                {
                    string filename = string.Empty;
                    DateTime dt = DateTime.Now;

                    if (!System.IO.Directory.Exists(Application.StartupPath + @"\log"))
                    {
                        System.IO.Directory.CreateDirectory(Application.StartupPath + @"\log");
                    }

                    filename = Application.StartupPath + @"\log\" + 
                                string.Format("{0,0:D4}{1,0:D2}{2,0:D2}_{3,0:D2}{4,0:D2}{5,0:D2}.txt", 
                                                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

                    richTextBox1.SaveFile(filename, RichTextBoxStreamType.PlainText);
                    richTextBox1.Clear();
                }));
            }
        }
    }
}
