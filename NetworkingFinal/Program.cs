using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace NetworkingFinal
{
    public class BankServer
    {
        private static string path = Path.Combine(AppContext.BaseDirectory, "AppData", "accountStarter.json");

        public static List<Account> Accounts = new List<Account>();
        public static int Main(string[] args)
        {
            LoadAccounts();
            StartServer();
            return 0;
        }


        //server starterup:
        public static void StartServer()
        {
            try
            {
                IPAddress ip = IPAddress.Parse("192.168.219.19");
                IPEndPoint localIP = new IPEndPoint(ip, 10001);

                Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localIP);
                listener.Listen(10); //  10 is the # of connections the server can handle

                //wait for connection
                Console.WriteLine("Waiting for a connection... ");
                //create new socket: when listener recieves a connection coming in, assign to an object called handler that will recieve/process info
                Socket handler = listener.Accept();

                while (true)
                {
                    string data = null;
                    byte[] bytes = null;
                    string response = null;

                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Text Received : {0}", data);

                    //trim data string to remove the EOF tag and any whitespace in order to parse the values before calling processcommand function
                    string trimmedData = data.Replace("<EOF>", "").Trim();
                    response = ProcessCommand(trimmedData);

                    byte[] msg = Encoding.ASCII.GetBytes(response);
                    handler.Send(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static string ProcessCommand(string command)
        {
            //splitting the user's input into parts using the | as a delimiter
            string[] parts = command.Split('|');
            //grab the first "part" of the command and determine the action and which method to call
            string action = parts[0].ToUpper();

            //calling whatever method is needed and sending in the parts as arguments
            switch (action)
            {
                case "LOGIN":
                    return HandleLogin(parts);

                case "CREATE":
                    return HandleCreate(parts);

                case "BALANCE":
                    return HandleBalance(parts);

                case "DEPOSIT":
                    return HandleDeposit(parts);

                case "WITHDRAW":
                    return HandleWithdraw(parts);

                case "TRANSFER":
                    return HandleTransfer(parts);

                default:
                    return "Error: Invalid input. Please try again.";
            }
        }

        private static string HandleTransfer(string[] parts)
        {
            //sending format = "TRANSFER|{accountNumber}|{toAccount}|{amount}|{token}"
            throw new NotImplementedException();
        }

        private static string HandleWithdraw(string[] parts)
        {
            //sending format = "WITHDRAW|{accountNumber}|{amount}"
            throw new NotImplementedException();
        }

        private static string HandleDeposit(string[] parts)
        {
            //sending format = "DEPOSIT|{accountNumber}|{cheque}|{amount}"
            
            string accountNumber= parts[1];
            string chequeNumber = parts[2];
            string amountString = parts[3];

            //validate the amount
            if(!decimal.TryParse(amountString, out decimal amount) || amount <= 0)
            {
                return "ERROR: Invalid deposit amount.";

            }

            //find the corresponding account
            var account = Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                return "ERROR: Account not found.";
            }

            //add the deposited amount to their account
            account.Balance += amount;

            //save the update balance to the file
            SaveAccount();

            //send back success message
            return "SUCCESS: Deposit successful.";

        }

        private static string HandleBalance(string[] parts)
        {
            //sending format = "BALANCE|{accountNumber}"
            throw new NotImplementedException();
        }

        public static string HandleLogin(string[] parts)
        {
            //sending format = "LOGIN|{accountNumber}|{password}"

            string accountNumber = parts[1];

            string password = parts[3];

            //check the list of accounts for matching account number
            var user = Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (user == null)
                return "ERROR: Invalid account number";

           

            //if a valid account and reference number is returned, then proceed to check the password
            if (user.Password != password)
                return "ERROR: Invalid password";

            //if all checks are passed, then log the user in by setting IsLoggedIn to true
            user.IsLoggedIn = true;
            return "SUCCESS: Successfully logged in";
            
        }



        public static string HandleCreate(string[] parts)
        {
            //sending in the following argument:
            //$"CREATE|{accountNumber}|{referenceNumber}|{password}"

            string accountNumber = parts[1];
            string referenceNumber = parts[2];
            string password = parts[3];

            if (!Accounts.Any(a => a.ReferenceNumber == referenceNumber))
                return "ERROR: Invalid reference number.";

            Accounts.Add(new Account
            {
                AccountNumber = accountNumber,
                Password = password,
                ReferenceNumber = referenceNumber,
                Balance = 0
            });


            // How are we saving accounts? Writing to a file?
            SaveAccount();

            return $"SUCCESS: Account created. Number: {accountNumber}, Reference: {referenceNumber}";
        }


        public static void SaveAccount()
        {
            //line to check/create the folder
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string json = JsonSerializer.Serialize(Accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }


        public static void LoadAccounts()
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Accounts = JsonSerializer.Deserialize<List<Account>>(json, options);
                Console.WriteLine("Loaded accounts: " + Accounts.Count);
            }
            else
            {
                Console.WriteLine("ERROR: Accounts file not found at " + path);
            }
        }














    }
}