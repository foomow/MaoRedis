using Newtonsoft.Json.Linq;

namespace MaoRedisMianBan
{
    public class R_Key : R_Record
    {
        public JToken Value { get; set; }

        public R_Key(string name, JToken value)
        {
            Name = name;
            Value = value;
        }
    }
}
