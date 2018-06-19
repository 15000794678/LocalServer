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
        private Socket sc = null;
        private IPAddress ip = null;
        private IPEndPoint iep = null;

        private object m_objTcpLock = new object();
        private AutoResetEvent m_EventTcpConnect = new AutoResetEvent(false);
        private AutoResetEvent m_EventTcpSend = new AutoResetEvent(false);
        private string m_strTcpSend = "";
       
        private bool ConnectTcp()
        {
            Log("【TCP】, " + DateTime.Now.ToString() + ", ConnectTcp");
            try
            {
                sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ip = IPAddress.Parse(MyData.tcp_ip);
                iep = new IPEndPoint(ip, MyData.tcp_port);
                
                sc.BeginConnect(iep, new AsyncCallback(ConnectCallback), sc);                
                if (m_EventTcpConnect.WaitOne(5000))
                {
                    Log("【TCP】, " + DateTime.Now.ToString() + ", 连接成功");
                    return true;
                }
                                
                Log("【TCP】, " + DateTime.Now.ToString() + ", 连接失败");
                return false;
            }
            catch (Exception ex)
            {
                Log("【TCP】, " + DateTime.Now.ToString() + ", " + ex.Message);                
            }

            return false;
        }

        private void ConnectCallback(IAsyncResult iar)
        {
            Log("【TCP】, " + DateTime.Now.ToString() + ", ConnectCallback");

            try
            {
                Socket client = (Socket)iar.AsyncState;
                client.EndConnect(iar);

                m_EventTcpConnect.Set();
                Log("设置连接成功信号量");
            }            
            catch (Exception ex)
            {
                Log("【TCP】, " + DateTime.Now.ToString() + ", " + ex.Message);                
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            Socket client = null;
            string ip_str = string.Empty;
            try
            {
                StateObject state = (StateObject)iar.AsyncState;
                client = state.workSocket;                
                int bytesRead = client.EndReceive(iar);

                if (bytesRead > 0)
                {
                    Log("【TCP】, " + DateTime.Now.ToString() + ", ReceiveCallback");
                    string str = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    str = str.Trim();
                    lock (m_objTcpLock)
                    {
                        m_strCmd += str;
                    }

                    Log("【TCP】, " + DateTime.Now.ToString() + ", Rx:" + m_strCmd);

                    //异步接收    
                    state.buffer = new byte[StateObject.BufferSize];
                    sc.BeginReceive(state.buffer,
                                    0,
                                    StateObject.BufferSize,
                                    SocketFlags.None,
                                    new AsyncCallback(ReceiveCallback),
                                    state);
                }
            }
            catch (Exception ex)
            {
                Log("【TCP】, " + DateTime.Now.ToString() + ", " + ex.Message);                
            }
        }

        private bool WriteTcp(string str)
        {
            Log("【TCP】, " + DateTime.Now.ToString() + ", Tx：" + str);

            try
            {
                if (sc == null)
                {
                    Log("【TCP】, TCP句柄为空");
                    return false;
                }

                if (!sc.Connected)
                {
                    if (!ConnectTcp())
                    {
                        return false;
                    }
                }

                byte[] buffer = Encoding.Default.GetBytes(str);

                //异步发送
                sc.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(WriteCallback), sc);

                StateObject state = new StateObject();
                state.workSocket = sc;

                //异步接收    
                state.buffer = new byte[StateObject.BufferSize];
                sc.BeginReceive(state.buffer,
                                0,
                                StateObject.BufferSize,
                                SocketFlags.None,
                                new AsyncCallback(ReceiveCallback),
                                state);

                if (m_EventTcpSend.WaitOne(500))
                {
                    Log("【TCP】, 发送成功");
                    return true;
                }
                

                Log("【TCP】, 发送失败");
                return false;
            }
            catch (Exception ex)
            {
                Log("【TCP】," + ex.Message);
                
                sc.Shutdown(SocketShutdown.Both);
                sc.Disconnect(true);
                sc.Close();
                
                return false;
            }
        }

        private void WriteCallback(IAsyncResult iar)
        {
            Log("【TCP】, " + DateTime.Now.ToString() + ", WriteCallback");

            try
            {
                Socket client = (Socket)iar.AsyncState;
                int len = client.EndSend(iar);

                if (len == m_strTcpSend.Length)
                {
                    m_EventTcpSend.Set();
                    Log("发送完毕！");
                }
            }
            catch (Exception ex)
            {
                Log("【TCP】, " + DateTime.Now.ToString() + ", " + ex.Message);                
            }
        }

        private void DisconnectTcp()
        {
            Log("【TCP】, " + DateTime.Now.ToString() + ", 断开TCP连接");
            try
            {
                if (sc != null)
                {
                    if (sc.Connected)
                    {                        
                        sc.Shutdown(SocketShutdown.Both);
                        sc.Close();
                    }
                    sc.Dispose();
                    sc = null;
                }
            }
            catch (Exception ex)
            {
                Log("【TCP】, " + DateTime.Now.ToString() + ",  " + ex.Message);
            }
        }
    }
}