using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace NetworkingFinal
{
    public class BankServer
    {
        private static string path = Path.Combine(
        Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName,
        "AppData",
        "accountStarter.json");

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
                IPAddress ip = IPAddress.Parse("10.0.0.80");
                //IPAddress ip = IPAddress.Parse("127.0.0.1");
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

                    byte[] msg = Encoding.ASCII.GetBytes(response + "<EOF>");
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

            //validate for input length
            if (parts.Length < 5)
                return "Error: Invalid input. Please try again.";

            //extract values from paramters being sent in
            string fromAccountNumber = parts[1].Trim();
            string toAccountNumber = parts[2].Trim();
            string amount = parts[3].Trim();
            string token = parts[4] ?? string.Empty;

            //validatation for values being sent in
            if (string.IsNullOrEmpty(fromAccountNumber) || string.IsNullOrEmpty(toAccountNumber))
                return "Error: Please check account numbers.";

            //can't transfer to the same account
            if (fromAccountNumber == toAccountNumber)
                return "Error: Cannot transfer funds to the same account.";

            //cannot transfer =< 0
            if (!decimal.TryParse(amount, out decimal amountValue) || amountValue <= 0)
                return "Error: Cannot transfer zero or neagative amounts.";

            // once validation has passed, then check account numbers exist
            //
            var fromAccount = Accounts.FirstOrDefault(a => a.AccountNumber == fromAccountNumber);
            if (fromAccount == null)
                return $"Error: Account #'{fromAccountNumber}' is invalid.";

            var toAccount = Accounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);
            if (toAccount == null)
                return $"Error: Account #'{toAccountNumber}' is invalid.";


            //validating token
            if (string.IsNullOrWhiteSpace(token) || token.Length != 12)
                return "Error: Invalid transfer token.";

            if (fromAccount.Balance < amountValue)
                return "Error: Insufficient funds.";

            //update transfer amounts
            fromAccount.Balance -= amountValue;
            toAccount.Balance += amountValue;
            try
            {
                //persist data by saving new balances to file
                SaveAccount();
            }
            catch (Exception ex)
            {
                //if there was an error with the save, reset account values and return error string
                fromAccount.Balance += amountValue;
                toAccount.Balance -= amountValue;
                return $"Error: {ex.Message}";
            }

            //display success message with new values
            return $"Token: {token} successfully validated.\nNew balance for {fromAccountNumber}: ${fromAccount.Balance}";
        }

    

        private static string HandleWithdraw(string[] parts)
        {
            //sending format = "WITHDRAW|{accountNumber}|{amount}"
            //throw new NotImplementedException();

            if (parts.Length < 3)
                return "Error: Invalid input. Please try again.";

            string accountNumber = parts[1].Trim();
            string amount = parts[2].Trim();

            //validate withdrawal amount
            if (!decimal.TryParse(amount, out decimal amountValue) || amountValue <= 0)
                return "Error: Invalid withdrawal amount.";

            //locate the account to withdraw from
            var account = Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
                return "Error: Account not found.";

            //get balance and check if they have enough money
            if (account.Balance < amountValue)
                return "Error: Insufficient funds.";

            //if all checks are passed, then attempt to withdraw the amount and persist, else throw error message and clear variable amount
            account.Balance -= amountValue;
            try
            {
                SaveAccount();
            }
            catch (Exception ex)
            {
                account.Balance += amountValue;
                return $"Error: {ex.Message}";
            }

            return $"Success: Withdrawal completed. New balance: ${account.Balance}";
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
            //throw new NotImplementedException();

            if (parts.Length < 2)
                return "Error: Invalid input. Please try again.";

            string accountNumber = parts[1].Trim();

            //get account number and check if itexists
            var account = Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
                return "Error: Account not found.";

            //display balance
            return $"Balance for {accountNumber} is: ${account.Balance}";
        }

        public static string HandleLogin(string[] parts)
        {
            //sending format = "LOGIN|{accountNumber}|{password}"

            string accountNumber = parts[1];
            string password = parts[2];

            //check the list of accounts for matching account number
            var user = Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (user == null)
                return "ERROR: Invalid account number";

            //get reference number and check if it exists
            //not prompting the client for it but validating that it exists in the account as an extra layer of security to prevent unauthorized access
            var referenceNumber = user.ReferenceNumber;
            if (referenceNumber == null)
                return "ERROR: Invalid reference number";

            //if a valid account is returned, then proceed to check the password
            if (user.Password != password)
                return "ERROR: Invalid password";

            //if all checks are passed, then log the user in by setting IsLoggedIn to true
            user.IsLoggedIn = true;

            return $"SUCCESS: Reference Number: {referenceNumber} validated.\nYou are now logged in.";
        }



        public static string HandleCreate(string[] parts)
        {
            //sending in the following argument:
            //$"CREATE|{accountNumber}|{referenceNumber}|{password}"

            string accountNumber = parts[1];
            string referenceNumber = parts[2];
            string password = parts[3];

            if (Accounts.Any(a => a.ReferenceNumber == referenceNumber))
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