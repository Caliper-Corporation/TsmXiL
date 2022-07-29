using System;
using System.IO;
using System.Net.Sockets;

namespace TsmXiL
{
    public static class Utils
    {
        public static TcpClient GetTcpClient(string serverIp, int port, Logger log = null)
        {
            if (!string.IsNullOrEmpty(serverIp) && port > 0)
            {
                try
                {
                    return new TcpClient(serverIp, port);
                }
                catch (Exception e)
                {
                    log?.Error(e.Message);
                }
            }
            else
            {
                log?.Error("Unable to initialize TCP client from controller. Missing or incorrect server or port in config file.");
            }
            return null;
        }

        public static void GetConfigValues(string configFile, ref string serverIp, out int port, Logger log = null)
        {
            port = 0;
            try
            {
                var lines = File.ReadAllLines(configFile);
                foreach (var line in lines)
                {
                    var values = line.Split(':');
                    if (values.Length > 0)
                    {
                        var key = values[0].ToLower().Trim();
                        var val = values[1].Trim();
                        if (key.Contains("address")) { serverIp = val; }
                        else if (key.Contains("port")) { port = int.Parse(val); }
                    }
                }
            }
            catch (Exception e)
            {
                log?.Error(e.Message);
            }
        }
    }
}
