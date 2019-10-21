using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisMianBan
{
    public class ListLeagueList : List<League>
    {
    }

    public class League
    {
        public string Name { get; set; }
        public List<Division> Divisions { get; set; }

    public League(string name, List<Division> divisions)
        {
            Name = name;
            Divisions = divisions;
        }
    }

    public class Division
    {
        public string Name { get; set; }
    public List<Team> Teams { get; set; }

    public Division(string name, List<Team> teams)
        {
            Name = name;
            Teams = teams;
        }
    }

    public class Team
    {
        public string Name { get; set; }

        public Team(string name)
        {
            Name = name;
        }
    }
}
