using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPostman.Setting
{
    [Serializable]
    public class Manager
    {

        private static Manager instance = null;

        public SettingInfo CurrentSetting
        {
            get; set;
        }
        private Manager()
        {
            CurrentSetting = new SettingInfo();
        }

        public static Manager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Manager();
                }
                return instance;
            }
        }


        [Serializable]
        public class SettingInfo
        {
            public BasicHeaderInfo BasicHeader = new BasicHeaderInfo(false, false, 0, 0, 0, 0);
            public List<AdditionalHeader> AHList = new List<AdditionalHeader>();
        }
    }
}
