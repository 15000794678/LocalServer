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

namespace LocalServer
{
    public partial class Form2 : Office2007Form
    {
        private static object m_objLockKey = new object();

        private enum ImageState 
        { 
            NOPICTURE = 0, //未读到图
            MESSAGEBOX = 1, //有messagebox窗口，x，y为坐标
            MESSGEBOXERROR = 2, //messagebox窗口未知错误
            NOTFINDAREA = 3, //未找到定位区域（右测试按钮）
            UNKOWNERROR = 4, //未知错误
            FINDAREA = 5, //查询到测试窗体
            FINDLEDBOX = 6, //找到看灯窗体
            FINDRESETBOX = 7, //找到RESET窗体
            MAX
        };
        
        private enum UIState
        {
            INIT = 0, //窗体初始状态
            RUN = 1, //测试中
            PASS = 2, //测试成功
            FAIL = 3, //测试失败
            UNKNOWN = 4, //未知状态
            PAUSE = 5, //暂停状态
            ABORT = 6, //测试终止
            MAX
        };

        private enum LEDState
        {
            NOLED = 0,
            YELLOW = 1,
            RED = 2,
            BLUE = 3,
            GREEN = 4,
            NOCAM = 20,
            NOPICTURE = 21,
            UNDEFINECOLOR = 22,
            MAX
        };
        
        private int[] LEDShowStatus = new int[] {                         
                        (int)LEDState.RED,
                        (int)LEDState.BLUE,
                        (int)LEDState.YELLOW
        };
        private int LEDShowNo = 0;

        //模拟点击
        private void MonitorClick(int x, int y)
        {
            Log("点击事件：【" + x.ToString() + ", " + y.ToString() + "】");

            lock (m_objLockKey)
            {
                SetCursorPos(x, y);

                mouse_event((int)(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP), 0, 0, 0, IntPtr.Zero);                             
            }
        }

        //模拟输入
        private void MonitorInput(int x, int y, string str)
        {
            Log("输入事件：【" + x.ToString() + ", " + y.ToString() + "】=" + str);

            lock (m_objLockKey)
            {
                SetCursorPos(x, y);
               
                mouse_event((int)(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP), 0, 0, 0, IntPtr.Zero);
                Thread.Sleep(10);
                SendKeys.SendWait(str);
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(1000);
            }
        }

        //检查图像状态
        /*返回值为含三个值得int数组，定义如下：
            [0,0,0] 未读到图
            [1,x,y] 有messagebox窗口，x，y为坐标
            [2,2,2] messagebox窗口未知错误
            [3,3,3] 未找到定位区域（右测试按钮）
            [4,4,4] 未知错误
            [5,a,b] 查询到测试窗体，其中a和b的值只有0,1,2,3,4；
            当a=0表示上窗体初始状态，=1测试中，=2测试成功，=3测试失败，=4未知状态
            当b=0表示下窗体初始状态，=1测试中，=2测试成功，=3测试失败，=4未知状态
         */
        
        private int[] CheckImageStatus()
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    //截屏               
                    if (PrintImage.PrintBitImage(bmpfile))
                    {                        
                        break;
                    }
                    if (i == 4)
                    {
                        Log("截图失败");
                        return null;
                    }
                    Thread.Sleep(500);
                }

                //调用函数
                object[] parm = new object[1];
                parm[0] = bmpfile;
                
                lock (m_objDll)
                {
                    Log(DateTime.Now.ToString() + ", 处理图像开始");
                    //判断是否处于结束态， 调用HALCON函数
                    int[] res = (int[])(m_dicMethod[MyData.halconCheckImageStatusName].Invoke(obj, parm));
                
                    Log(DateTime.Now.ToString() + ", 处理图像结束");

                    Log("图像状态：【" + res[0].ToString() + ", " + res[1].ToString() + ", " + res[2].ToString() + "】");
                    return res;
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            return null;
        }

        private int GetImageStatus()
        {
            int[] r1 = new int[3]{-1,-1,-1};
            int i = 0;
            int cnt = 5;
            int time = 1000;

            while (true)
            {
                if (i == cnt)
                {
                    Log("截图超时");
                    return -1;
                }

                r1 = CheckImageStatus();
                if (r1 == null)
                {
                    i++;
                    Thread.Sleep(time);
                    continue;
                }

                if (r1[0] == (int)ImageState.MESSAGEBOX)  //有messagebox窗体
                {
                    i++;
                    MonitorClick(r1[2], r1[1]);
                    Thread.Sleep(200);
                    continue;
                }                
                else if (r1[0]==(int)ImageState.NOPICTURE ||  //0:未读到图
                        r1[0]==(int)ImageState.MESSGEBOXERROR || //2:messagebox窗口未知错误
                        r1[0]==(int)ImageState.NOTFINDAREA || //3:未找到定位区域（右测试按钮）
                        r1[0]==(int)ImageState.UNKOWNERROR)  //4:未知错误
                {
                    i++;
                    Thread.Sleep(time);
                    continue;
                }
                else if (r1[0] == (int)ImageState.FINDAREA) //5=找到测试窗体
                {
                    //以下是找到窗体的处理
                    int result = m_iNo == 1 ? r1[1] : r1[2];

                    if (result == (int)UIState.PAUSE || result == (int)UIState.ABORT)
                    {
                        MonitorClick(pos.button_x, pos.button_y);
                        Thread.Sleep(200);
                        continue;
                    }
                    else
                    {
                        return result;
                    }
                }
                else if (r1[0] == (int)ImageState.FINDLEDBOX)
                {
                    //最多看三次灯
                    UInt16[] passbutton = new UInt16[2];
                    UInt16[] failbutton = new UInt16[2];

                    passbutton[0] = (UInt16)(r1[1] >> 16);
                    passbutton[1] = (UInt16)(r1[2] >> 16);
                    failbutton[0] = (UInt16)(r1[1] & 0xFFFF);
                    failbutton[1] = (UInt16)(r1[2] & 0xFFFF);

                    if (passbutton[0] == 0 ||
                        passbutton[1] == 0 ||
                        failbutton[0] == 0 ||
                        failbutton[1] == 0)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    for (int num = 0; num < 3; num++)
                    {
                        //找到看灯窗体
                        if (CheckLightStatus((LEDState)LEDShowStatus[LEDShowNo]))
                        {
                            LEDShowNo++;
                            break;
                        }
                        else if (num == 2)
                        {
                            MonitorClick(failbutton[1], failbutton[0]);
                            Thread.Sleep(3000);
                            return (int)(UIState.RUN);  //从另一个界面失败
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                    MonitorClick(passbutton[1], passbutton[0]);
                    Thread.Sleep(3000);
                    continue;
                }
                else if (r1[0] == (int)ImageState.FINDRESETBOX)
                {
                    //设置按RESET按钮信号量
                    m_EventResetDone.Reset();

                    //设置RESET信号量
                    m_EventResetButton.Set();

                    //MessageBox.Show("Please Press the Reset Button!");
                    m_EventResetDone.WaitOne(5000);

                    Thread.Sleep(1000);
                }
            }
        }

        private bool CheckLightStatus(LEDState ls)
        {
            //调用函数
            
            lock (m_objDll)
            {
                Log(DateTime.Now.ToString() + ", 处理看灯开始, Thresthod=[" + MyData.r_thresthod.ToString() + "," + 
                                            MyData.b_thresthod.ToString() + "," + MyData.y_thresthod.ToString() + "]");
                //判断是否处于结束态， 调用HALCON函数
                object[] parm = new object[4];
                parm[0] = MyData.b_thresthod;
                parm[1] = MyData.r_thresthod;
                parm[2] = MyData.y_thresthod;
                parm[3] = 0;
                int res = (int)(m_dicMethod[MyData.halconCheckLightStatusName].Invoke(obj, parm));

                Log(DateTime.Now.ToString() + ", 处理看灯结束, 颜色为value=" + parm[3].ToString());                
                if (res != (int)ls)
                {
                    Log("灯状态为：【" + res.ToString() + "】, 看灯失败，要看灯的颜色为：" + ((int)ls).ToString());
                    return false;
                }

                Log("灯状态为：【" + res.ToString() + "】， 看灯成功");
                return true;                
            }
        }

        private bool StartTest()
        {
            int r1 = 0;            
            int cnt = 0;

            if (!m_bRunFlag) return false;

            //1.判断输入前是否处于停止状态
            #region 1.启动ATE前，检查ATE不处于测试中
            cnt = 0;
            while (m_bRunFlag)
            {
                r1 = GetImageStatus();
                if (r1 == (int)UIState.RUN)
                {
                    m_EventStartWait.Set();
                    return false;
                }
                else if (r1 == (int)UIState.INIT ||
                    r1 == (int)UIState.PASS ||
                    r1 == (int)UIState.FAIL)
                {
                    break;
                }
                else if (cnt == 5)
                {
                    Log("检查ATE状态失败！");
                    return false;
                }
                else
                {
                    cnt++;
                    Thread.Sleep(200);
                }
            }
            #endregion

            //2.输入或者按钮点击动作
            #region 2.模拟操作启动ATE
            if (MyData.startMode == 1)
            {
                MonitorInput(pos.input_x, pos.input_y, m_strMac);
                Thread.Sleep(100);
            }
            else
            {
                MonitorClick(pos.button_x, pos.button_y);
                Thread.Sleep(100); //点完开始按钮，等待5ms
            }
            LEDShowNo = 0;  //重置看灯顺序
            Thread.Sleep(2000);
            #endregion

            //3.判断是否处于运行态， 调用HALCON函数
            #region 3.检查ATE是否启动成功，处于测试中
            cnt = 0;
            while (m_bRunFlag)
            {
                r1 = GetImageStatus();
                if (r1 == (int)UIState.RUN)
                {                        
                    break;
                }
                else if (cnt == 15)
                {
                    m_RunStatus = TestState.FAIL;
                    m_EventStartFail.Set();
                    Log("图像状态：【启动测试超时失败】");
                    return false;
                }
                else
                {
                    cnt++;
                    Thread.Sleep(200);
                }
            }

            Log("图像状态：【启动测试成功】");
            m_RunStatus = TestState.RUN; 
            m_EventStartDone.Set(); //设置信号量
            #endregion 

            #region 4.检查ATE是否测试完成
            if (!m_EventStartCheck.WaitOne(5000))
            {
                Log("主现程检测到启动测试完成信号量失败，不进行图像检测！");
                m_RunStatus = TestState.FAIL;
                return false;
            }

            //3.等待结束，最好有超时设置，目前为三分钟                
            Log("图像状态：【测试中， 等待测完完毕】");
            cnt = 0; //遮挡超过5次，认为测试异常
            while (m_bRunFlag)
            {
                r1 = GetImageStatus();
                if (r1 == (int)UIState.RUN)
                {
                    cnt = 0;
                    Thread.Sleep(MyData.pictureDelayTime);//以前是500ms
                }
                else if (r1 == (int)UIState.PASS)
                {
                    cnt = 0;
                    Log("图像状态：【测试成功】"); 
                    m_RunStatus = TestState.PASS;
                    //m_EventSpeedUp.Set();
                    return true;
                }
                else if (r1 == (int)UIState.FAIL)
                {
                    cnt = 0;
                    Log("图像状态：【测试失败】"); 
                    m_RunStatus = TestState.FAIL;
                    //m_EventSpeedUp.Set();
                    return true;
                }
                else if (r1 == (int)UIState.INIT)
                {
                    cnt = 0;
                    Log("图像状态：【初始化】，重新检测");
                    Thread.Sleep(500);
                }
                else
                {
                    cnt++;
                    Thread.Sleep(500);
                    if (cnt == 6)
                    {
                        Log("图像状态：【测试异常】");
                        m_RunStatus = TestState.FAIL;
                        return false;
                    }
                }
            }
            #endregion
            

            return true;
        }

        //监视屏幕线程
        private void MonitorScreen()
        {            
            while (m_bRunFlag)
            {
                //等待启动测试信号量
                m_EventStart.WaitOne();

                StartTest();
            }
        }     
    }
}
