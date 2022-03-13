using System;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Threaded_Chat_Client
{
    public static class Terminal
    {
        public static bool IsReading;

        public static string RedaPrefix 
        {
            get 
            {
                return string.Format("[{0}]#Server: ", GetTime());
            }
            set 
            {

            }
        }

        public enum Status
        {
            Warning,
            Info,
            Error
        }

        private static string GetTime() 
        {
            DateTime time = DateTime.Now;
            return string.Format("{0}:{1}:{2}", time.Hour, time.Minute, time.Second);
        }
        /// <summary>
        /// Writes to the console in a better way
        /// </summary>
        /// <param name="value"></param>
        /// <param name="status"></param>
        public static void WriteLine(string value, Status status = Status.Info)
        {
           
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine("[{0}]{1}: {2}", GetTime(), status, value);
            if (IsReading) 
            {
                Console.Write(RedaPrefix);
                
            }
        }
    }


    public static class HandleConnection 
    {
        public static TcpClient Client;

        /// <summary>
        /// Handle client connection
        /// </summary>
        /// <param name="client"></param>
        public static void StartHandle(TcpClient client) 
        {
            Client = client;

            Thread t1 = new Thread(ReciveDate);
            Thread t2 = new Thread(SendData);
            t1.Start();
            t2.Start();


        }

        /// <summary>
        /// Writes the received data to the console
        /// </summary>
        public static void ReciveDate() 
        {
            StreamReader sr = new StreamReader(Client.GetStream(),Encoding.UTF8);

            while (Client.Connected) 
            {
                Terminal.WriteLine(sr.ReadLine());
            }


        }

        /// <summary>
        /// Sends the message
        /// </summary>
        public static void SendData() 
        {
            StreamWriter sw = new StreamWriter(Client.GetStream(), Encoding.UTF8);

            bool ishs = false;
            while (true) 
            {
                Terminal.IsReading = true;
                Console.Write(Terminal.RedaPrefix);


                if(ishs == false) 
                {
                    sw.WriteLine("hs:"+Console.ReadLine());
                }
                else 
                {
                    sw.WriteLine(Console.ReadLine());
                }

                
                sw.Flush();

                ishs = true;
            }



        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Random rnd = new Random();
            Console.Title = string.Format("Client #{0}", rnd.Next(1000,10000));
            while (true) 
            {
                try 
                {
                    TcpClient client = new TcpClient("192.168.1.100", 8080);
                    Terminal.WriteLine("Connected");
                    Thread t = new Thread(() => HandleConnection.StartHandle(client));
                    t.Start();

                    //It is ugly but works
                    while (client.Connected)
                    {

                    }
                }
                catch 
                {
                    Terminal.WriteLine("Connection failure", Terminal.Status.Error);
                    Thread.Sleep(50);
                }
            }
        }
    }
}
