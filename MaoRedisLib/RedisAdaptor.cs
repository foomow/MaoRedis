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

        private string ReceiveData(int length = -1,Func<int,int> report=null)
        {
            string ret = "";
            int totalreceived = 0;
            if (length >= 0)
            {
                length += 2;                
                int bytes;
                do
                {
                    byte[] bytesReceived = new byte[length];
                    try
                    {
                        bytes = R_Socket.Receive(bytesReceived, bytesReceived.Length, 0);
                        totalreceived += bytes;
                        report?.Invoke(totalreceived);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e.Message);
                        throw new SocketException();
                    }                    
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
                        totalreceived += bytes;
                        report?.Invoke(totalreceived);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e.Message);
                        throw new SocketException();
                    }
                    ret += Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                    if (bytesReceived[0] == (byte)'\r')
                    {
                        bytesReceived.Initialize();
                        try
                        {
                            bytes = R_Socket.Receive(bytesReceived, 1, 0);
                            totalreceived += bytes;
                            report?.Invoke(totalreceived);
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

        private JObject Interact(string cmd, Func<int, int> report)
        {
            R_Socket.SendTimeout = RequestTimeout;
            R_Socket.ReceiveTimeout = ResponseTimeout;

            string stringSent = BuildSendString(cmd);

            Logger.Info(stringSent);

            if (stringSent == "") return ParseResponse("",report);

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
                ret = ParseResponse(command, report);
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

        private JObject ParseResponse(string cmd = "",Func<int, int> report=null)
        {
            JObject json = new JObject();
            json.Add("result", "unknown");
            json.Add("command", cmd);

            string data = ReceiveData(report:report);

            json.Add("data", data);

            if (data.StartsWith('-'))
            {
                json["result"] = "error";
                json["data"] = data.TrimStart('-');
            }
            else if (data.StartsWith(':'))
            {
                json["result"] = "success";
                json["data"] = data.TrimStart(':');
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
                    if (length != -1)
                    {
                        json["data"] = ReceiveData(length,report);
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
                            dataList.Add(ParseResponse(report:report)["data"].ToString(), ParseResponse(report:report)["data"].ToString());
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
                                dataList.Add(ParseResponse(report:report)["data"]);
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

        
    }
}
