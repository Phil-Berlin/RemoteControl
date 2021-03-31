using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CommandAndControl
{
    public static class Network
    {
        // ToDo: use config file
        public static readonly IPAddress IP = IPAddress.Parse("192.168.0.2");
        public static readonly short Port = 0x1710;
        public static readonly string Username = "Username";
        public static readonly string Path = $@"C:\Users\{Username}\Desktop\";
        public static readonly string OutputFile = @"rccout.log";
    }
}
