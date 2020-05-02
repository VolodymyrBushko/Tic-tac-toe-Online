using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        private Socket serverSocket;
        private string ip = "192.168.0.104";
        private int port = 22000;
        private static bool myStep = false;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(IPAddress.Parse(ip), port);
            Task.Run(() => ListenServer(serverSocket));
        }

        private void ListenServer(Socket serverSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[7];
                serverSocket.Receive(buffer);

                string message = Encoding.UTF8.GetString(buffer);

                if (message != "RESTART")
                    Task.Run(() => StepServer(message));
                else
                    RestartGame();

                myStep = true;
                button1.Invoke(new Action(() => Text = "My step"));
            }
        }

        private void StepServer(string position)
        {
            Button button = Controls.OfType<Button>().First(x => x.Name.Equals(position));

            button.Invoke(new Action(() =>
            {
                button.BackColor = Color.Green;
                button.Enabled = false;
            }));
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button button = (sender as Button);

            if (button.Enabled != false && myStep)
            {
                button.BackColor = Color.Red;
                button.Enabled = false;
                byte[] buffer = Encoding.UTF8.GetBytes(button.Name);
                serverSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                myStep = false;
                button1.Invoke(new Action(() => Text = "Step of server"));
            }
        }

        private void RestartGame()
        {
            button1.Invoke(new Action(() =>
            {
                foreach (Button button in Controls.OfType<Button>())
                {
                    button.BackColor = Color.Gray;
                    button.Enabled = true;
                }
            }));
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e) => serverSocket.Close();
    }
}