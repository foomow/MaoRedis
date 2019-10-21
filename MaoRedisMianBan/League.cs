using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisMianBan
{
    public class League
    {
        public string Name;
        public List<Division> Divisions;

        public League(string name, List<Division> divisions)
        {
            Name = name;
            Divisions = divisions;
        }
    }

    public class Division
    {
        public string Name;
        public List<Team> Teams;

        public Division(string name, List<Team> teams)
        {
            Name = name;
            Teams = teams;
        }
    }

    public class Team
    {
        public string Name;

        public Team(string name)
        {
            Name = name;
        }
    }
}
