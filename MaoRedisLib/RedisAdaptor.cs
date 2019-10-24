using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public string Name;
        public ushort RequestTimeout = 5000;
        public ushort ResponseTimeout = 5000;

        private readonly string mIP;
        private readonly ushort mPort;
        private Socket R_Socket;
        private List<byte> ReceivedData;

        public RedisAdaptor(string ip_addr, ushort port = 6379)
        {
            mIP = ip_addr;
            mPort = port;
            Name = mIP + ":" + mPort;
            R_Socket = null;
            Logger.Info($"Redis Adaptor [{Name}] initialized");
        }

        private string ParseData(int length = -1)
        {
            string ret = "-err\r\n";
            if (length >= 0)
            {
                if (ReceivedData.Count >= length)
                {
                    byte[] bytesReceived = ReceivedData.GetRange(0, length).ToArray();
                    ret = Encoding.UTF8.GetString(bytesReceived, 0, length);
                    ReceivedData.RemoveRange(0, length);
                }
            }
            else
            {
                int idx = ReceivedData.IndexOf((byte)'\r');
                while (idx != -1 && (idx + 1) < ReceivedData.Count && ReceivedData[idx + 1] != (byte)'\n')
                {
                    idx = ReceivedData.IndexOf((byte)'\r', idx + 1);
                }
                if (idx != -1 && (idx + 1) < ReceivedData.Count && ReceivedData[idx + 1] == (byte)'\n')
                {
                    byte[] bytesReceived = ReceivedData.GetRange(0, idx + 2).ToArray();
                    ret = Encoding.UTF8.GetString(bytesReceived, 0, idx + 2);
                    ReceivedData.RemoveRange(0, idx + 2);
                }

            }
            ret = ret.TrimEnd('\n').TrimEnd('\r');
            return ret;
        }


        private JObject Interact(string cmd)
        {
            R_Socket.SendTimeout = RequestTimeout;
            R_Socket.ReceiveTimeout = ResponseTimeout;

            string stringSent = BuildSendString(cmd);

            if (stringSent == "") return ParseResponse("");

            string command = cmd.Trim(' ').Split(' ')[0].ToLower();

            byte[] bytesSent = Encoding.UTF8.GetBytes(stringSent);

            try
            {
                R_Socket.Send(bytesSent);
            }
            catch (Exception)
            {
                JObject json = new JObject();
                json.Add("result", "error");
                json.Add("command", command);
                json.Add("data", "RequestTimeout exception thrown");
                return json;
            }
            byte[] bytesReceived = new byte[1024];
            ReceivedData = new List<byte>();
            int bytes;
            do
            {
                bytesReceived.Initialize();
                try
                {
                    bytes = R_Socket.Receive(bytesReceived, bytesReceived.Length, 0);
                }
                catch (Exception e)
                {
                    JObject json = new JObject();
                    json.Add("result", "error");
                    json.Add("command", command);
                    json.Add("data", "ReceiveTimeout exception thrown");
                    return json;
                }
                ReceivedData.AddRange(bytesReceived.ToList().GetRange(0, bytes));                
            } while (bytes == 1024);
            return ParseResponse(command);
        }

        private JObject ParseResponse(string cmd = "")
        {
            JObject json = new JObject();
            json.Add("result", "unknown");
            json.Add("command", cmd);

            string data = ParseData();

            json.Add("data", data);

            if (data.StartsWith('-'))
            {
                json["result"] = "error";
                json["data"] = data.TrimStart('-');
            }
            else if (data.StartsWith('+'))
            {
                json["result"] = "success";
                json["data"] = data.TrimStart('+');
            }
            else if (data.StartsWith('$'))
            {
                json["result"] = "success";
                int length = int.Parse(data.TrimStart('$'));
                json["data"] = ParseData(length);
                if (cmd == "info")
                {
                    JObject infos = new JObject();
                    string[] sections = json["data"].ToString().Split("# ");
                    foreach (string section in sections)
                    {
                        if (section.Length > 0)
                        {
                            string[] lines = section.Split("\r\n");
                            JObject sectJson = new JObject();
                            foreach (string line in lines)
                            {
                                if (line.Length > 0 && line.Contains(':'))
                                {
                                    sectJson.Add(line.Split(':')[0], line.Split(':')[1]);
                                }
                            }
                            infos.Add(lines[0], sectJson);
                        }
                    }
                    json["data"] = infos;
                }
            }
            else if (data.StartsWith('*'))
            {
                json["result"] = "success";
                int arraylength = int.Parse(data.TrimStart('*'));

                if (cmd == "hgetall")
                {
                    JObject dataList = new JObject();
                    for (int i = 0; i < arraylength / 2; i++)
                    {
                        dataList.Add(ParseResponse()["data"].ToString(), ParseResponse()["data"].ToString());
                    }
                    json["data"] = dataList;
                }
                else
                {
                    JArray dataList = new JArray();
                    for (int i = 0; i < arraylength; i++)
                    {
                        dataList.Add(ParseResponse()["data"]);
                    }
                    json["data"] = dataList;
                }
            }
            return json;
        }

        private string BuildSendString(string cmd)
        {
            if (cmd == null || cmd.Trim(' ') == "") return "";
            string ret = "*";
            string[] strlist = cmd.Split(" ");
            ret += strlist.Length + "\r\n";
            foreach (string strpart in strlist)
            {
                string str = strpart.Trim().Replace("%20", " ").Replace("%25", "%");
                int len = Encoding.UTF8.GetByteCount(str);
                ret += "$" + len + "\r\n" + str + "\r\n";
            }
            return ret;
        }

        public bool Connect(string password = "")
        {
            IPHostEntry entry = Dns.GetHostEntry(mIP);
            foreach (IPAddress address in entry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, mPort);
                R_Socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                R_Socket.Connect(ipe);

                if (R_Socket.Connected)
                {
                    Logger.Info($"{address.ToString()}:{mPort} connected");
                    if (password != "")
                    {
                        Logger.Info("auth result:" + Interact("auth " + password));
                    }
                    //Logger.Info("ping result:" + Interact("ping"));
                    //Logger.Info("info result:" + Interact("info"));
                    break;
                }
                else
                {
                    continue;
                }
            }
            return true;
        }
    }
}
