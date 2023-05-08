using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Tsm;

namespace TsmXiL
{
    public class Controller
    {
        private ITsmApplication Tsm { get; set; }
        private Logger Log { get; set; }
        public double UpdateIntervalInSeconds { get; set; }

        public ConfigOptions Config;
        private TcpClient _client;
        private static BinaryReader _reader;
        private static NetworkStream _stream;
        private string _simTime;
        public ITsmVehicle Vehicle { get; set; }
        private readonly string _configFile;
        private string _dataLine;


        public Controller(ITsmApplication tsm, Logger logger, string configFile)
        {
            Tsm = tsm;
            Log = logger;
            _configFile = configFile;
        }

        public void Init()
        {
            Config = Utils.GetConfigValues(_configFile, Log);
            UpdateIntervalInSeconds = (double)Config.Interval / 1000;
            _client = Utils.GetTcpClient(Config.ServerIp, Config.Port, Log);
            _stream = _client?.GetStream();
            if (_stream != null)
            {
                _reader = new BinaryReader(_stream, Encoding.Default, true);
            }
            Log.Data("Sim Time, Request Acceleration, Request Speed, Response Acceleration, Response Speed");
        }

        public void Update(double time)
        {
            _simTime = Tsm?.TimeToString(time);
            _simTime = string.IsNullOrEmpty(_simTime) ? DateTime.Now.ToString("G") : _simTime;
            _dataLine = $"{_simTime},";
            UpdateRealVehicle();
            UpdateSimulationVehicle();
            if (Vehicle != null) Log.Data(_dataLine);
        }

        public void AddVehicle()
        {
            var opts = new TsmAttributes();
            opts.Set("Speed", Config.VehInitialSpeed);
            //opts.Set("Departure Time", Config.VehStartTime);
            var ori = new STsmLocation
            {
                type = TsmLocationType.LOCATION_LINK,
                id = Config.VehOriginLaneId
            };
            var des = new STsmLocation()
            {
                type = TsmLocationType.LOCATION_LINK,
                id = Config.VehDestinationLaneId
            };
            Vehicle = Tsm.Network.AddVehicle(ref ori, ref des, opts);
            if (Vehicle == null)
            {
                var msg = "Unable to add vehicle to network.";
                Log.Error(msg);
                throw new Exception(msg);
            }
            Vehicle.Track(true, true);
            Log.Info($"Added new vehicle on lane {Config.VehOriginLaneId} with id {Vehicle.id} and turned on tracking");
            Tsm.Pause(true);
        }

        private void UpdateRealVehicle()
        {
            var req = new Request();
            if (Vehicle != null && Vehicle.Leader != null)
            {
                req.Acceleration = Vehicle.Leader.Acceleration;
                req.Speed = Vehicle.Leader.Speed;
            }
            else
            {
                req.Acceleration = Config.VehAcceleration;
                req.Speed = Config.VehInitialSpeed;
            }

            _dataLine += $"{req.Acceleration:F2},{req.Speed:F2},";
            var bytes = req.GetByteArray();
            _stream?.Write(bytes, 0, bytes.Length);
        }

        private void UpdateSimulationVehicle()
        {
            if (_reader == null) return;
            var res = new Response
            {
                Acceleration = _reader.ReadDouble(),
                Speed = _reader.ReadDouble()
            };
            _dataLine += $"{res.Acceleration:F2},{res.Speed:F2}";
            if (Vehicle != null)
            {
                Vehicle.Acceleration = (float)res.Acceleration;
                Vehicle.Speed = (float)res.Speed;
            }
        }

        public void CleanUp()
        {
            _reader?.Close();
            _reader?.Dispose();
            _stream?.Close();
            _client?.Close();
        }
    }
}
