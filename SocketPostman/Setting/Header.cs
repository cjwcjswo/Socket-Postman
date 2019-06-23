using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SocketPostman.Setting.Common;

namespace SocketPostman.Setting
{
    [Serializable]
    public class BasicHeaderInfo
    {
        public bool CheckedPacketSize = false;
        public bool CheckedPacketID = false;
        public int IndexPacketSize = 0;
        public int IndexPacketID = 0;
        public IndexDataType DIndexPacketSize = 0;
        public IndexDataType DIndexPacketID = 0;

        public BasicHeaderInfo(bool chekcedPacketSize, bool checkedPacketID, int indexPacketSize, int indexPacketID, IndexDataType dIndexPacketSize, IndexDataType dIndexPacketID)
        {
            CheckedPacketSize = chekcedPacketSize;
            CheckedPacketID = checkedPacketID;
            IndexPacketSize = indexPacketSize;
            IndexPacketID = indexPacketID;
            DIndexPacketSize = dIndexPacketSize;
            DIndexPacketID = dIndexPacketID;
        }
    }


    [Serializable]
    public class AdditionalHeader
    {
        public string Name {get;set;}
        public int StartIndex { get; set; }
        public int Value { get; set; }
        public IndexDataType DataType { get; set; }

        public AdditionalHeader(string name, int startIndex, int value, IndexDataType dataType)
        { 
            Name = name;
            StartIndex = startIndex;
            Value = value;
            DataType = dataType;
        }
    }
}
