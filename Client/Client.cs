using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
                        Login();
                        break;
                        case "2":
                        //account creation flow
                        CreateAccount();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please choose either 1 or 2.");
                        break;
                }
            }
        }

        //handles login process
        private void Login()
        {
            Console.WriteLine("\n--- Login ---");

            //get clients account number
            Console.WriteLine("Enter your account number: ");
            string accountNumber= Console.ReadLine();

            //client enters password
            Console.WriteLine("Enter your password: ");
            string password = Console.ReadLine();

            //build the login message
            //serverside will validate all login process stuff
            string message = $"LOGIN|{accountNumber}|{password}";

            //send this message to server
            SendMessage(message);

            //wait for servers response
            string response = ReceiveMessage();

            Console.WriteLine("\nServer Response: ");
            Console.WriteLine(response);

            //if login was successful take client to the logged in menu
            if(response.Contains("Succesfully logged in"))
            {
                LoggedInMenu(accountNumber);
            }

        }

        private void LoggedInMenu(string? accountNumber)
        {
            while (true)
            {
                Console.WriteLine("\n--- Account Menu ---");
                Console.WriteLine("1. Check Balance");
                Console.WriteLine("2. Deposit Cheque");
                Console.WriteLine("3. Withdraw Amount");
                Console.WriteLine("4. Transfer Amount");
                Console.WriteLine("5. Logout");

                Console.WriteLine("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice) 
                {
                    case "1":
                        //check balance
                        CheckBalance(accountNumber);
                        break;
                        case "2":
                        //deposit cheque
                        Deposit(accountNumber);
                        break;
                        case "3":
                        //withdraw
                        Withdraw(accountNumber);
                        break;
                        case "4":
                        //transfer
                        Transfer(accountNumber);
                        break;
                        case "5":
                        Console.WriteLine("Logging out...");
                        return;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter a number from 1-5.");
                        break;
                }

            }
        }

        private void Transfer(string? accountNumber)
        {
            throw new NotImplementedException();
        }

        private void Withdraw(string? accountNumber)
        {
            Console.WriteLine("\n--- Withdraw ---");

            //ask for amount wanting to withdraw
            Console.WriteLine("Enter amount to withdraw: ");
            string amount = Console.ReadLine();

            //send message to server
            string message = $"WITHDRAW|{accountNumber}|{amount}";
            SendMessage(message);

            //get server response
            string response = ReceiveMessage();

            //show servers reponse
            Console.WriteLine("\nServer Response: \n" + response);
        }

        private void Deposit(string? accountNumber)
        {
            Console.WriteLine("\n--- Deposit a Cheque ---");

            //ask for amount to deposit
            Console.WriteLine("Enter cheque amount to deposit: ");
            string amount = Console.ReadLine();

            //create message to send to the server
            string message = $"DEPOSIT|{accountNumber}|{amount}";
            SendMessage(message);

            //get server response
            string response = ReceiveMessage();

            //show servers reponse
            Console.WriteLine("\nServer Response: \n" + response);
        }

        private void CheckBalance(string? accountNumber)
        {
            Console.WriteLine("\n--- Check Balance ---");

            //send the message to the server
            string message = $"BALANCE|{accountNumber}";
            SendMessage(message);

            //get response and show to client
            string response = ReceiveMessage();

            Console.WriteLine("\nServer Response: \n" + response);
            
        }

        //handles the create account process
        private void CreateAccount()
        {
            Console.WriteLine("\n--- Create New Account ---");

            //generate the IDs
            string accountNumber = GenerateAccountNumber();
            string refNumber = GenerateReferenceNumber();

            //ask client for a password
            Console.WriteLine("Enter a password: ");
            string password = Console.ReadLine();

            //create message to send to the server
            string message = $"CREATE|{accountNumber}|{refNumber}|{password}";

            //send the message to the server
            SendMessage(message);

            //wait for the servers response
            string response = ReceiveMessage();

            //show the response
            Console.WriteLine("\nServer Response: ");
            Console.WriteLine(response);

            //show client their account and refernce number
            Console.WriteLine($"\nYour account number is: {accountNumber} \nYou will need this number to login, please store it safely.");
            Console.WriteLine($"\nYour reference number is: {refNumber}");
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
