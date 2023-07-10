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
                    Console.WriteLine(e.Message);
                    log?.Error(e.Message);
                }
            }
            else
            {
                var msg = "Unable to initialize TCP client from controller. Missing or incorrect server or port in config file.";
                Console.WriteLine(msg);
                log?.Error(msg);
            }
            return null;
        }

        public static ConfigOptions GetConfigValues(string configFile, Logger log = null)
        {
            var config = new ConfigOptions();
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
                        if (key.Contains("address")) { config.ServerIp = val; }
                        else if (key.Contains("port")) { config.Port = int.Parse(val); }
                        else if (key.Contains("interval")) { config.Interval = int.Parse(val); }
                        else if (key.Contains("acceleration")) { config.VehAcceleration = double.Parse(val); }
                        else if (key.Contains("origin")) { config.VehOriginLinkId = int.Parse(val); }
                        else if (key.Contains("destination")) { config.VehDestinationLinkId = int.Parse(val); }
                        else if (key.Contains("speed")) { config.VehInitialSpeed = double.Parse(val); }
                        else if (key.Contains("departuretime")) { config.VehDepartureTime = int.Parse(val); }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                log?.Error(e.Message);
            }
            return config;
        }
    }

    public class ConfigOptions
    {
        public string ServerIp { get; set; }
        public int Port { get; set; }
        public int Interval { get; set; }

        public int VehOriginLinkId { get; set; }
        public int VehDestinationLinkId { get; set; }
        public double VehAcceleration { get; set; }
        public double VehInitialSpeed { get; set; }
        public int VehDepartureTime { get; set; }
    }
}
