using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaoRedisLib;
using StackExchange.Redis;

namespace MaoRedisMianBan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Team teamA1 = new Team("Team A1");
            Team teamB1 = new Team("Team B1");
            Team teamC1 = new Team("Team C1");
            Division divisionA = new Division("Division A", new List<Team>() { teamA1, teamB1, teamC1 });

            Team teamA2 = new Team("Team A2");
            Team teamB2 = new Team("Team B2");
            Team teamC2 = new Team("Team C2");
            Division divisionB = new Division("Division B", new List<Team>() { teamA2, teamB2, teamC2 });

            League leagueA = new League("League A", new List<Division>() { divisionA, divisionB });

            Team teamA3 = new Team("Team A3");
            Team teamB3 = new Team("Team B3");
            Team teamC3 = new Team("Team C3");
            Division divisionC = new Division("Division C", new List<Team>() { teamA3, teamB3, teamC3 });

            Team teamA4 = new Team("Team A4");
            Team teamB4 = new Team("Team B4");
            Team teamC4 = new Team("Team C4");
            Division divisionD = new Division("Division D", new List<Team>() { teamA4, teamB4, teamC4 });

            League leagueB = new League("League B", new List<Division>() { divisionC, divisionD });

            ListLeagueList LeagueList = new ListLeagueList();
            
            LeagueList.Add(leagueA);
            LeagueList.Add(leagueB);

            
            InitializeComponent();
            MyTreeViewItems.ItemsSource = LeagueList;
            MyMenuItems.ItemsSource = LeagueList;

            //Console.WriteLine("************** Hello Chengdu! **************");
            //RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183:6379", "MukAzxGMOL2");
            //adaptor.Connect();
            //int db_count = adaptor.GetDBCount();
            //Logger.Info($"Database count:{db_count}");
            //int current_db = adaptor.UseDB(0);
            //Logger.Info($"Current Database:{current_db}");
            //RedisKey[] keys = adaptor.GetKeys().ToArray();
            //foreach (RedisKey key in keys)
            //{
            //    Logger.Info(adaptor.KeyType(key) + " " + key.ToString() + ":[" + adaptor.Get(key) + "]");
            //}
            //Console.WriteLine("************** Bye! **************");
        }
    }
}
