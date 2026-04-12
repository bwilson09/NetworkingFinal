using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BankClient
{
    public class Client
    {
        //connection to the server
        private TcpClient _client;
        //how we send and receive data
        private NetworkStream _stream;

        //program.cs calls this method for client logic
        public void Run()
        {
            Console.WriteLine("Enter the server IP address:");
            string ip = Console.ReadLine();

            //try to connect to the server using the ip provided
            ConnectToServer(ip);

            //if theres a failed connection then stop the program
            if (_client == null || !_client.Connected)
            {
                Console.WriteLine("Could not connect. Exiting program...");
                return;
            }

            //load the main menu for client logic
            MainMenu();
        }

        //the main menu loop
        private void MainMenu()
        {
           while (true)
            {
                Console.WriteLine("\n--- Welcome to the Main Menu ---");
                Console.WriteLine("1. Log in");
                Console.WriteLine("2. Create New Account");

                Console.WriteLine("Please choose an option: ");
                string choice = Console.ReadLine();

                switch(choice)
                {
                    case "1":
                        //login flow
                        break;
                        case "2":
                        //account creation flow
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please choose either 1 or 2.");
                        break;
                }
            }
        }

        //method to handle the socket connection
        private void ConnectToServer(string? ip)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(ip, 10001);
                _stream = _client.GetStream();
                Console.WriteLine("Connected to server!");
            }
            catch(Exception ex)
            { 
                Console.WriteLine("Connection failed: " + ex.ToString());
            }
        }

        //for sending a message string to the server over the network stream
        private void SendMessage(string message)
        {
            //convert message to bytes
            byte[] data = Encoding.UTF8.GetBytes(message);

            //weite the bits to the stream
            _stream.Write(data, 0, data.Length);
        }

        //reads the response from the server
        private string ReceiveMessage()
        {
            //buffer is the temporary storage for incoming bytes
            byte[] buffer = new byte[1024];

            int bytesRead= _stream.Read(buffer, 0, buffer.Length);

            //convert the bytes into a string
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        //generate a radom, unique 9-digit account number
        private string GenerateAccountNumber()
        {
            Random rand = new Random();
            return rand.Next(100000000, 999999999).ToString();
        }

        //generate a unique 7-digit alphanumeric reference code
        private string GenerateReferenceNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random rand = new Random();
            char[] buffer = new char[7];

            for (int i = 0; i < chars.Length; i++)
            {
                buffer[i] = chars[rand.Next(chars.Length)];
            }

            return new string(buffer);
        }
    }
}
