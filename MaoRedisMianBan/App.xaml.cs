using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MaoRedisMianBan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly List<R_Server> ServerPool = new List<R_Server>();

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                string FileName = AppContext.BaseDirectory + "/config.json";
                JObject cfgJson = new JObject();
                JArray serversJson = new JArray();
                foreach (R_Server server in ServerPool)
                {
                    JObject serverJson = new JObject();
                    serverJson.Add("name", server.Name);
                    serverJson.Add("addr", server.Addr);
                    serverJson.Add("port", server.Port.ToString());
                    serverJson.Add("psw", server.Psw);
                    serversJson.Add(serverJson);
                }
                cfgJson.Add("servers", serversJson);
                File.WriteAllText(FileName, cfgJson.ToString());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Save config file failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadConfig();
        }

        private void LoadConfig()
        {
            string FileName = AppContext.BaseDirectory + "/config.json";

            if (File.Exists(FileName))
            {
                try
                {
                    JObject json = JObject.Parse(File.ReadAllText(FileName));
                    if (json.ContainsKey("servers"))
                    {
                        if (json["servers"].Type == JTokenType.Array)
                        {
                            JArray serverJArray = (JArray)json["servers"];
                            foreach (JToken serverToken in serverJArray)
                            {
                                R_Server newserver = new R_Server(serverToken["name"].ToString(), new List<R_Database>())
                                {
                                    Addr = serverToken["addr"].ToString(),
                                    Port = ushort.Parse(serverToken["port"].ToString()),
                                    Psw = serverToken["psw"].ToString()
                                };
                                if (!ServerPool.Exists(x => x.Addr == newserver.Addr && x.Port == newserver.Port))
                                    ServerPool.Add(newserver);
                            }
                        }
                    }
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Load config file failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

            }
            JObject cfgJson = new JObject();
            JObject server = new JObject();
            server.Add("name", "localhost");
            server.Add("addr", "127.0.0.1");
            server.Add("port", "6379");
            server.Add("psw", "");
            JArray serversJson = new JArray();
            serversJson.Add(server);
            cfgJson.Add("servers", serversJson);
            File.WriteAllText(FileName, cfgJson.ToString());

            R_Server defaultserver = new R_Server(server["name"].ToString(), new List<R_Database>())
            {
                Addr = server["addr"].ToString(),
                Port = ushort.Parse(server["port"].ToString()),
                Psw = server["psw"].ToString()
            };

            ServerPool.Add(defaultserver);
        }

        public R_Server AddServer(string name, string addr, ushort port, string psw)
        {
            R_Server server;
            if (ServerPool.Exists(x => x.Addr == addr && x.Port == port))
            {
                MessageBox.Show("The server already exists.", "Add server exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                server = ServerPool.Find(x => x.Addr == addr && x.Port == port);
            }
            else
            {
                server = new R_Server(name, new List<R_Database>())
                {
                    Addr = addr,
                    Port = port,
                    Psw = psw
                };
                ServerPool.Add(server);
            }
            return server;
        }

        public void RefreshFolder(R_Record record)
        {
            ((R_Folder)record).Server.RefreshKeys((R_Folder)record);
        }

        public string GetKey(R_Key key)
        {
            return key.Server.GetKey(key);
        }
    }
}
