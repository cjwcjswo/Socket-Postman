using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPostman.Setting
{
    public enum CellColumn
    {
        IndexName,
        IndexStart,
        IndexValue,
        IndexType,
    };

    public enum IndexDataType
    {
        Select = -1,
        Int8 = 0,
        Int16 = 1,
        Int32 = 2,
        Int64 = 3,
    }

    public class Common
    {

        public static Dictionary<IndexDataType, int> TypeToSizeMap = new Dictionary<IndexDataType, int>
        {
            { IndexDataType.Int8, 1 },
            { IndexDataType.Int16, 2 },
            { IndexDataType.Int32, 4},
            { IndexDataType.Int64, 8 },
        };
    }
}
