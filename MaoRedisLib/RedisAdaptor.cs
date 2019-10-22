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
    public class RedisAdaptor
    {
        public string Name;
        public ushort RequestTimeout = 5000;
        public ushort ResponseTimeout = 5000;
        public ushort ReceiveCacheSize = 4096;

        private readonly string mIP;
        private readonly ushort mPort;
        private Socket R_Socket;

        public RedisAdaptor(string ip_addr, ushort port = 6379)
        {
            mIP = ip_addr;
            mPort = port;
            Name = mIP + ":" + mPort;
            R_Socket = null;
            Logger.Info($"Redis Adaptor [{Name}] initialized");
        }

        private JObject Interact(string cmd)
        {
            string ret = "";

            R_Socket.SendTimeout = RequestTimeout;
            R_Socket.ReceiveTimeout = ResponseTimeout;

            string stringSent = BuildSendString(cmd);

            if (stringSent == "") return ParseResponse("");

            string command = cmd.Trim(' ').Split(' ')[0].ToLower();

            byte[] bytesSent = Encoding.UTF8.GetBytes(stringSent);
            byte[] bytesReceived = new byte[ReceiveCacheSize];

            try
            {
                R_Socket.Send(bytesSent);
                Logger.Info($"message sent:{cmd}");

                int bytes;
                string page = "";

                do
                {
                    Logger.Info($"waiting for response data...");
                    try
                    {
                        bytes = R_Socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    }
                    catch (Exception)
                    {
                        ret = "-ResponseTimeout exception thrown";
                        break;
                    }
                    Logger.Info($"received data {bytes} bytes");
                    page += Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                }
                while (bytes > 0 && !page.EndsWith("\r\n"));
                if (ret == "") ret = page;
            }
            catch (Exception)
            {
                ret = "-RequestTimeout exception thrown";
            }

            return ParseResponse(ret, command);
        }

        private JObject ParseResponse(string response, string cmd = "")
        {
            JObject json = new JObject();
            json.Add("result", "unknown");
            json.Add("command", cmd);
            json.Add("data", response);

            if (response.StartsWith('-'))
            {
                json["result"] = "error";
                json["data"] = response.TrimStart('-').TrimEnd('\n').TrimEnd('\r');
            }

            else if (response.StartsWith('+'))
            {
                json["result"] = "success";
                json["data"] = response.TrimStart('+').TrimEnd('\n').TrimEnd('\r');
            }
            else if (response.StartsWith('$'))
            {
                int startIdx = response.IndexOf("\r\n") + 2;
                response = response.Substring(startIdx);
                json["result"] = "success";
                response = response.TrimEnd('\n').TrimEnd('\r');
                json["data"] = response;
                if (cmd == "info")
                {
                    JObject infos = new JObject();
                    string[] sections = response.Split("# ");
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
            //else if ("0123456789".IndexOf(response.Substring(0,1))>-1)
            //{
            //    json["result"] = "success";
            //    response = response.TrimEnd('\n').TrimEnd('\r');
            //    json["data"] = response;
            //}
            else if (response.StartsWith('*'))
            {
                json["result"] = "success";
                JArray dataList = new JArray();

                int arraylength = int.Parse(response.Split("\r\n")[0].TrimStart('*'));
                response = response.Substring(response.IndexOf("\r\n") + 2);
                for (int i = 0; i < arraylength; i++)
                {
                    int endIdx = response.LastIndexOf("\r\n") + 2;
                    string[] prefix = new string[] { "+", "-", "$", "*", ":" };
                    foreach (string p in prefix)
                    {
                        endIdx = (response.IndexOf($"\r\n{p}", 1) > -1) && (endIdx > response.IndexOf($"\r\n{p}", 1)) ? response.IndexOf($"\r\n{p}", 1) + 2 : endIdx;
                    }
                    string subdata = response.Substring(0, endIdx);
                    dataList.Add(ParseResponse(subdata)["data"]);
                    response = response.Substring(endIdx);
                }
                json["data"] = dataList;
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
                string str = strpart.Trim();
                ret += "$" + str.Length + "\r\n" + str + "\r\n";
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
                    Logger.Info("ping result:" + Interact("ping"));
                    Logger.Info("info result:" + Interact("info"));


                    break;
                }
                else
                {
                    continue;
                }
            }
            return true;
        }

        public int UseDB(int db_number)
        {
            Logger.Info("select result:" + Interact($"select {db_number}"));
            return db_number;
        }

        public JObject GetKeys()
        {
            JObject ret = Interact("keys *");
            Logger.Info("keys result:" + ret);
            return ret;
        }

        public JObject ScanKeys(int cursor, int count = 10)
        {
            JObject ret = Interact($"scan {cursor} count {count}");
            Logger.Info("scan result:" + ret);
            return ret;
        }

        public string Get(string key)
        {
            return "";
        }

        public RT_Hash[] GetHash(string key)
        {
            return new RT_Hash[0];
        }

        public void SetHash(string key, RT_Hash[] fields)
        {

        }

        public RO_RecordInfo[] GetListAll(string key)
        {
            return new RO_RecordInfo[0];
        }

        public RO_RecordInfo GetListByIndex(string key, int index = 0)
        {
            return new RO_RecordInfo();
        }

        public bool Set(string key, string value)
        {
            return false;
        }

        public bool Del(string key)
        {
            return false;
        }

        public RT_Type KeyType(string key)
        {
            return new RT_Type();
        }
    }
}
