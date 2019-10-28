using Newtonsoft.Json.Linq;

namespace MaoRedisMianBan
{
    public class R_Key : R_Record
    {
        public JToken Value { get; set; }
        public R_Server Server { get; }
        public int Database_Number{get;set;}

        public R_Key(string name, JToken value,R_Server server)
        {
            Name = name;
            Value = value;
            Server = server;
        }
    }
}
