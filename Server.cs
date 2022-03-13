using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;


namespace Threaded_Chat_Server
{
    public static class Terminal 
    {

        public enum Status 
        {
            Warning,
            Info,
            Error
        }
        /// <summary>
        /// Writes to the console in a better way
        /// </summary>
        /// <param name="value"></param>
        /// <param name="status"></param>
        public static void WriteLine(string value, Status status = Status.Info) 
        {
            DateTime time = DateTime.Now;
            string Stime = string.Format("{0}:{1}:{2}", time.Hour, time.Minute,time.Second);

            Console.WriteLine("[{0}]{1}: {2}", Stime, status, value);
        }
    }


    public static class Help 
    {

        /// <summary>
        /// Gets the clients name
        /// </summary>
        /// <param name="clients"></param>
        /// <returns></returns>
        public static List<string> GetNames(this List<Connectionhandler.RClient> clients) 
        {
            List<string> handler = new List<string>();

            for (int i = 0; i < clients.Count; i++)
            {
                handler.Add(clients[i].Name);
            }
            return handler;
        }
    }

    public class Connectionhandler 
    {

        public enum ClientStatus 
        {
            IncorrectName,
            CorrectName,
            HaveData
        }

        public struct RClient
        {
            public TcpClient client;
            public string Data;
            public string Name;
            public ClientStatus ClientStatus;

            
        }
        public static List<RClient> Clients = new List<RClient>();

        /// <summary>
        /// Handle the client connection
        /// </summary>
        /// <param name="Client"></param>
        public static void Clientregister(TcpClient Client) 
        {
            Clients.Add(new RClient { client = Client });

            
            Thread rd =new  Thread(() => ReciveData(Clients.Count-1));
            rd.Start();

            if(Clients.Count == 1) 
            {
                Thread sd = new Thread(SendData);
                sd.Start();
            }
           
        }

       

        /// <summary>
        /// Sends message to every client
        /// </summary>
        /// <param name="str"></param>
        public static void SendAll(string str)
        {
            
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].client.Connected) 
                {
                    StreamWriter sw = new StreamWriter(Clients[i].client.GetStream());

                    sw.WriteLine(str);
                    sw.Flush();
                }
                
            }
        }

        /// <summary>
        /// Sends out the received messages
        /// </summary>
        public static void SendData() 
        {
            int ClientwantSend = -1;
            RClient handler = new RClient();


            
            while (true) 
            {
                
                //Search for message
                for (int i = 0; i < Clients.Count; i++)
                {
                    if(Clients[i].Data != string.Empty) 
                    {
                        ClientwantSend = i;
                        break;
                    }
                }

                
                if(ClientwantSend != -1)
                {
                   
                    for (int i = 0; i < Clients.Count; i++)
                    {


                        if (Clients[i].client.Connected) 
                        {
                            StreamWriter sw = new StreamWriter(Clients[i].client.GetStream());
                            if (Clients[i].ClientStatus == ClientStatus.IncorrectName)
                            {
                                sw.WriteLine("Choose (another) name!");
                                sw.Flush();
                            }


                            if (i != ClientwantSend)
                            {



                                sw.WriteLine("{0}# {1}", Clients[ClientwantSend].Name, Clients[ClientwantSend].Data);
                                Terminal.WriteLine(string.Format("{0}# {1}", Clients[ClientwantSend].Name, Clients[ClientwantSend].Data));
                                sw.Flush();


                            }
                        }

                        
                        //sw.Flush();
                    }

                    //Reset
                    
                    handler = Clients[ClientwantSend];
                    handler.Data = string.Empty;
                    Clients[ClientwantSend] = handler;
                    ClientwantSend = -1;

                }
            }
        }

        /// <summary>
        /// If string is handshake returns client name
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetClientName(string input) 
        {
            //hs:{Name}

            string name = string.Empty;

            if (input.Contains(":")) 
            {
                if (input.Split(':')[0] == "hs")
                {
                    name = input.Split(':')[1];
                }
            }

            return name;

        }

        /// <summary>
        /// Receive message and do the name registration
        /// </summary>
        /// <param name="index"></param>
        public static void ReciveData(int index) 
        {
            TcpClient client = Clients[index].client;
            
            StreamReader sr = new StreamReader(client.GetStream(), Encoding.UTF8);

            RClient handler = Clients[index];
            
            while (client.Connected) 
            {

                try 
                {
                    handler.Data = sr.ReadLine();
                    
                }
                catch 
                {
                    break;
                }
                
                //A beérkezett adat handshake
                if(GetClientName(handler.Data) != string.Empty) 
                {
                    //A kliens neve már regisztrálva van
                    if (Clients.GetNames().Contains(GetClientName(handler.Data)))
                    {
                        handler.Data = string.Empty;
                        handler.ClientStatus = ClientStatus.IncorrectName;
                        Clients[index] = handler;
                    }
                    else
                    {
                        
                        handler.Name = GetClientName(handler.Data);
                        Terminal.WriteLine(string.Format("{0} Registraded", handler.Name));
                        handler.Data = string.Empty;
                        handler.ClientStatus = ClientStatus.CorrectName;
                        Clients[index] = handler;
                        Thread t2 = new Thread(() => SendAll(string.Format("Members: {0}", string.Join("|", Clients.GetNames()))));
                        
                        t2.Start();
                    }
                }
                else 
                {
                    Terminal.WriteLine(string.Format("{0}: {1}", handler.Name, handler.Data));

                    Clients[index] = handler;
                }

            }
            SendAll(string.Format("{0} Disconnected", Clients[index].Name));
            
            Terminal.WriteLine(string.Format("{0} Disconnected", Clients[index].Name));
            sr.Close();
            
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string ip = Dns.GetHostByName(Dns.GetHostName()).AddressList[1].ToString();

            const int port = 8080;
            
            

            Terminal.WriteLine(string.Format(  "Server Ip: {0}:{1}", ip, port));

            TcpListener server = new TcpListener(System.Net.IPAddress.Parse(ip),port);


            server.Start();
            Terminal.WriteLine("Server started!");


            TcpClient client;
            while (true) 
            {


                client = server.AcceptTcpClient();
                Terminal.WriteLine(string.Format("{0} Connected", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()));
                Thread t = new Thread(() => Connectionhandler.Clientregister(client));
                t.Start();

               
            }


            
        }
    }
}
