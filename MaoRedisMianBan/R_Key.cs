using Newtonsoft.Json.Linq;

namespace MaoRedisMianBan
{
    public class R_Key : R_Record
    {
        public JToken Value { get; set; }
        public R_Server Server { get; set; }
        public int Database_Number{get;set;}
        public R_Folder Folder { get; set; }

        public R_Key(string name, R_Server server = null, int db_number = 0, R_Folder folder = null, JToken value=null)
        {
            Name = name;
            Value = value;
            Server = server;
            Database_Number = db_number;
            Folder = folder;
        }
    }
}
