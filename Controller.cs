using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsm;

namespace TsmXiL
{
    internal class Controller
    {
        private ITsmApplication Tsm { get; set; }
        private Logger Log { get; set; }

        public Controller(ITsmApplication tsm, Logger logger)
        {
            Tsm = tsm;
            Log = logger;
        }

        public void Update(double time)
        {
            SendData(null);
            ReceiveData();
            UpdateSimulationVehicle();
            var simTime = Tsm.TimeToString(time);
            Log.Data(simTime);
        }

        private void SendData(object leaderData)
        {

        }

        private object ReceiveData()
        {
            return null;
        }

        private bool UpdateSimulationVehicle()
        {
            return false;
        }
    }
}
