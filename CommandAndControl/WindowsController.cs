using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

using Command = CommandAndControl.Commands.Command;
using ShutdownType = CommandAndControl.Commands.ShutdownType;

namespace CommandAndControl
{
    public class WindowsController : IController
    {
        public void ExecuteAndRespond(object obj)
        {            
            Socket socket;
            byte[] data;
            
            try
            {
                object[] objectArray = obj as object[];
                socket = (Socket)objectArray[0];
                data = (byte[])objectArray[1];
            }
            catch (Exception e)
            {
                // Log.WriteLine(e.Message);
                return;
            }

            Command command;
            bool result;

            if (data != null && data.Length > 0)
            {
                try
                {
                    command = (Command)data[0];

                    byte[] payload = new byte[data.Length - 1];
                    Array.Copy(data, 1, payload, 0, payload.Length);
                    
                    result = Execute(command, payload);

                }
                catch (Exception e)
                {
                    // Log.WriteLine(e.Message);
                    result = false;
                }

                SendResponse(socket, data[0], result);
            }
            else
            {
                //Log.WriteLine("Bad data format");
            }

        }

        public bool Execute(Command command, byte[] payload = null)
        {
            switch (command)
            {
                case (Command.SendMessage):
                {
                    if (payload != null && payload.Length > 0)
                    {
                        string message = Encoding.UTF8.GetString(payload, 0, payload.Length);
                        return ShowMessage(message);
                    }

                    return false;
                }
                case (Command.Shutdown):
                {
                    string message = "";

                    if (payload != null && payload.Length > 0)
                    {
                        message = Encoding.UTF8.GetString(payload, 0, payload.Length);
                    }

                    return Shutdown(delay:30, message:message);
                }
                case (Command.GetTasks):
                {
                    return GetTasks();
                }
                case (Command.Ping):
                {
                    return Ping();
                }
                default:
                {
                    return false;
                }
            }
        }

        private void SendResponse(Socket socket, byte command, bool result)
        {
            if (socket != null && socket.Connected)
            {
                socket.Send(new byte[] { command, (byte)(result ? 1 : 0) });
                socket.Close();
            }
        }

        public bool ShowMessage(string message)
        {
            if (message == null || message == "")
            {
                return false;
            }

            Process.Start("msg", Network.Username + " " + message);
            return true;
        }

        public bool Shutdown(ShutdownType shutdownType = ShutdownType.Shutdown, int delay = 0, bool force = true, string message = "")
        {
            string param;

            switch (shutdownType)
            {
                case (ShutdownType.Shutdown):
                {
                    param = "/s";
                    break;
                }
                case (ShutdownType.Reboot):
                {
                    param = "/r";
                    break;
                }
                case (ShutdownType.Logoff):
                {
                    param = "/l";
                    break;
                }
                default:
                    return false;
            }

            if (delay > 0)
            {
                param += " /t " + Convert.ToString(delay);
            }

            if (force)
            {
                param += " /f";
            }

            if (message != null && message != "")
            {
                param += " /c \"" + message + "\"";
            }

            Process.Start("shutdown", param);

            return true;
        }

        public bool GetTasks()
        {
            return true;
        }

        public bool Ping()
        {
            return true;
        }
    }
}
