using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisLib
{
    public class RO_Value
    {
        public string Value;
    }

    public class RO_RecordInfo
    {
        public string Key;
        public string Value;
    }

    public class RO_Database
    {
        public List<RO_RecordInfo> RecordList;
    }

    public class RO_Server
    {
        public List<RO_Database> DatabaseList;
    }

    public class RT_Type
    { }

    public class RT_String: RT_Type
    { }
    public class RT_List: RT_Type
    { }
    public class RT_Set: RT_Type
    { }
    public class RT_SortedSet: RT_Type
    { }
    public class RT_Hash: RT_Type
    { }
    public class RT_BitArray: RT_Type
    { }
    public class RT_HyperLogLog: RT_Type
    { }
    public class RT_Stream: RT_Type
    { }
}
