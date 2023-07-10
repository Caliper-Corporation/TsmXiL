using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Tsm;

namespace TsmXiL
{
    public class Controller
    {
        private static BinaryReader _reader;
        private static NetworkStream _stream;
        private readonly string _configFile;
        private TcpClient _client;

        private ConfigOptions _config;
        private string _dataLine;
        private string _simTime;
        private int _vehicleId;

        private ITsmApplication Tsm { get; }
        private Logger Log { get; }
        public double UpdateIntervalInSeconds { get; private set; }
        public ITsmVehicle Vehicle { get; private set; }

        public Controller(ITsmApplication tsm, Logger logger, string configFile)
        {
            Tsm = tsm;
            Log = logger;
            _configFile = configFile;
            _vehicleId = 0;
        }
        
        public void Init()
        {
            _config = Utils.GetConfigValues(_configFile, Log);
            UpdateIntervalInSeconds = (double)_config.Interval / 1000;
            _client = Utils.GetTcpClient(_config.ServerIp, _config.Port, Log);
            _stream = _client?.GetStream();
            if (_stream != null) _reader = new BinaryReader(_stream, Encoding.Default, true);
            Log.Data("Sim Time, Request Acceleration, Request Speed, Response Acceleration, Response Speed");
            AddVehicle();
        }

        public void Update(double time)
        {
            _simTime = Tsm?.TimeToString(time);
            _simTime = string.IsNullOrEmpty(_simTime) ? DateTime.Now.ToString("G") : _simTime;
            _dataLine = $"{_simTime},";

            if (Vehicle == null)
            {
                Vehicle = Tsm?.Network?.Vehicle[_vehicleId];
                if (Vehicle != null)
                {
                    Vehicle.Track(true, true);
                    Tsm?.Pause(true);
                }
            }

            UpdateRealVehicle();
            UpdateSimulationVehicle();
            if (Vehicle != null) Log.Data(_dataLine);
        }

        private void AddVehicle()
        {
            var opts = new TsmAttributes();
            opts.Set("Speed", _config.VehInitialSpeed);
            opts.Set("Departure Time", _config.VehDepartureTime);
            opts.Set("Stop on Arrival", true);
            var ori = new STsmLocation
            {
                type = TsmLocationType.LOCATION_LINK,
                id = _config.VehOriginLinkId
            };
            var des = new STsmLocation
            {
                type = TsmLocationType.LOCATION_LINK,
                id = _config.VehDestinationLinkId
            };
            _vehicleId = Tsm.Network.AddVehicle(ref ori, ref des, opts);
            if (_vehicleId == 0)
            {
                var msg = "Unable to add vehicle to network.";
                Log.Error(msg);
                throw new Exception(msg);
            }

            Log.Info(
                $"Added new vehicle on link {_config.VehOriginLinkId} with id {_vehicleId} and turned on tracking");
        }

        private void UpdateRealVehicle()
        {
            if (Vehicle == null) return;
            var req = new Request
            {
                Acceleration = Vehicle.Acceleration,
                Speed = Vehicle.Speed
            };

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