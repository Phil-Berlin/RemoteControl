using CommandAndControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControl
{
    public partial class Server : ServiceBase
    {
        Thread serverThread;
        StreamWriter writer;
        IController controller;

        public Server()
        {
            InitializeComponent();
            controller = new WindowsController();
            writer = new StreamWriter(Network.Path + Network.OutputFile, true, Encoding.UTF8, 1024);
        }

        protected override void OnStart(string[] args)
        {
            writer.WriteLine("Starting Server");

            serverThread = new Thread(RunServer);
            serverThread.Name = "ServerThread";
            serverThread.Start();
        }

        public void RunServer()
        {
            writer.WriteLine("Server started");

            byte[] data = new byte[1024];

            IPEndPoint localEndPoint = new IPEndPoint(Network.IP, Network.Port);
            Socket listener = new Socket(Network.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(1);

                while (true)
                {
                    writer.WriteLine("Waiting for connection");
                    Socket handler = listener.Accept();

                    int bytesRcvd = handler.Receive(data);

                    byte[] payload = new byte[bytesRcvd];
                    Array.Copy(data, payload, bytesRcvd);

                    ThreadPool.QueueUserWorkItem(controller.ExecuteAndRespond, new object[] { handler, payload });

                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                writer.WriteLine(e.Message);
            }
        }

        protected override void OnStop()
        {
            writer.WriteLine("Stopping server");
            
            if (serverThread.IsAlive)
            {
                serverThread.Suspend();
            }
            
            writer.WriteLine("Server stopped");

            writer.Close();
        }
    }
}
