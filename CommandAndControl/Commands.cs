using System;
using System.Collections.Generic;
using System.Text;

namespace CommandAndControl
{
    public class Commands
    {
        public enum Command
        {
            SendMessage = 1,
            Shutdown = 2,
            GetTasks = 3,
            Ping = 4
        }

        public enum ShutdownType
        {
            Shutdown,
            Reboot,
            Logoff
        }
    }
}
