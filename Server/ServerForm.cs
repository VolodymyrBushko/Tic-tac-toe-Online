using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class ServerForm : Form
    {
        private Socket listenSocket;
        private Socket clientSocket;
        private int port = 22000;
        private static bool myStep = true;
        private static List<Button> buttons;

        public ServerForm()
        {
            InitializeComponent();
        }

        [Obsolete]
        private void ServerForm_Load(object sender, EventArgs e)
        {
            Text = "My step";

            buttons = new List<Button>();
            List<Button> tmp = Controls.OfType<Button>().ToList();
            for (int i = tmp.Count - 1; i >= 0; i--)
                buttons.Add(tmp[i]);

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(Dns.Resolve(SystemInformation.ComputerName).AddressList[1], port));
            listenSocket.Listen(1);
            Task.Run(() => Listen(listenSocket));

        }

        private void Listen(Socket listenSocket)
        {
            clientSocket = listenSocket.Accept();
            Task.Run(() => ReceiveClient(clientSocket));
        }

        private void ReceiveClient(Socket clientSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[7];
                clientSocket.Receive(buffer);
                Task.Run(() => StepClient(Encoding.UTF8.GetString(buffer)));
                myStep = true;
                button1.Invoke(new Action(() => Text = "My step"));
            }
        }

        private void StepClient(string position)
        {
            Button button = Controls.OfType<Button>().First(x => x.Name.Equals(position));

            button.Invoke(new Action(() =>
            {
                button.BackColor = Color.Red;
                button.Enabled = false;
            }));

            if (GameOver() == true)
                MessageBox.Show("Server win");
            else if (GameOver() == false)
                MessageBox.Show("Client win");

            RestartGame();
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button button = (sender as Button);

            if (button.Enabled != false && myStep)
            {
                button.BackColor = Color.Green;
                button.Enabled = false;
                byte[] buffer = Encoding.UTF8.GetBytes(button.Name);
                clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);

                if (GameOver() == true)
                    MessageBox.Show("Server win");
                else if (GameOver() == false)
                    MessageBox.Show("Client win");

                RestartGame();

                myStep = false;
                button1.Invoke(new Action(() => Text = "Step of client"));
            }
        }

        private static bool? VerticalLogic()
        {
            for (int i = 0; i < 3; i++)
            {
                if (buttons[i].BackColor != Color.Gray &&
                    buttons[i].BackColor == buttons[i + 3].BackColor &&
                    buttons[i].BackColor == buttons[i + 6].BackColor)
                {
                    if (buttons[i].BackColor == Color.Green)
                        return true;
                    return false;
                }
            }
            return null;
        }

        private static bool? HorizontalLogic()
        {
            for (int i = 0; i < buttons.Count; i += 3)
            {
                if (buttons[i].BackColor != Color.Gray &&
                    buttons[i].BackColor == buttons[i + 1].BackColor &&
                    buttons[i].BackColor == buttons[i + 2].BackColor)
                {
                    if (buttons[i].BackColor == Color.Green)
                        return true;
                    return false;
                }
            }

            return null;
        }

        private static bool? DiagonalLeft()
        {
            if (buttons[0].BackColor != Color.Gray &&
                buttons[0].BackColor == buttons[4].BackColor &&
                buttons[0].BackColor == buttons[8].BackColor)
            {
                if (buttons[0].BackColor == Color.Green)
                    return true;
                return false;
            }

            return null;
        }

        private static bool? DiagonalRight()
        {
            if (buttons[2].BackColor != Color.Gray &&
              buttons[2].BackColor == buttons[4].BackColor &&
              buttons[2].BackColor == buttons[6].BackColor)
            {
                if (buttons[2].BackColor == Color.Green)
                    return true;
                return false;
            }

            return null;
        }

        public static bool? GameOver()
        {
            if (VerticalLogic() == true || HorizontalLogic() == true ||
                DiagonalLeft() == true || DiagonalRight() == true)
                return true;
            else if (VerticalLogic() == false || HorizontalLogic() == false ||
                DiagonalLeft() == false || DiagonalRight() == false)
                return false;
            else
                return null;
        }

        private void RestartGame()
        {
            bool? gameOver = GameOver();

            if (gameOver == true || gameOver == false ||
                buttons.FirstOrDefault(x => x.BackColor == Color.Gray) == null)
            {
                button1.Invoke(new Action(() =>
                {
                    buttons.ToList().ForEach(x => { x.BackColor = Color.Gray; x.Enabled = true; });
                    clientSocket.Send(Encoding.UTF8.GetBytes("RESTART"));
                }));
            }
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e) => listenSocket.Close();
    }
}