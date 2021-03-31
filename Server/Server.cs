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

        bool shouldFinish = false;
        ManualResetEvent connectionEvent = new ManualResetEvent(false);
        ManualResetEvent serverFinishedEvent = new ManualResetEvent(false);

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

            IPEndPoint localEndPoint = new IPEndPoint(Network.IP, Network.Port);
            Socket listener = new Socket(Network.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(1);

                while (!shouldFinish)
                {
                    writer.WriteLine("Waiting for connection");

                    connectionEvent.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    connectionEvent.WaitOne();
                }

                listener.Close();
            }
            catch (Exception e)
            {
                writer.WriteLine(e.Message);
            }
        }

        protected void AcceptCallback(IAsyncResult ar)
        {
            writer.WriteLine("Stopping server");
            connectionEvent.Set();

            byte[] data = new byte[1024];

            Socket listener = (Socket)ar.AsyncState;

            try
            {
                Socket handler = listener.EndAccept(ar);


                int bytesRcvd = handler.Receive(data);
                byte[] payload = new byte[bytesRcvd];

                Array.Copy(data, payload, bytesRcvd);

                ThreadPool.QueueUserWorkItem(controller.ExecuteAndRespond, new object[] { handler, payload });
            }
            catch (ObjectDisposedException ode)
            {
                serverFinishedEvent.Set();
            }
        }

        protected override void OnStop()
        {
            shouldFinish = true;
            connectionEvent.Set();

            writer.WriteLine("Server stopped");
            serverFinishedEvent.WaitOne();

            writer.Close();
        }
    }
}
