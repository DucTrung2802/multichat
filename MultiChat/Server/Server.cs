using System.Net.Sockets;
using System.Net;
using System.Text.Json;

namespace Server
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (Socket item in clientList)
            {
                Send(item);
            }

            AddMessage(txbMessage.Text);

        }

        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;

        void Connect()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            server.Bind(IP);
            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();

                        clientList.Add(client);

                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }

        void Close()
        {
            server.Close();
        }

        void Send(Socket client)
        {
            if (txbMessage.Text != string.Empty)
            {
                // Thêm tên client vào tin nhắn
                string message = $"SERVER: {txbMessage.Text}";
                client.Send(Serialize(message));
            }
        }

        void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);

                    AddMessage(message);
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }


        }

        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
        }

        byte[] Serialize(object obj)
        {
            string jsonString = JsonSerializer.Serialize(obj);
            return System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        object Deserialize(byte[] data)
        {
            string jsonString = System.Text.Encoding.UTF8.GetString(data).TrimEnd('\0');
            return JsonSerializer.Deserialize<string>(jsonString);
        }

        private void txbMessage_TextChanged(object sender, EventArgs e)
        {

        }
    }
}