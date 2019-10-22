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
            Logger.Info($"Redis [{Name}] initialized");
        }

        private string Interact(string cmd)
        {
            string ret = "";

            R_Socket.SendTimeout = RequestTimeout;
            R_Socket.ReceiveTimeout = ResponseTimeout;

            string stringSent = BuildSendString(cmd);
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

            if (ret.StartsWith('-'))
            {
                ret = ret.TrimStart('-');
            }

            if (ret.StartsWith('+')) ret = ret.TrimStart('+');
            if (ret.StartsWith('$'))
            {
                int startIdx = ret.IndexOf("\r\n") + 2;
                ret = ret.Substring(startIdx);
                ret = ret.TrimEnd('\n').TrimEnd('\r');
            }

            return ret;
        }        

        private string BuildSendString(string cmd)
        {
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
                Socket tempSocket =
                    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    R_Socket = tempSocket;
                    Logger.Info($"{address.ToString()}:{mPort} connected");
                    if (password != "")
                    {
                        Logger.Info("auth result:" + Interact("auth " + password));
                        Logger.Info("ping result:" + Interact("ping"));
                        Logger.Info("info result:" + Interact("info"));
                        Logger.Info("select result:" + Interact("select 0"));
                        //Logger.Info("hmset result:" + Interact("hmset user:100001 name \"Jack\" age \"18\""));
                        //Logger.Info("hmset result:" + Interact("hmset user:100002 name \"Tom\" age \"17\""));
                        Logger.Info("scan result:" + Interact("scan 0 count 20"));
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

        public int GetDBCount()
        {
            int ret = 0;
            return ret;
        }

        public int UseDB(int db_number)
        {
            return 0;
        }

        public RO_RecordInfo[] GetKeys()
        {
            RO_RecordInfo[] keys = new RO_RecordInfo[0];
            return keys;
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
