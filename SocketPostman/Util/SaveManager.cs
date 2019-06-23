using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static SocketPostman.Setting.Manager;

namespace SocketPostman.Util
{
    class SaveManager
    {
        private static SaveManager instance = null;
        private const string SAVE_FOLDER_NAME = "user";
        private const string SAVE_FILE_NAME = "save.dat";

        private SaveManager()
        {

        }

        [Serializable]
        public class SaveData
        {
            public SettingInfo SaveSettingInfo;
            public string CurrentFolder;
            public string SaveHost;
            public string SavePort;

            public SaveData(SettingInfo saveSettingInfo, string currentFolder, string saveHost, string savePort)
            {
                SaveSettingInfo = saveSettingInfo;
                CurrentFolder = currentFolder;
                SaveHost = saveHost;
                SavePort = savePort;
            }
        }

        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SaveManager();
                }
                return instance;
            }
        }

        public void Save(SaveData data)
        {
            string dirName = ".\\" + SAVE_FOLDER_NAME;

            DirectoryInfo di = new DirectoryInfo(dirName);
            if (!di.Exists)
            {
                di.Create();
            }

            FileStream fs = new FileStream(dirName + "\\" + SAVE_FILE_NAME, FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(fs, data);
            fs.Close();
        }

        public SaveData Load()
        {
            string dirName = ".\\" + SAVE_FOLDER_NAME;
            if (File.Exists(dirName + "\\" + SAVE_FILE_NAME))
            {
                FileStream fs = new FileStream(dirName + "\\" + SAVE_FILE_NAME, FileMode.Open, FileAccess.Read);
                BinaryFormatter bf = new BinaryFormatter();
                var saveData = (SaveData)bf.Deserialize(fs);
                fs.Close();

                Setting.Manager.Instance.CurrentSetting.BasicHeader = saveData.SaveSettingInfo.BasicHeader;
                Setting.Manager.Instance.CurrentSetting.AHList = saveData.SaveSettingInfo.AHList;
                DynamicManager.Instance.CurrentFolder = saveData.CurrentFolder;
                return saveData;
            }
            return null;
        }
    }
}
