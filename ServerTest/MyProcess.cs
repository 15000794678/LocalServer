using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.IO;

namespace ServerTest
{
    public partial class Form1 : Office2007Form
    {
        private bool m_bRunFlag = false; //程序开始运行标志
        private StationResult sr = new StationResult();

        private enum RUN_STATUS { RUN, PASS, FAIL, ABORT };

        //private object m_lockDeviceParm = new object();
        //private Dictionary<string, DeviceParam> m_dicDeviceParam = new Dictionary<string, DeviceParam>();

        private int testnum = 0;
        private int randomdate = 0;

        private void ProcessThread()
        {
            try
            {
                //ShowStatus(RUN_STATUS.RUN);

                while (m_bRunFlag)
                {
                    lock (m_lockDeviceParm)
                    {
                        foreach (KeyValuePair<string, DeviceParam> kv in m_dicDeviceParam)
                        {
                            while (m_bRunFlag)
                            {
                                string cmd = string.Empty;
                                //lock (kv.Value.obj)
                                {
                                    if (!GetOneCmd(kv.Value, out cmd))
                                    {
                                        break;
                                    }
                                    ProcessOneCmd(kv, cmd);
                                }
                            }
                        }
                    }

                    Thread.Sleep(50);
                }

                if (!m_bRunFlag)
                {
                    //ShowStatus(RUN_STATUS.ABORT);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("process出错：" + ex.Message);

                //stw.Stop();
                //timer.Stop();

                //m_bRunFlag = false;
                //m_EventStart.Set();
                //m_EventSn.Set();
                //m_EventSnReady.Set();
                MessageBox.Show("process出错：" + ex.Message);
            }
        }

        private bool GetOneCmd(DeviceParam lp, out string cmd)
        {
            int start = 0;
            int stop = 0;

            cmd = string.Empty;
            if (string.IsNullOrEmpty(lp.cmd))
            {
                return false;
            }

            start = lp.cmd.IndexOf("*");
            stop = lp.cmd.IndexOf("#");
            if (start < 0) //没有包头
            {
                lp.cmd = string.Empty;//清空掉无效数据      
                return false;
            }
            if (stop < 0) //没有包尾
            {
                return false;
            }
            if (start > stop)
            {
                lp.cmd = lp.cmd.Substring(lp.cmd.IndexOf("*") + 1);
                return false;
            }

            cmd = lp.cmd.Substring(start, stop - start + 1);
            lp.cmd = lp.cmd.Substring(stop + 1);
            while (cmd.Substring(1).IndexOf("*") > -1) //若包中包含两个或以上的*
            {
                cmd = cmd.Substring(cmd.Substring(1).IndexOf("*"));
            }

            //根据实际情况来看, 后面的数据都是重复此帧
            lp.cmd = string.Empty;

            return true;
        }

        private bool ProcessOneCmd(KeyValuePair<string, DeviceParam> kv, string cmd)
        {
            string str = string.Empty;
            string response = string.Empty;

            Logout("[" + kv.Key + "]：Rx," + cmd);
            if (cmd.Contains("*TEST:MAC"))
            {//注册ID信息
                if (testnum == 0)
                {
                    response = "SN:" + randomdate.ToString();
                    randomdate++;
                    if (randomdate == 100)
                    {
                        randomdate = 0;
                    }
                    testnum = 1;
                }
                else if (testnum == 1)
                {
                    response = "127.0.0.1";
                    testnum = 0;
                }
            }
            else if (cmd.Contains("*RESULT:"))
            {//进站后询问上一次的测试结果信息，
                response = "*OUTPUT#";
            }
            else
            {
                response = "*Unknown#";
            }

            Logout("[" + kv.Key + "]：Tx," + response);
            return SocketSend(kv.Value.sc, response);
        }
    }
}
