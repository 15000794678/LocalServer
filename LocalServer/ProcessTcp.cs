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
using System.Net;
using System.Net.Sockets;

namespace LocalServer
{
    public partial class Form2 : Office2007Form
    {
        //线程
        private void ProcessTCP()
        {
            if (!ConnectTcp())
            {
                return;
            }

            ShowStatus(TestState.RUN);
            while (m_bRunFlag)
            {
                AskMac();

                WaitTestDone();

                UploadResult();

                //SaveLog();
            }

            ShowStatus(TestState.ABORT);
            DisconnectTcp();
        }

        private void AskMac()
        {
            Log(DateTime.Now.ToString() + ", 询问MAC");
            string str = string.Empty;

            m_strTcpSend = "*TEST:MAC?:" + m_iNo.ToString() + "#";
            while (m_bRunFlag)
            {
                //先清掉缓存
                lock (m_strCmd)
                {
                    m_strCmd = string.Empty;
                }

                WriteTcp(m_strTcpSend);               

                int cnt = 0;
                while (m_bRunFlag)
                {
                    lock (m_objTcpLock)
                    {                       
                        if (m_strCmd.Contains("SN:"))
                        {
                            m_strMac = m_strCmd.Replace("SN:", "");                            
                            return;
                        }                        
                    }

                    if (cnt < 30)
                    {
                        cnt++;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }                    
                }
            }
        }

        private void WaitTestDone()
        {
            Log(DateTime.Now.ToString() + ", 等待测试完毕");
            //重置信号量
            m_EventStart.Reset();
            m_EventStartDone.Reset();
            m_EventStartFail.Reset();
            m_EventStartCheck.Reset();            

            //设置信号量
            m_EventStart.Set();
            
            //测试信号量时间内是否完成
            bool res = false;
            int i = 0, cnt = 300;
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
                else if (i == cnt)
                {
                    break;      //启动超时
                }
                else
                {
                    i++;
                    Thread.Sleep(100);
                }
            }

            if (!res)           
            {                         
                Log("等待测试信号量完成，失败!");                
                return;
            }            
             
            //SaveLog();
            m_EventStartCheck.Set();
            Log("等待测试信号量完成，成功!");
           
            Log("等待测试结束");
            m_EventStartCheck.Set();  //设置开始检测测试完成的标志位
            while (m_bRunFlag)
            {
                if (m_RunStatus == TestState.RUN)
                {
                    Thread.Sleep(10);
                }
                else
                {
                    break;
                }
            }
            Log("测试完成");
        }

        private void UploadResult()
        {
            Log(DateTime.Now.ToString() + ", 上传测试结果");
            string str = string.Empty;

            if (m_RunStatus == TestState.PASS)
            {
                m_strTcpSend = "*RESULT:PASS:" + m_iNo.ToString() + "#";
            }
            else if (m_RunStatus == TestState.FAIL)
            {
                m_strTcpSend = "*RESULT:FAIL:" + m_iNo.ToString() + "#";
            }
            else
            {
                m_strTcpSend = "*RESULT:FAIL:" + m_iNo.ToString() + "#";
            }

            while (m_bRunFlag)
            {
                //先清掉缓存
                lock (m_strCmd)
                {
                    m_strCmd = string.Empty;
                }
                WriteTcp(m_strTcpSend);
                
                //接收响应超时
                int cnt = 0;
                while (m_bRunFlag)
                {
                    lock (m_objTcpLock)
                    {
                        if (m_strCmd.Contains("*OUTPUT#"))
                        {                            
                            return;
                        }
                    }

                    if (cnt < 30)
                    {
                        cnt++;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

    }
}
