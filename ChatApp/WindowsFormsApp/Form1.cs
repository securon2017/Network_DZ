using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class Form1 : Form
    {
        bool alive = false;
        static Socket clientSocket;
        const int PORT = 55_555;
        const string HOST = "127.0.0.1";

        
        string userName;
        public Form1()
        {
            InitializeComponent();

            LogInButton.Enabled = true; 
            LogOutButton.Enabled = false; 
            SendButton.Enabled = false; 
            
        }

        // вход в чат
        private void LogInButton_Click(object sender, EventArgs e)
        {
            userName = UserNameTextBox.Text;
            UserNameTextBox.ReadOnly = true;

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(HOST), PORT);
            try
            {
                clientSocket.Connect(endPoint);
                
                // запуск прослушки сообщений от сервера
                StartListening();
                string message = userName + " вошел в чат";
                SendMessage(message);
                LogInButton.Enabled = false;
                LogOutButton.Enabled = true;
                SendButton.Enabled = true;
            }
            catch (SocketException)
            {
                if (!alive)
                    return;
                throw;
            }
        }

        static void SendMessage(string message)
        {
            clientSocket.Send(ConvertStringToBytes(message));
        }

        void StartListening()
        {
            Task.Run(() =>
            {
                alive = true;
                while (true)
                {
                    try
                    {
                        byte[] buff = ReceiveMessage();
                        string message = ConvertBytesToString(buff);
                        string time = DateTime.Now.ToShortTimeString();
                        Invoke(new MethodInvoker(() =>
                        {
                            time = DateTime.Now.ToShortTimeString();
                            ChatTextBox.Text = ChatTextBox.Text + "\r\n" + time + " " + message;
                        }));
                    }
                    catch (SocketException)
                    {
                        if (!alive)
                            return;
                        Console.WriteLine();
                    }
                    
                }
            });
        }
        
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", userName, MessageTextBox.Text);
                SendMessage(message);
                MessageTextBox.Clear();
            }
            
            catch (Exception)
            {

                throw;
            }
        }

        static string ConvertBytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        byte[] ReceiveMessage()
        {           
             const int BUFF_SIZE = 100;
             byte[] buff = new byte[BUFF_SIZE];
             clientSocket.Receive(buff);
             return buff;       
        }

        static byte[] ConvertStringToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private void LogOutButton_Click(object sender, EventArgs e)
        {
            ExitChat();
        }

        private void ExitChat()
        {
            string message = userName + " покидает чат";
            SendMessage(message);
            clientSocket.Close();
            LogInButton.Enabled = true;
            LogOutButton.Enabled = false;
            SendButton.Enabled = false;
            alive = false;
        }


    }
}
