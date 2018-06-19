using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using DevComponents.DotNetBar;

namespace LocalServer
{
    public partial class Form2 : Office2007Form
    {
        private bool OpenPort()
        {
            try
            {
                if (sp.IsOpen)
                {
                    sp.Close();
                }

                sp.PortName = m_strComPort;
                sp.BaudRate = m_iBaudRate;

                sp.DataBits = 8;
                sp.StopBits = StopBits.One;
                sp.Parity = Parity.None;
                sp.ReceivedBytesThreshold = 1;
                sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

                sp.Open();
                if (sp.IsOpen)
                {
                    Log("打开串口成功！");
                    return true;
                }
                else
                {
                    Log("打开串口失败");
                }
            }
            catch (Exception ex)
            {
                Log("打开串口异常:");
                Log(ex.Message);
            }

            return false;
        }

        private void WritePort(string str)
        {
            try
            {
                lock (m_objTxLock)
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                        if (!sp.IsOpen)
                        {
                            Log("串口发送失败： 串口未打开");
                        }
                    }

                    sp.Write(str);
                }
                Log(DateTime.Now + ", Tx: " + str);
            }
            catch (Exception ex)
            {
                Log("串口发送异常：" + ex.Message);
            }
        }

        private void ClosePort()
        {            
            try
            {
                if (sp.IsOpen)
                {
                    sp.Close();
                }
                Log("串口正常关闭");
            }
            catch (Exception ex)
            {
                Log("关闭串口异常");
                Log(ex.Message);
            }
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesCanRead = sp.BytesToRead;
                byte[] byte_rec = new byte[bytesCanRead];
                string str = string.Empty;

                if (bytesCanRead > 0)
                {
                    sp.Read(byte_rec, 0, bytesCanRead);
                    str = Encoding.Default.GetString(byte_rec, 0, bytesCanRead);
                    Log(DateTime.Now.ToString() + ", Rx：" + str);
                    lock (m_objComLock)
                    {
                        m_strCmd += str;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("串口接收异常：" + ex.Message);
            }
        }

    }
}
