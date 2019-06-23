using Google.Protobuf;
using SocketPostman.Setting;
using SocketPostman.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketPostman
{
    public sealed class Network
    {
        private ConnectEvent connectEvent;
        private ReceivePacketEvent recvEvent;

        private static Network instance = null;
        private Socket client;
        private readonly byte[] receiveBuffer = new byte[MAX_BUFFER_SIZE];
        private const int MAX_BUFFER_SIZE = 1024;

        public int HeaderSize
        {
            get
            {
                HeadLastElem maxIndex = CalculateMaxIndex();
                int headerSize = 0;
                if (maxIndex.DIndex != IndexDataType.Select)
                {
                    headerSize += maxIndex.Index + Common.TypeToSizeMap[maxIndex.DIndex];
                }
                return headerSize;
            }
        }

        public struct RecvPacket
        {
            public string Display;
            public byte[] Data;
        }
        private Network()
        {

        }

        private struct HeadLastElem
        {
            public int Index;
            public IndexDataType DIndex;
        }

        public static Network Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Network();
                }
                return instance;
            }
        }

        public void InitDelegate(ConnectEvent connectEvent, ReceivePacketEvent recvEvent)
        {
            this.connectEvent = connectEvent;
            this.recvEvent = recvEvent;
        }

        public void Connect(IPEndPoint ipep)
        {
            try
            {
                if (client != null)
                {
                    throw new Exception("Already Connected State!");
                }
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(ipep);

                connectEvent(true);

                Receive();
            }
            catch (Exception ex)
            {
                Disconnect();
                throw ex;
            }
        }

        public void Disconnect()
        {
            if (client == null)
            {
                return;
            }
            client.Close();
            client = null;
            connectEvent(false);
        }

        public void Send(byte[] data)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("Client is not connected!");
                }
                client.Send(data);
            }
            catch
            {
                throw;
            }
        }

        private void Receive()
        {
            try
            {
                client.BeginReceive(receiveBuffer, 0, MAX_BUFFER_SIZE, 0, ReceiveCallback, client);
            }
            catch
            {
                throw;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var arSocket = (Socket)ar.AsyncState;
            if (client == null)
            {
                return;
            }
            if (arSocket == null)
            {
                Disconnect();
                return;
            }
            var readBytes = arSocket.EndReceive(ar);

            if (readBytes <= 0)
            {
                Disconnect();
                return;
            }
            byte[] recvBytes = new byte[readBytes];
            Buffer.BlockCopy(receiveBuffer, 0, recvBytes, 0, readBytes);
            PacketEvent(recvBytes);
            Receive();

        }

        private void PacketEvent(byte[] data)
        {
            string displayString = "Receive Packet! Size: " + data.Length.ToString();
            var basicHeader = Setting.Manager.Instance.CurrentSetting.BasicHeader;
            if (basicHeader.CheckedPacketID)
            {
                var packetID = Util.PBParser.PeekValueFromData(data, basicHeader.DIndexPacketID, basicHeader.IndexPacketID);
                displayString = String.Format("[{0}]: PacketID: {1}, {2}", DateTime.Now.ToString("HH:mm:ss"), packetID, displayString);
            }
            else
            {
                displayString = String.Format("[{0}]: PacketID: {1}", DateTime.Now.ToString("HH:mm:ss"), displayString);
            }
            RecvPacket recv;
            recv.Display = displayString;
            recv.Data = data;
            recvEvent(recv);
        }


        public byte[] MakePacket(int packetID, IMessage message)
        {
            var basicHeader = Setting.Manager.Instance.CurrentSetting.BasicHeader;
            var ahList = Setting.Manager.Instance.CurrentSetting.AHList;
            HeadLastElem maxIndex = CalculateMaxIndex();
            int headerSize = 0;
            if (maxIndex.DIndex != IndexDataType.Select)
            {
                headerSize += maxIndex.Index + Common.TypeToSizeMap[maxIndex.DIndex];
            }
            
            byte[] body = message.ToByteArray();
            int totalSize = headerSize + body.Length;
            byte[] result = new byte[totalSize];

            if (basicHeader.CheckedPacketSize)
            {
                var bytes = PBParser.HeaderToByteArray(basicHeader.DIndexPacketSize, totalSize);
                Buffer.BlockCopy(bytes, 0, result, basicHeader.IndexPacketSize, Common.TypeToSizeMap[basicHeader.DIndexPacketSize]);
            }
            if (basicHeader.CheckedPacketID)
            {
                var bytes = PBParser.HeaderToByteArray(basicHeader.DIndexPacketID, packetID);
                Buffer.BlockCopy(bytes, 0, result, basicHeader.IndexPacketID, Common.TypeToSizeMap[basicHeader.DIndexPacketID]);
            }
            foreach (var ah in ahList)
            {
                var bytes = PBParser.HeaderToByteArray(ah.DataType, ah.Value);
                Buffer.BlockCopy(bytes, 0, result, ah.StartIndex, Common.TypeToSizeMap[ah.DataType]);
            }
            Buffer.BlockCopy(body, 0, result, headerSize, body.Length);
            Console.WriteLine(result);
            return result;
        }

        private HeadLastElem CalculateMaxIndex()
        {
            HeadLastElem result;
            result.Index = 0;
            result.DIndex = IndexDataType.Select ;
            var settingInfo = Setting.Manager.Instance.CurrentSetting;
            if (settingInfo.BasicHeader.CheckedPacketSize)
            {
                if (settingInfo.BasicHeader.IndexPacketSize > result.Index)
                {
                    result.Index = settingInfo.BasicHeader.IndexPacketSize;
                    result.DIndex = settingInfo.BasicHeader.DIndexPacketSize;
                }
            }
            if (settingInfo.BasicHeader.CheckedPacketID)
            {
                if (settingInfo.BasicHeader.IndexPacketID > result.Index)
                {
                    result.Index = settingInfo.BasicHeader.IndexPacketID;
                    result.DIndex = settingInfo.BasicHeader.DIndexPacketID;
                }
            }
            foreach (var ah in settingInfo.AHList)
            {
                if (ah.StartIndex > result.Index)
                {
                    result.Index = ah.StartIndex;
                    result.DIndex = ah.DataType;
                }
            }
            return result;
        }
    }
}
