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
        private bool GetOneCmd()
        {            
            lock (m_objComLock)
            {
                if (m_strCmd.IndexOf("*") < 0)
                {
                    m_strCmd = string.Empty;
                    return false;
                }
                if (m_strCmd.IndexOf("#") < 0)
                {
                    return false;
                }

                if (m_strCmd.IndexOf("*") > m_strCmd.IndexOf("#"))
                {
                    m_strCmd = m_strCmd.Substring(m_strCmd.IndexOf("*"));
                    return false;
                }

                //取出*#之间包含的字段
                string cmd = m_strCmd.Substring(m_strCmd.IndexOf("*"), m_strCmd.IndexOf("#") - m_strCmd.IndexOf("*") + 1);
                m_strCmd = m_strCmd.Substring(m_strCmd.IndexOf("#") + 1);

                while (cmd.Substring(1).IndexOf("*") > -1)
                {
                    cmd = cmd.Substring(cmd.Substring(1).IndexOf("*"));
                }

                m_strOneCmd = cmd;

                m_strCmd = string.Empty; //清空缓存
                return true;                
            }
        }

        private void ProcessData()
        {
            while (m_bRunFlag)
            {
                while (m_bRunFlag)
                {
                    if (GetOneCmd())
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }

                if (!m_bRunFlag) return;
                ProcessOneCmd();
            }
        }

        private void ProcessOneCmd()
        {
            Log(DateTime.Now + ", 处理命令：" + m_strOneCmd);
            if (m_strOneCmd.Contains("*StartTest:"))
            {
                m_strMac = m_strOneCmd.Replace("*StartTest:", "").Replace("#", "").Trim();//记录本次正常测试的MAC

                if (string.IsNullOrEmpty(m_strMac))
                {
                    Log("MAC为空");
                    WritePort(m_strOneCmd.Replace("#", "") + ",FAIL#");
                    return;
                }

                //当前不处于运行态
                if (m_RunStatus != TestState.RUN)
                {
                    //重置信号量
                    m_EventStart.Reset();
                    m_EventStartDone.Reset();
                    m_EventStartFail.Reset();
                    m_EventStartWait.Reset();

                    //设置开始测试信号量
                    m_EventStart.Set();

                    //测试信号量时间内是否完成
                    bool res = false;
                    int i=0, cnt=10;
                    while (m_bRunFlag)
                    {
                        if (m_EventStartDone.WaitOne(1))
                        {
                            res = true; //启动成功
                            break;
                        }
                        else if (m_EventStartFail.WaitOne(1))
                        {
                            break;      //启动失败
                        }
                        else if (m_EventStartWait.WaitOne(1))
                        {//ATE处于不受控的测试中，设备需要等待                            
                            WritePort(m_strOneCmd.Replace("#", "") + ",WAIT#");
                            return;
                        }
                        else if (i == cnt)
                        {
                            break;      //启动超时
                        }
                        else
                        {
                            i++;
                            Thread.Sleep(3000);
                        }
                    }

                    if (res)
                    {
                        //SaveLog();
                        m_EventStartCheck.Set();
                        Log("等待测试信号量完成，成功!");
                        WritePort(m_strOneCmd.Replace("#", "") + ",OK#");
                        m_strMac2 = m_strMac; //保存上次启动成功的mac
                    }
                    else
                    {
                        Log("等待测试信号量完成，失败!");
                        WritePort(m_strOneCmd.Replace("#", "") + ",FAIL#");
                    }
                }
                else  //处于测试态
                {
                    //同样的MAC号码，再次发送启动测试，已在测试中，直接回复OK
                    if (m_strOneCmd.Contains(m_strMac2))
                    {
                        WritePort(m_strOneCmd.Replace("#", "") + ",OK#");
                    }
                    else
                    {
                        WritePort(m_strOneCmd.Replace("#", "") + ",FAIL#");
                    }
                }
            }
            else if (m_strOneCmd.Contains("*TestDone"))
            {
                bool outStation = (m_iNo == 1 ? MyData.bLeftOut : MyData.bRightOut);

                if (m_RunStatus == TestState.RUN)
                {
                    Thread.Sleep(MyData.comDelayTime);
                }

                if (m_RunStatus == TestState.PASS)
                {
                    WritePort(m_strOneCmd.Replace("?#", "") + ":Pass#");
                }
                else if (m_RunStatus == TestState.FAIL)
                {
                    if (!outStation)//如果不出站
                    {
                        WritePort(m_strOneCmd.Replace("?#", "") + ":Wait#");
                    }
                    else
                    {
                        WritePort(m_strOneCmd.Replace("?#", "") + ":Fail#");
                    }
                }
                else if (m_RunStatus == TestState.ABORT)
                {
                    if (!outStation)//如果不出站
                    {
                        WritePort(m_strOneCmd.Replace("?#", "") + ":Wait#");
                    }
                    else
                    {
                        WritePort(m_strOneCmd.Replace("?#", "") + ":Fail#");
                    }
                }
                else if (m_RunStatus == TestState.INIT)
                {
                    WritePort(m_strOneCmd.Replace("?#", "") + ":ReTest#");
                }
                else
                {
                    if (m_EventResetButton.WaitOne(1))
                    {
                        WritePort(m_strOneCmd.Replace("?#", "") + ":Reset#");
                        m_EventResetDone.Set();
                    }
                    else
                    {
                        WritePort(m_strOneCmd.Replace("?#", "") + ":Wait#");
                    }
                }
            }
            else
            {
                WritePort("*UndefineCmd#");
            }

            m_strOneCmd = string.Empty;
        }

        //线程
        private void ProcessCom()
        {
            if (!OpenPort())
            {
                return;
            }

            ShowStatus(TestState.RUN);
            while (m_bRunFlag)
            {
                ProcessData();
                Thread.Sleep(10);                
            }

            ShowStatus(TestState.ABORT);
            ClosePort();
        }


        //加速线程，在检测到图像测试完毕后主动发送信息给单片机
        //private void SpeedUpThread()
        //{
        //    while (m_bRunFlag)
        //    {
        //        m_EventSpeedUp.WaitOne();

        //        if (!m_bRunFlag) return;

        //        if (m_RunStatus == TestState.PASS)
        //        {
        //            WritePort("*TestDone:Pass#");
        //        }
        //        else if (m_RunStatus == TestState.FAIL)
        //        {
        //            WritePort("*TestDone:Fail#");
        //        }
        //    }
        //}
    }
}
