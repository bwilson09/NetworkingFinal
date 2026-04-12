// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    public static int Main(string[] args)
    {
        StartServer();
        return 0;
    }

    public static void StartServer()
    {
        try
        {
            IPAddress ip = IPAddress.Parse("192.168.219.19");
            IPEndPoint localIP = new IPEndPoint(ip, 10001); // --> this needs to match the # on the client side

            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localIP);
            listener.Listen(10); //  -->10 indicates the number of connections that this socket can handle at the same time

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

                //response = "This is a response";
                if (data.ToLower().IndexOf("dog") > -1)
                {
                    response = "I like dogs, chihuahuas are the way to go.";
                }

                if (data.Contains("pizza"))
                {
                    response += "Pineapples on pizzas is okay";
                }

                if (response == null)
                {
                    response = "This is a response to a null response";
                }

                byte[] msg = Encoding.ASCII.GetBytes(response);
                handler.Send(msg);
            }


        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }








}