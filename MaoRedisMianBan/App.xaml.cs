﻿using Newtonsoft.Json.Linq;
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

        public R_Server AddServer()
        {
            AddServerDlg dlg = new AddServerDlg();
            dlg.Owner = MainWindow;
            if ((bool)dlg.ShowDialog())
            {
                R_Server server;
                string addr = dlg.Addr;
                ushort port = dlg.Port;
                string name = dlg.ServerName;
                if (name == "")
                    name = addr + ":" + port;
                string psw = dlg.Password;

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
            return null;
        }

        public void RefreshFolder(R_Record record)
        {
            ReloadDlg dlg = new ReloadDlg(record);
            dlg.Owner = MainWindow;
            dlg.ShowDialog();
        }

        public string GetKey(R_Key key)
        {
            return key.Server.GetKey(key);
        }

        public string DeleteKey(R_Key key)
        {
            string ret = key.Server.DeleteKey(key);
            R_Folder folder = key.Folder;
            try
            {
                JObject retJson = JObject.Parse(ret);
                if (retJson["result"].ToString() == "success")
                {
                    lock (folder.Records)
                    {
                        folder.Records.Remove(key);
                    }
                    RefreshFolder(folder);
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message + "\r\n" + ex.StackTrace;
            }
            return ret;
        }

        public string DeleteFolder(R_Folder folder)
        {
            List<string> keys = GetAllKeys(folder);
            int db_number;
            if (folder.Pattern.Contains(":"))
            {
                int idx = folder.Pattern.IndexOf(":");
                db_number = int.Parse(folder.Pattern.Substring(0, idx));
            }
            else
            {
                db_number = int.Parse(folder.Pattern);
            }
            string ret = folder.Server.DeleteKeys(db_number, keys);
            try
            {
                JObject retJson = JObject.Parse(ret);
                if (retJson["result"].ToString() == "success")
                {
                    lock (folder.Records)
                    {
                        folder.Records.Clear();
                    }
                    RefreshFolder(folder);
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message + "\r\n" + ex.StackTrace;
            }
            return ret;
        }

        private List<string> GetAllKeys(R_Folder folder)
        {
            List<string> ret = new List<string>();
            foreach (R_Record record in folder.Records)
            {
                if (record.GetType() == typeof(R_Key))
                {
                    ret.Add(record.Name);
                }
                else
                {
                    ret.AddRange(GetAllKeys((R_Folder)record));
                }
            }
            return ret;
        }

        public void ServerConnect(R_Server server)
        {
            ConnectDlg dlg = new ConnectDlg(server);
            dlg.Owner = MainWindow;
            if (dlg.ShowDialog() == false)
            {
                if (dlg.Result == "fail")
                {
                    MessageBox.Show("Failed to connect to server.", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (dlg.Result == "noauth")
                {
                    MessageBox.Show("Failed to authorize", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        public bool ServerEdit(R_Server server)
        {
            AddServerDlg dlg = new AddServerDlg();
            dlg.TB_Addr.Text = server.Addr;
            dlg.TB_Port.Text = server.Port.ToString();
            dlg.TB_Psw.Text = server.Psw;
            dlg.TB_ServerName.Text = server.Name;
            dlg.BTN_Add.Content = "Edit";
            dlg.Owner = MainWindow;
            if ((bool)dlg.ShowDialog())
            {
                string addr = dlg.Addr;
                ushort port = dlg.Port;
                string serverName = dlg.ServerName;
                if (serverName == "")
                    serverName = addr + ":" + port;
                string psw = dlg.Password;

                server.Addr = addr;
                server.Port = port;
                server.Psw = psw;
                server.Name = serverName;
                return true;
            }
            return false;
        }
    }
}
