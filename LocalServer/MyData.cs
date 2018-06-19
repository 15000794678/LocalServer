using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LocalServer
{
    public class MyData
    {
        public static int startMode;

        public static int communicateMode;

        public static int num;
        public static bool bLeftOut = true; //左边测试完毕后出站
        public static bool bRightOut = true; //右边测试完毕后出站
        
        public static Dictionary<int, PortStruct> dic_port = new Dictionary<int, PortStruct>();

        //图像相关
        public static string dllfilepath;  //库文件路径
        public static string dllclass;   //库类名称
        public static string modulefilepath;  //模板文件

        public static Dictionary<int, Postion> dic_xy = new Dictionary<int, Postion>();

        public static string halconDLLName = "HalconATE";
        public static string halconCheckModuleName = "Halcon_CheckModule";
        public static string halconCheckImageStatusName = "Halcon_CheckImageStatus";
        public static string halconCheckLightStatusName = "Halcon_CheckLightStatus";
        public static int r_thresthod = 400;
        public static int b_thresthod = 1000;
        public static int y_thresthod = 40;

        public static string pixel = "";

        public static string tcp_ip = "";
        public static int tcp_port = 0;

        //public static int SpeedUpEnable = 0; //默认不使用加速线程
        public static int comDelayTime = 2500;
        public static int pictureDelayTime = 1500;

        public MyData()
        {
            num = 1;

            dic_port.Clear();

            dllfilepath = string.Empty;

            dllclass = string.Empty;

            modulefilepath = string.Empty;
        }

        public static void LoadCfg()
        {
            try
            {
                startMode = int.Parse(MyFile.Read("Mode", "Start", "1", ".\\localserver.ini"));
                communicateMode = int.Parse(MyFile.Read("Mode", "Communicate", "1", ".\\localserver.ini"));
                tcp_ip = MyFile.Read("TCPIP", "IP", "127.0.0.1", ".\\localserver.ini");
                tcp_port = int.Parse(MyFile.Read("TCPIP", "Port", "9100", ".\\localserver.ini"));

                num = int.Parse(MyFile.Read("TestNum", "Num", "1", ".\\localserver.ini"));
                r_thresthod = int.Parse(MyFile.Read("ResetColor", "r", "400", ".\\localserver.ini"));
                b_thresthod = int.Parse(MyFile.Read("ResetColor", "b", "1000", ".\\localserver.ini"));
                y_thresthod = int.Parse(MyFile.Read("ResetColor", "y", "40", ".\\localserver.ini"));

                dic_port.Clear();
                for (int i = 1; i < num + 1; i++)
                {
                    int no = i;
                    PortStruct ps = new PortStruct();
                    
                    ps.portname = MyFile.Read("Port" + i.ToString(), "PortName", "COM" + i, ".\\localserver.ini");
                    ps.baudrate = int.Parse(MyFile.Read("Port" + i.ToString(), "BaudRate", "115200", ".\\localserver.ini"));

                    dic_port.Add(no, ps);
                }

                dllfilepath = MyFile.Read("Image", "dllfilepath", "", ".\\localserver.ini");
                dllclass = MyFile.Read("Image", "dllclass", "", ".\\localserver.ini");
                //modulefilepath = MyFile.Read("Image", "modulefilepath", "", ".\\localserver.ini");

                dic_xy.Clear();
                for (int i = 1; i < num + 1; i++)
                {
                    //int no = i;
                    Postion ps = new Postion();

                    ps.button_x = int.Parse(MyFile.Read("Position", "Button" + i.ToString() + "_x", "0", ".\\localserver.ini"));
                    ps.button_y = int.Parse(MyFile.Read("Position", "Button" + i.ToString() + "_y", "0", ".\\localserver.ini"));
                    ps.input_x = int.Parse(MyFile.Read("Position", "Input" + i.ToString() + "_x", "0", ".\\localserver.ini"));
                    ps.input_y = int.Parse(MyFile.Read("Position", "Input" + i.ToString() + "_y", "0", ".\\localserver.ini")); ;

                    dic_xy.Add(i, ps);
                }

                //SpeedUpEnable = int.Parse(MyFile.Read("SpeedUp", "Enable", "0", ".\\localserver.ini"));
                comDelayTime = int.Parse(MyFile.Read("DelayTime", "ComDelay", "2500", ".\\localserver.ini"));
                pictureDelayTime = int.Parse(MyFile.Read("DelayTime", "PictureDelay", "1500", ".\\localserver.ini"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SaveCfg()
        {
            try
            {
                MyFile.Write("Mode", "Start", startMode.ToString(), ".\\localserver.ini");
                MyFile.Write("Mode", "Communicate", communicateMode.ToString(), ".\\localserver.ini");

                MyFile.Write("TestNum", "Num", num.ToString(), ".\\localserver.ini");
                MyFile.Write("TCPIP", "IP", tcp_ip, ".\\localserver.ini");
                MyFile.Write("TCPIP", "Port", tcp_port.ToString(), ".\\localserver.ini");

                foreach (KeyValuePair<int, PortStruct> pp in dic_port)
                {
                    MyFile.Write("Port" + pp.Key, "PortName", pp.Value.portname, ".\\localserver.ini");
                    MyFile.Write("Port" + pp.Key, "BaudRate", pp.Value.baudrate.ToString(), ".\\localserver.ini");
                }

                MyFile.Write("Image", "dllfilepath", dllfilepath, ".\\localserver.ini");
                MyFile.Write("Image", "dllclass", dllclass, ".\\localserver.ini");
                //MyFile.Write("Image", "modulefilepath", modulefilepath, ".\\localserver.ini");

                foreach (KeyValuePair<int, Postion> kp in dic_xy)
                {
                    MyFile.Write("Position", "Button" + kp.Key + "_x", kp.Value.button_x.ToString(), ".\\localserver.ini");
                    MyFile.Write("Position", "Button" + kp.Key + "_y", kp.Value.button_y.ToString(), ".\\localserver.ini");
                    MyFile.Write("Position", "Input" + kp.Key + "_x", kp.Value.input_x.ToString(), ".\\localserver.ini");
                    MyFile.Write("Position", "Input" + kp.Key + "_y", kp.Value.input_y.ToString(), ".\\localserver.ini");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class Postion
    {
        public int button_x;
        public int button_y;
        public int input_x;
        public int input_y;
    };

    public class PortStruct
    {
        public string portname;
        public int baudrate;
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
}
