using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TUIO
{
    public class Client
    {
        private NetworkStream stream;
        private TcpClient client;

        // Event to notify when a message is received
        public event Action<string> MessageReceived;
        public event Action ConnectionTerminated;

        // Connect to the server socket
        public bool ConnectToSocket(string host, int portNumber)
        {
            try
            {
                client = new TcpClient(host, portNumber);
                stream = client.GetStream();
                Console.WriteLine("Connected to " + host);
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine("Connection Failed: " + e.Message);
                return false;
            }
        }

        // Start receiving messages asynchronously
        public void StartReceivingMessages()
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        string message = ReceiveMessage();
                        if (message == "q")
                        {
                            Disconnect();
                            break;
                        }
                        MessageReceived?.Invoke(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error receiving message: " + ex.Message);
                    Disconnect();
                }
            });
        }

        // Receive a single message
        private string ReceiveMessage()
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                string data = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return null;
            }
        }

        // Disconnect from the server
        public void Disconnect()
        {
            stream?.Close();
            client?.Close();
            ConnectionTerminated?.Invoke();
            Console.WriteLine("Connection Terminated!");
        }
    }
}