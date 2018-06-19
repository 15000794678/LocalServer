using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevComponents.DotNetBar;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ServerTest
{
    public partial class Form1 : Office2007Form
    {
        private Socket sc = null;
        private IPAddress ip = null;
        private IPEndPoint iep = null;

        //private AutoResetEvent m_EventAccept = new AutoResetEvent(false);
        private object m_lockDeviceParm = new object();
        private Dictionary<string, DeviceParam> m_dicDeviceParam = new Dictionary<string, DeviceParam>();

        private void AcceptThread()
        {
            m_dicDeviceParam.Clear();

            try
            {
                sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ip = IPAddress.Any;//IPAddress.Parse("192.168.1.104");
                iep = new IPEndPoint(ip, 7600);

                sc.Bind(iep);

                sc.Listen(3000);

                //m_EventAccept.Reset();

                sc.BeginAccept(new AsyncCallback(AcceptCallback), sc);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[DEBUG]AcceptThreadException:" + ex.Message);
                MessageBox.Show("mysocket出错:" + ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult iar)
        {
            try
            {
                Socket listenner = (Socket)iar.AsyncState;
                Socket client = listenner.EndAccept(iar);

                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer,
                                    0,
                                    StateObject.BufferSize,
                                    SocketFlags.None,
                                    new AsyncCallback(ReceiveCallback),
                                    state);

                sc.BeginAccept(new AsyncCallback(AcceptCallback), sc);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[DEBUG]AcceptException: " + ex.Message);
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
                IPAddress remote_ip = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address;
                ip_str = remote_ip.ToString();
                int bytesRead = client.EndReceive(iar);

                if (bytesRead > 0)
                {
                    string str = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

                    lock (m_lockDeviceParm)
                    {
                        if (m_dicDeviceParam.ContainsKey(ip_str))
                        {
                            //lock (m_dicDeviceParam[ip_str].obj)
                            {
                                m_dicDeviceParam[ip_str].cmd += str;
                                m_dicDeviceParam[ip_str].sc = client;
                            }
                        }
                        else
                        {
                            //第一次初始化
                            m_dicDeviceParam.Add(ip_str,
                                new DeviceParam("", ip_str, 0, 0, null, null, new object(), str, client, false));
                        }
                    }

                    //打印Logout                    
                    //FrmLogout.Logout("ReceiveCallback:" + ip_str + ", " + str + "\r\n");
                    Logout("ReceiveCallback:" + ip_str + ", " + str + "\r\n");

                    state.buffer = new byte[StateObject.BufferSize];
                }

                client.BeginReceive(state.buffer,
                                    0,
                                    StateObject.BufferSize,
                                    SocketFlags.None,
                                    new AsyncCallback(ReceiveCallback),
                                    state);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[DEBUG]ReceiveException:" + ex.Message);

                //if (client != null)
                //{
                //    if (client.Connected)
                //    {
                //        client.Shutdown(SocketShutdown.Both);
                //        client.Close();
                //    }
                //    client.Dispose();
                //    client = null;
                //}
                if (m_dicDeviceParam.ContainsKey(ip_str))
                {
                    //lock (m_dicDeviceParam[ip_str].obj)
                    lock (m_lockDeviceParm)
                    {
                        m_dicDeviceParam[ip_str].cmd = string.Empty;
                    }
                }
            }
        }

        private bool SocketSend(Socket client, string str)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(str);
                if (client.Send(buffer) == buffer.Length)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[DEBUG]SocketSendException: " + ex.Message);
            }

            return false;
        }
    }
}
