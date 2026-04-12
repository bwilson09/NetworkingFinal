using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingFinal
{
    public class Account
    {
        public string AccountNumber { get; set; }
        public string Password { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Balance { get; set; }

        public bool IsLoggedIn { get; set; }
    }
}
