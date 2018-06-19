using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ServerTest
{
    class DeviceParam
    {
        public string station; //站位名称
        public string ip; //IP地址
        public int pass; //成功数量
        public int fail; //失败数量

        //RFID标签对应的Label
        public Label front_label;
        public Label back_label;

        public object obj;  //接收和处理时需要上锁
        public string cmd;  //接受到的命令缓存

        public Socket sc;   //对应的socket

        public bool logon; //登陆

        public DeviceParam()
        {
            station = string.Empty;
            ip = string.Empty;
            pass = 0;
            fail = 0;

            front_label = null;
            back_label = null;

            obj = null;
            cmd = string.Empty;
            sc = null;

            logon = false;
        }
        public DeviceParam(string sta, string i, int p, int f, Label fl, Label bl, object j, string str, Socket s, bool log)
        {
            station = sta;
            ip = i;
            pass = p;
            fail = f;
            front_label = fl;
            back_label = bl;
            obj = j;
            cmd = str;
            sc = s;
            logon = false;
        }
    }

    class RfidParm
    {
        public string rfid; //前面小站位的rfid
        public string sn;
        public bool result;
        public Label label;
        public Color color;
        public string failstation;

        public RfidParm()
        {
            rfid = string.Empty;
            sn = string.Empty;
            result = true;
            label = null;
            color = Color.Transparent;
            failstation = string.Empty;
        }

        public RfidParm(string r, string s, bool res, Label l, Color cl, string failsta)
        {
            rfid = r;
            sn = s;
            result = res;
            label = l;
            color = cl;
            failstation = failsta;
        }

    }

    public class StateObject
    {
        // Client socket.     
        public Socket workSocket = null;
        // Size of receive buffer.     
        public const int BufferSize = 1024;
        // Receive buffer.     
        public byte[] buffer = new byte[BufferSize];
        // Received data string.     
        //public StringBuilder sb = new StringBuilder();
    }

    public class StationLabel
    {
        public string station;
        public Label front_label;
        public Label back_label;

        public StationLabel(string sta, Label fl, Label bl)
        {
            station = sta;
            front_label = fl;
            back_label = bl;
        }
    }

    public class StationResult
    {
        public uint pass;
        public uint fail;
        public object loc;

        public StationResult()
        {
            pass = 0;
            fail = 0;
            loc = new object();
        }
    }


}
