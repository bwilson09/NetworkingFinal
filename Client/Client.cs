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
    }
}
