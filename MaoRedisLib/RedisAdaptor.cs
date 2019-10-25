using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ushort ResponseTimeout = 15000;

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

        private string ReceiveData(int length = -1)
        {
            string ret = "";
            if (length >= 0)
            {
                length += 2;
                int totalreceived = 0;
                int bytes;
                do
                {
                    byte[] bytesReceived = new byte[length];
                    try
                    {
                        bytes = R_Socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    }
                    catch
                    {
                        throw new SocketException();
                    }
                    totalreceived += bytes;
                    ret += Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                    length -= totalreceived;
                } while (length > 0);
            }
            else
            {
                byte[] bytesReceived = new byte[1];
                int bytes;
                do
                {
                    bytesReceived.Initialize();
                    try
                    {
                        bytes = R_Socket.Receive(bytesReceived, 1, 0);
                    }
                    catch
                    {
                        throw new SocketException();
                    }
                    ret += Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                    if (bytesReceived[0] == (byte)'\r')
                    {
                        bytesReceived.Initialize();
                        try
                        {
                            bytes = R_Socket.Receive(bytesReceived, 1, 0);
                        }
                        catch
                        {
                            throw new SocketException();
                        }
                        ret += Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                        if (bytesReceived[0] == (byte)'\n') break;
                    }
                } while (true);
            }
            ret = ret.TrimEnd('\n').TrimEnd('\r');
            return ret;
        }

        private JObject Interact(string cmd)
        {
            Logger.Info(cmd);
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
            JObject ret;
            try
            {
                ret = ParseResponse(command);
            }
            catch (Exception e)
            {
                ret = new JObject();
                ret.Add("result", "error");
                ret.Add("command", cmd);
                ret.Add("data", e.Message);
            }
            return ret;
        }

        private JObject ParseResponse(string cmd = "")
        {
            JObject json = new JObject();
            json.Add("result", "unknown");
            json.Add("command", cmd);

            string data = ReceiveData();

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
                try
                {
                    int length = int.Parse(data.TrimStart('$'));
                    json["data"] = ReceiveData(length);
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
                catch
                {
                    json["result"] = "error";
                    json["datar-received"] = json["data"];
                    json["data"] = "parse failed";
                }
            }
            else if (data.StartsWith('*'))
            {
                json["result"] = "success";
                try
                {
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
                            try
                            {
                                dataList.Add(ParseResponse()["data"]);
                            }
                            catch (Exception e)
                            {
                                dataList.Add(e.Message);
                            }
                        }
                        json["data"] = dataList;
                    }
                }
                catch
                {
                    json["result"] = "error";
                    json["datar-eceived"] = json["data"];
                    json["data"] = "parse failed";
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
