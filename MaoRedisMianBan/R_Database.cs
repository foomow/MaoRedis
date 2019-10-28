using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisMianBan
{
    public class R_Database : R_Folder
    {
        public new int Count
        {
            get; set;
        }
        public R_Database(string name, List<R_Record> records,R_Server server) : base(name, records,server)
        {
        }
    }
}
