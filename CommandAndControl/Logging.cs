using CommandAndControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public class Logging
    {
        private static Logging instance = null;
        private StreamWriter writer;

        public static Logging Instance
        {
            get
            {
                if (instance == null)
                    instance = new Logging();

                return instance;
            }
        }

        private Logging()
        {
            writer = new StreamWriter(Network.Path + Network.OutputFile, true, Encoding.UTF8, 1024);
        }

        public void Log(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
        }

        public void Close()
        {
            writer.Flush();
            writer.Close();

            instance = null;
        }
    }
}
