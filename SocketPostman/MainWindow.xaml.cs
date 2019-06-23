using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketPostman.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SocketPostman
{
    public delegate void ReceivePacketEvent(Network.RecvPacket packet);
    public delegate void ConnectEvent(bool isConnect);
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private List<AdditionalHeader> ahList = new List<AdditionalHeader>();
        private List<Network.RecvPacket> recvPacketList = new List<Network.RecvPacket>();
        private const string
            Disconnect = "Disconnect",
            Connect = "Connect";

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Windows Event
        private void Window_Loaded(object sender, RoutedEventArgs e)        
        {
            ahGrid.ItemsSource = ahList;
            LoadSavedData();
            Network.Instance.InitDelegate(ConnectedUIChange, ReceivePacketProcess);
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CurrentStateSave();
        }
        #endregion

        private void ConnectedUIChange(bool isConnect)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new ConnectEvent(ConnectedUIChange), isConnect);
                return;
            }
            if (isConnect)
            {
                connectBtn.Content = Disconnect;
                sendBtn.IsEnabled = true;
            }
            else
            {
                connectBtn.Content = Connect;
                sendBtn.IsEnabled = false;
            }
        }

        private void ReceivePacketProcess(Network.RecvPacket packet)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new ReceivePacketEvent(ReceivePacketProcess), packet);
                return;
            }

            recvPacketList.Add(packet);
            resListBox.Items.Add(packet.Display);
        }

        private void LoadSavedData()
        {
            var saveData = Util.SaveManager.Instance.Load();
            if (saveData != null)
            {
                // UI Change
                hostTextBox.Text = saveData.SaveHost;
                portTextBox.Text = saveData.SavePort;
                LoadAndCompile();
                if (Setting.Manager.Instance.CurrentSetting.BasicHeader.CheckedPacketID)
                {
                     sendPIDTextBox.IsEnabled = true;
                }

                var basicHeader = Setting.Manager.Instance.CurrentSetting.BasicHeader;
                ahList = Setting.Manager.Instance.CurrentSetting.AHList;

                totalSizeChkBox.IsChecked = basicHeader.CheckedPacketSize;
                packetIDChkBox.IsChecked = basicHeader.CheckedPacketID;
                totalSizeTextBox.Text = basicHeader.IndexPacketSize.ToString();
                packetIDTextBox.Text = basicHeader.IndexPacketID.ToString();
                totalSizeComboBox.SelectedIndex = (int)basicHeader.DIndexPacketSize;
                packetIDComboBox.SelectedIndex = (int)basicHeader.DIndexPacketID;

                ahGrid.ItemsSource = ahList;
                ahGrid.Items.Refresh();
            }
        }

        private void LoadAndCompile()
        {
            try
            {
                Util.DynamicManager.Instance.DynamicCompileAndLoad();
                var typeArr = Util.DynamicManager.Instance.GetResourceTypeArray();

                fmtListBox.Items.Clear();
                resFmtComboBox.Items.Clear();
                foreach (var type in typeArr)
                {
                    fmtListBox.Items.Add(type);
                    resFmtComboBox.Items.Add(type);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void NumericFilter(object sender, TextCompositionEventArgs e)
        {
            char c = e.Text.ToCharArray().First();
            e.Handled = !(char.IsNumber(c) || char.IsControl(c));
        }

        private void HeaderAddBtn_Click(object sender, RoutedEventArgs e)
        {
            ahList.Add(new AdditionalHeader("", 0, 0, IndexDataType.Select));
            ahGrid.Items.Refresh();
                
        }

        private void HeaderDelBtn_Click(object sender, RoutedEventArgs e)
        {
            ahList.RemoveAt(ahGrid.SelectedIndex);
            ahGrid.Items.Refresh();
        }

        private void TotalSizeChkBox_Change(object sender, RoutedEventArgs e)
        {
            Console.WriteLine((bool)totalSizeChkBox.IsChecked);
            totalSizeTextBox.IsEnabled = (bool)totalSizeChkBox.IsChecked;
            totalSizeComboBox.IsEnabled = (bool)totalSizeChkBox.IsChecked;
        }

        private void PacketIDChkBox_Change(object sender, RoutedEventArgs e)
        {
            packetIDTextBox.IsEnabled = (bool)packetIDChkBox.IsChecked;
            packetIDComboBox.IsEnabled = (bool)packetIDChkBox.IsChecked;
            sendPIDTextBox.IsEnabled = (bool)packetIDChkBox.IsChecked;
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)connectBtn.Content == Disconnect)
                {
                    Network.Instance.Disconnect();
                    ConnectedUIChange(false);
                    return;
                }

                string address = hostTextBox.Text.Trim();
                string portString = portTextBox.Text.Trim();
                var port = int.Parse(portString);
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(address), port);
                Network.Instance.Connect(ipep);
                ConnectedUIChange(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FormatBtn_Click(object sender, RoutedEventArgs e)
        {
            // Select And Change Current Directory
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = Util.DynamicManager.Instance.CurrentFolder;
            dialog.ShowDialog();
            Util.DynamicManager.Instance.CurrentFolder = dialog.SelectedPath;

            LoadAndCompile();
        }

        private void ParseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (resListBox.Items.Count == 0)
            {
                MessageBox.Show("Response Packet not exists");
                return;
            }

            if (resFmtComboBox.SelectedItem == null)
            {
                MessageBox.Show("Response Message Type is not selected");
                return;
            }

            string typeString = resFmtComboBox.SelectedItem.ToString();
            var resIndex = resListBox.SelectedIndex;

            var parseResult = Util.PBParser.ByteStreamToResult(typeString, recvPacketList[resIndex].Data);
            resTextBox.Text = parseResult;
        }

        
        private void FmtListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (fmtListBox.SelectedItem == null)
            {
                return;
            }
            try
            {
                var jsonString = Util.PBParser.TypeStringToJson(fmtListBox.SelectedItem.ToString());
                reqTextBox.Text = JToken.Parse(jsonString).ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (fmtListBox.SelectedItem == null)
                {
                    throw new Exception("Please Select Message Type");
                }

                ApplyCurrentState();
                var message = Util.PBParser.JsonToMessage(reqTextBox.Text, fmtListBox.SelectedItem.ToString());
                int packetID = int.Parse(sendPIDTextBox.Text);
                var packet = Network.Instance.MakePacket(packetID, message);
                Network.Instance.Send(packet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyCurrentState()
        {
            int totalSizeIndex = 0;
            int packetIDIndex = 0;
            if (totalSizeTextBox.Text != "")
            {
                totalSizeIndex = int.Parse(totalSizeTextBox.Text);
            }
            if (packetIDTextBox.Text != "")
            {
                packetIDIndex = int.Parse(packetIDTextBox.Text);
            }
            Setting.Manager.Instance.CurrentSetting.BasicHeader = new BasicHeaderInfo((bool)totalSizeChkBox.IsChecked, (bool)packetIDChkBox.IsChecked,
                totalSizeIndex, packetIDIndex,
                (IndexDataType)totalSizeComboBox.SelectedIndex, (IndexDataType)packetIDComboBox.SelectedIndex);

            Setting.Manager.Instance.CurrentSetting.AHList = new List<AdditionalHeader>();
            foreach (var item in ahGrid.Items)
            {
                var ah = (AdditionalHeader)item;
                if (ah.Name == "" || ah.DataType == IndexDataType.Select)
                {
                    continue;
                }
                Setting.Manager.Instance.CurrentSetting.AHList.Add(ah);
            }
        }

        private void CurrentStateSave()
        {
            ApplyCurrentState();
            var saveData = new Util.SaveManager.SaveData(
                Setting.Manager.Instance.CurrentSetting,
                Util.DynamicManager.Instance.CurrentFolder,
                hostTextBox.Text,
                portTextBox.Text);
            Util.SaveManager.Instance.Save(saveData);
        }
    }
}
