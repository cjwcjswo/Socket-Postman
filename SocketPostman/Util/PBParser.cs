using Google.Protobuf;
using SocketPostman.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPostman.Util
{
    public static class PBParser
    {
        private static JsonFormatter jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));

        public static string PBParscalCase(string word)
        {
            string result = "";
            string[] splitResult = word.Split('_');
            foreach (string w in splitResult)
            {
                result += w[0].ToString().ToUpper() + w.Substring(1);
            }
            return result;
        }

        public static IMessage JsonToMessage(string json, string typeString)
        {
            IMessage result = null;
            try
            {
                var messageType = DynamicManager.Instance.GetTypeFromMap(typeString);
                if (messageType == null)
                {
                    throw new Exception("Unknown Type: " + typeString);
                }

                var msgObject = ConverTypeToMessage(messageType);
                msgObject = JsonParser.Default.Parse(json, msgObject.Descriptor);
                result = (IMessage)msgObject;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static long PeekValueFromData(byte[] data, IndexDataType sIndex, int index)
        {
            switch (sIndex)
            {
                case IndexDataType.Int8:
                    {
                        return data[index];
                    }
                case IndexDataType.Int16:
                    {
                        return BitConverter.ToInt16(data, index);
                    }
                case IndexDataType.Int32:
                    {
                        return BitConverter.ToInt32(data, index);
                    }
                case IndexDataType.Int64:
                    {
                        return BitConverter.ToInt64(data, index);
                    }
            }
            return -1;
        }

        public static byte[] HeaderToByteArray(IndexDataType dIndex, int value)
        {
            byte[] result = null;
            switch (dIndex)
            {
                case IndexDataType.Int8:
                    {
                        result = BitConverter.GetBytes((byte)value);
                        break;
                    }
                case IndexDataType.Int16:
                    {
                        result = BitConverter.GetBytes((Int16)value);
                        break;
                    }
                case IndexDataType.Int32:
                    {
                        result = BitConverter.GetBytes((Int32)value);
                        break;
                    }
                case IndexDataType.Int64:
                    {
                        result = BitConverter.GetBytes((Int64)value);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return result;
        }

        public static string ByteStreamToResult(string typeString, byte[] data)
        {
            try
            {
                string result = "";

                // 1. Parsing Header Data And Add Result
                var settingInfo = Setting.Manager.Instance.CurrentSetting;
                foreach (var ah in settingInfo.AHList)
                {
                    var headerValue = PeekValueFromData(data, ah.DataType, ah.StartIndex);
                    result += String.Format("[{0}]: {1}{2}", ah.Name, headerValue, Environment.NewLine);
                }

                // 2. Create Message
                var messageType = DynamicManager.Instance.GetTypeFromMap(typeString);
                if (messageType == null)
                {
                    throw new Exception("Unknown Type: " + typeString);
                }
                var message = ConverTypeToMessage(messageType);

                // 3. Parsing Body Data And Add Result
                var headerSize = Network.Instance.HeaderSize;
                var bodyData = new byte[data.Length - headerSize];
                Buffer.BlockCopy(data, headerSize, bodyData, 0, data.Length - headerSize);
                message.Descriptor.Parser.ParseFrom(bodyData);

                result += jsonFormatter.Format(message);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static string TypeStringToJson(string typeString)
        {
            try
            {
                var messageType = DynamicManager.Instance.GetTypeFromMap(typeString);
                if (messageType == null)
                {
                    throw new Exception("Unknown Type: " + typeString);
                }
                var message = ConverTypeToMessage(messageType);
                return jsonFormatter.Format(message);
            }
            catch
            {
                throw;
            }
        }

        private static IMessage ConverTypeToMessage(Type type)
        {
            var messageObj = Activator.CreateInstance(type);
            return (IMessage)messageObj;
        }
    }
}
