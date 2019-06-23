using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPostman.Util
{
    [Serializable]
    public sealed class DynamicManager
    {
        private static DynamicManager instance = null;

        public string CurrentFolder { get; set; }
        public string SaveProtoDir { get; set; }


        private const string RESOURCE_FOLDER_NAME = "meta";
        private static readonly string COMPILE_COMMAND = "protoc --proto_path={0} --csharp_out={1}\\" + RESOURCE_FOLDER_NAME + " {2}";

        private Dictionary<string, Type> currentTypeMap_ = new Dictionary<string, Type>();

        private DynamicManager()
        {
            CurrentFolder = ".";
            SaveProtoDir = ".";
        }

        public static DynamicManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DynamicManager();
                }
                return instance;
            }
        }

        public void DynamicCompileAndLoad()
        {
            try
            {
                var fileNameList = LoadPbNameList();
                if (fileNameList.Count == 0)
                {
                    return;
                }
                var fileNameArr = fileNameList.ToArray();
                CompilePbFormat(fileNameArr);
                DynamicCompileAndLoad(fileNameArr);
            }
            catch
            {
                throw;
            }
        }

        private List<string> LoadPbNameList()
        {
            List<string> fileNameList = new List<string>();
            DirectoryInfo di = new DirectoryInfo(this.CurrentFolder);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Extension.ToLower().CompareTo(".proto") == 0)
                {
                    fileNameList.Add(file.Name);
                }
            }

            return fileNameList;
        }

        private bool CompilePbFormat(string[] fileNameArr)
        {
            ClearResourceFile();
            ResourceFolderCreate();

            // Process Create & Start
            ProcessStartInfo proInfo = new ProcessStartInfo();
            Process pro = new Process();
            proInfo.FileName = @"cmd";
            //proInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proInfo.CreateNoWindow = false;
            proInfo.UseShellExecute = false;
            proInfo.RedirectStandardInput = true;
            proInfo.RedirectStandardOutput = true;
            proInfo.RedirectStandardError = true;

            pro.StartInfo = proInfo;
            pro.Start();

            pro.StandardInput.WriteLine(@"set");
            var output = pro.StandardOutput.ReadToEndAsync();
            var error = pro.StandardError.ReadToEndAsync();
            foreach (string fileName in fileNameArr)
            {
                string command = String.Format(COMPILE_COMMAND, CurrentFolder, SaveProtoDir, fileName);
                pro.StandardInput.WriteLine(command);
            }
            pro.StandardInput.Close();
            pro.WaitForExit();
            pro.Close();

            return true;
        }

        private void ClearResourceFile()
        {
            DirectoryInfo di = new DirectoryInfo(".\\" + RESOURCE_FOLDER_NAME);
            if (di.Exists)
            {
                Directory.Delete(".\\" + RESOURCE_FOLDER_NAME, true);
            }
        }

        private void ResourceFolderCreate()
        {
            DirectoryInfo di = new DirectoryInfo(".\\" + RESOURCE_FOLDER_NAME);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        public bool DynamicCompileAndLoad(string[] fileNameArr)
        {
            string[] sources = new string[fileNameArr.Length];
            for (int i = 0; i < fileNameArr.Length; i++)
            {
                string fileName = PBParser.PBParscalCase(fileNameArr[i].Replace(".proto", ""));
                sources[i] = File.ReadAllText(".\\" + RESOURCE_FOLDER_NAME + "\\" + fileName + ".cs");
            }
            CodeDomProvider cdp = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters cParams = new CompilerParameters();
            cParams.GenerateInMemory = true;
            cParams.ReferencedAssemblies.Add("Google.Protobuf.dll");
            CompilerResults compileResult = cdp.CompileAssemblyFromSource(cParams, sources);

            if (compileResult.Errors.Count > 0)
            {
                foreach (var err in compileResult.Errors)
                {
                    Console.WriteLine(err.ToString());
                }
                return false;
            }
            Type[] types = compileResult.CompiledAssembly.GetExportedTypes();
            foreach (var type in types)
            {
                if (type.IsAbstract || !(type.BaseType.Name == "Object"))
                {
                    continue;
                }
                currentTypeMap_[type.ToString()] = type;
            }
            return true;
        }

        public string[] GetResourceTypeArray()
        {
            string[] result = new string[currentTypeMap_.Count];
            int idx = 0;
            foreach (var pair in currentTypeMap_)
            {
                result[idx++] = pair.Key;
            }
            return result;
        }

        public Type GetTypeFromMap(string typeString)
        {
            if (!currentTypeMap_.ContainsKey(typeString))
            {
                return null;
            }
            return currentTypeMap_[typeString];
        }
    }
}
