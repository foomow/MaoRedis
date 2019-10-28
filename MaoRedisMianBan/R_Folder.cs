using System.Collections.Generic;

namespace MaoRedisMianBan
{
    public class R_Folder : R_Record
    {
        public List<R_Record> Records { get; set; }
        public R_Server Server { get; }
        public string Pattern { get; set; }
        public int Count
        {
            get
            {
                int ret = 0;
                foreach (R_Record record in Records)
                {
                    if (record.GetType() == typeof(R_Key)) ret++;
                    if (record.GetType() == typeof(R_Folder)) ret += ((R_Folder)record).Count;
                }
                return ret;
            }
        }

        public R_Folder(string name, List<R_Record> records,R_Server server)
        {
            Name = name;
            Records = records;
            Server = server;
        }

    }
}
