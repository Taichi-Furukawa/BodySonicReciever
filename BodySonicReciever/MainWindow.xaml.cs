using System;
using System.Collections.Generic;
using System.Linq;
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
using Rug.Osc;
using System.IO.Ports;

namespace BodySonicReciever
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // OSCのレシーバー
        private OscReceiver m_OscReceiver;

        // OSC受信待ちをするタスク
        private Task m_OscReceiveTask = null;

        private System.IO.Ports.SerialPort SerialPort;

        public MainWindow()
        {
            InitializeComponent();

        }

        public void LounchOscServer()
        {
            // OSCのレシーバーをポート12345で作る
            m_OscReceiver = new OscReceiver(12345);

            // OSCのレシーバーを接続
            m_OscReceiver.Connect();

            // OSC受信用のタスクを生成
            m_OscReceiveTask = new Task(() => OscListenProcess());

            // タスクをスタート
            m_OscReceiveTask.Start();
        }

        private CmdDataSet OscToDataSet(string msg)
        {
            var data = msg.Split('-');
            int[] frame = data.Select(int.Parse).ToArray();
            List<string> frameHex = new List<string>();
            for (int i=0;i<frame.Length;i++)
            {
                frameHex.Add(frame[i].ToString("X2"));
            }
            byte a = Convert.ToByte(frameHex[0], 16);
            byte b = Convert.ToByte(frameHex[1], 16);
            byte c = Convert.ToByte(frameHex[2], 16);
            byte d = Convert.ToByte(frameHex[3], 16);
            byte e = Convert.ToByte(frameHex[4], 16);
            CmdDataSet senddata = new CmdDataSet(Convert.ToByte(frameHex[0], 16), Convert.ToByte(frameHex[1], 16), Convert.ToByte(frameHex[2], 16),Convert.ToByte(frameHex[3], 16),Convert.ToByte(frameHex[4], 16));

            return senddata;
        }

        private void SetComportBox()
        {
            ComportNameBox.Items.Clear();

            string[] ports = SerialPort.GetPortNames();

            if (ports.Count() > 0)
            {
                Array.Sort(ports);

                foreach (string port in ports)
                {
                    ComportNameBox.Items.Add(port);
                }
                ComportNameBox.SelectedIndex = 0;
            }
        }

        private void OscListenProcess()
        {
            try
            {
                // OSCレシーバーが終了されるまで繰り返し処理する
                while (m_OscReceiver.State != OscSocketState.Closed)
                {
                    // 受信待ち(メッセージを受信したら処理が帰ってくる)
                    OscPacket packet = m_OscReceiver.Receive();
                    // 受信したメッセージをコンソールに出力
                    Console.WriteLine(packet.ToString());
                    string msgText = packet.ToString();

                    //msgbox.Text = "hoge";
                    msgText = msgText.Remove(0,1);
                    Console.WriteLine(msgText);

                    CmdDataSet cmdDataSet = OscToDataSet(msgText);
                    byte[] cmdFrame = cmdDataSet.getFrame();
                    Console.WriteLine("{0} : " + BitConverter.ToString(cmdFrame));
                    
                    try
                    {
                        SerialPort.Write(cmdFrame, 0, cmdFrame.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                // 例外処理　発生時はコンソールに出力
                // 　ただし
                //    m_OscReceiver.Receive()で受信待ち状態の時に終了処理(m_OscReceiver.close())をすると
                //    正しい処理でもExceptionnとなるため、接続中かで正しい処理か例外かを判断する
                if (m_OscReceiver.State == OscSocketState.Connected)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {

            bool isErr = false;

            try
            {
                if (SerialPort == null)
                { 
                    SerialPort = new SerialPort();
                    SerialPort.PortName = ComportNameBox.SelectedItem.ToString();
                    SerialPort.BaudRate = 115200;
                    SerialPort.Parity = Parity.None;
                    SerialPort.StopBits = StopBits.One;
                    SerialPort.Encoding = Encoding.ASCII;
                    SerialPort.Open();
                    Console.WriteLine("Connect!");
                    LounchOscServer();
                }
                else
                {
                    SerialPort.Close();
                }

            }
            catch (Exception ex_l)
            {
                isErr = true;
                MessageBox.Show(ex_l.Message, "エラー");
            }
            finally
            {
                //setSerialStsText(SerialPort.IsOpen, isErr);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComportNameBox_MouseEnter(object sender, MouseEventArgs e)
        {
            SetComportBox();
        }

        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
            SerialPort.Close();
        }
    }


    class CmdDataSet
    {
        private byte gid;
        private byte nid;
        private byte sid;
        private byte strength;
        private byte lid;

        public CmdDataSet(byte gid, byte nid, byte sid, byte strength, byte lid)
        {
            this.gid = gid;
            this.nid = nid;
            this.sid = sid;
            this.strength = strength;
            this.lid = lid;
        }

        public byte[] getFrame()
        {
            byte[] frame = new byte[5];

            frame[0] = 0xF2;
            frame[1] = (byte)((int)gid << 4);
            frame[1] += nid;
            frame[2] = sid;
            frame[3] = strength;
            frame[4] = lid;

            return frame;
        }
    }
}
