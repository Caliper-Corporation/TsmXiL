using System;
using System.Net.Sockets;
using Tsm;

namespace TsmXiL
{
    public class Controller
    {
        private ITsmApplication Tsm { get; set; }
        private Logger Log { get; set; }
        private TcpClient Client { get; set; }

        public Controller(ITsmApplication tsm, Logger logger)
        {
            Tsm = tsm;
            Log = logger;
            
            var config = Utils.GetConfigValues("config.txt", logger);
            Client = Utils.GetTcpClient(config.ServerIp, config.Port, logger);
        }

        public void Update(double time)
        {
            var res = SendAccelCommand();
            UpdateSimulationVehicle(res);
            var simTime = Tsm?.TimeToString(time);
            simTime = string.IsNullOrEmpty(simTime) ? DateTime.Now.ToString("G") : simTime;
            Log.Data(simTime);
        }

        public Response SendAccelCommand()
        {
            //send acceleration command and get back a response
            return null;
        }

        public bool UpdateSimulationVehicle(Response res)
        {
            return false;
        }
    }
}
