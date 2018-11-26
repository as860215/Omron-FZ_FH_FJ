using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace OmronFZ_FH_FJ_{
    public partial class Form1 : Form{
        Socket Sck; // 先行宣告Socket
        string SckIp = "10.5.6.100";
        string SckRec = "";
        List<string> tem_Rec = new List<string>();
        int show_count = 0;
        int SckPort = 9876;
        int RDataLen = 15;  // 以長度為15傳送資料
        public Form1(){
            InitializeComponent();
            tb_ip.Text = SckIp;
            tb_port.Text = SckPort.ToString();
        }
        private void btn_connect_Click(object sender, EventArgs e){
            if(btn_connect.Text == "Connect"){
                lb_status.Text = "Start connect...";
                if(PingIP()) ConnectServer();
                else{
                    lb_status.Text = "IP error.\nPlease check \nand try again.";
                    return;
                }
                if (Sck.Connected == true){
                    lb_status.Text = "Connecting \nsuccess.";
                    btn_photo.Enabled = true;
                    btn_connect.Text = "Disconnect";
                }
                else lb_status.Text = "Connecting \nfailed.";
            }
            else{
                btn_photo.Enabled = false;
                Sck.Close();
                btn_connect.Text = "Connect";
            }
        }
        /// <summary>
        /// 利用IPAddress屬性配合Ping進行遠端Server的確認。
        /// </summary>
        /// <returns></returns>
        public bool PingIP(){
            IPAddress tIP = IPAddress.Parse(tb_ip.Text);
            Ping tPingControl = new Ping();
            PingReply tReply = tPingControl.Send(tIP);
            tPingControl.Dispose();
            if (tReply.Status != IPStatus.Success)
                return false;
            else
                return true;
        }
    // 連線
    private void ConnectServer(){
            try{
                Sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Sck.SendTimeout = 2000;
                Sck.Connect(new IPEndPoint(IPAddress.Parse(tb_ip.Text), int.Parse(tb_port.Text)));
                // RmIp和SPort分別為string和int型態, 前者為Server端的IP, 後者為Server端的Port
                // 同 Server 端一樣要另外開一個執行緒用來等待接收來自 Server 端傳來的資料, 與Server概念同
                Sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
                Sck.ReceiveTimeout = 0;
                Thread SckSReceiveTd = new Thread(SckSReceiveProc);
                SckSReceiveTd.Start();
            }
            catch { }
        }
        private void SckSReceiveProc(){
            try{
                long IntAcceptData;
                byte[] clientData = new byte[RDataLen];
                while (true){
                    // 程式會被 hand 在此, 等待接收來自 Server 端傳來的資料
                    IntAcceptData = Sck.Receive(clientData);
                    // 接收到來自Server端的資料”
                    SckRec = Encoding.Default.GetString(clientData);
                    string[] tem_split = SckRec.Split('\r');
                    tem_split = tem_split[0].Split(',');
                    tem_split = tem_split[0].Split(' ');
                    foreach (string s in tem_split){
                        if (s != ""){
                            SckRec = s;
                            break;
                        }
                    }
                    tem_Rec.Add(SckRec);
                    show_count++;
                    //接收Socketc回傳訊息後執行指令
                    clientData = new byte[RDataLen];
                }
            }
            catch(Exception ex) {
                //MessageBox.Show(ex.ToString());
            }
        }
        private void SckSSend(string send){
            try{
                Sck.Send(Encoding.ASCII.GetBytes(send));
            }
            catch { }
        }
        private void btn_photo_Click(object sender, EventArgs e){
            tem_Rec.Clear();
            tb_get.Text = "";
            timer_get.Enabled = true;
            SckSSend("m");
        }
        private void timer_get_Tick(object sender, EventArgs e){
            if (tem_Rec.Count > 0){
                foreach (string s in tem_Rec) tb_get.Text += s + "\r\n";
                tem_Rec.Clear();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e){
            if(Sck!=null) Sck.Close();
        }
    }
}
