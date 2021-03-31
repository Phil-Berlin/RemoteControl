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

using CommandAndControl;
using Util;

namespace RemoteControl
{
    public partial class Server : ServiceBase
    {
        Thread serverThread;
        IController controller;

        bool shouldFinish = false;
        ManualResetEvent connectionEvent = new ManualResetEvent(false);
        ManualResetEvent serverFinishedEvent = new ManualResetEvent(false);

        public Server()
        {
            InitializeComponent();
            controller = new WindowsController();
        }

        protected override void OnStart(string[] args)
        {
            Logging.Instance.Log("Service started");

            serverThread = new Thread(RunServer);
            serverThread.Name = "ServerThread";
            serverThread.Start();
        }

        public void RunServer()
        {
            Logging.Instance.Log("Server started");

            IPEndPoint localEndPoint = new IPEndPoint(Network.IP, Network.Port);
            Socket listener = new Socket(Network.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(1);

                while (!shouldFinish)
                {
                    Logging.Instance.Log("Waiting for connection");

                    connectionEvent.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    connectionEvent.WaitOne();
                }

                listener.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.Log(e.Message);
            }
        }

        protected void AcceptCallback(IAsyncResult ar)
        {
            connectionEvent.Set();

            byte[] data = new byte[1024];

            Socket listener = (Socket)ar.AsyncState;

            try
            {
                Socket handler = listener.EndAccept(ar);

                Logging.Instance.Log("Connection established");

                int bytesRcvd = handler.Receive(data);
                byte[] payload = new byte[bytesRcvd];

                Array.Copy(data, payload, bytesRcvd);

                ThreadPool.QueueUserWorkItem(controller.ExecuteAndRespond, new object[] { handler, payload });
            }
            catch (ObjectDisposedException ode)
            {
                serverFinishedEvent.Set();
                Logging.Instance.Log("Server stopped");
            }
        }

        protected override void OnStop()
        {
            shouldFinish = true;
            connectionEvent.Set();

            serverFinishedEvent.WaitOne();

            Logging.Instance.Log("Service stopped");
            Logging.Instance.Close();
        }
    }
}
