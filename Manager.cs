using System;
using System.IO;
using Tsm;

namespace TsmXiL
{
    public class Manager
    {
        private ITsmApplication Tsm { get; set; }
        private CSensorEvents SensorEvents { get; set; }
        private CSimulationEvents SimulationEvents { get; set; }
        private CVehicleEvents VehicleEvents { get; set; }
        private Controller Controller { get; set; }
        private Logger Log { get; set; }
        public string LogFile => Log?.LogFile;
        private double NextTime { get; set; }

        //This is the entry point of the plugin from TransModeler
        public bool Start(string configFile)
        {
            try
            {
                Tsm = new TsmApplication();
                if (Tsm == null)
                {
                    return false;
                }

                var logFile = Path.Combine(Tsm.OutputBaseFolder, "TsmXiL-log.txt");
                var dataFile = Path.Combine(Tsm.OutputBaseFolder, "TsmXiL-data.csv");
                Log = new Logger(logFile, dataFile);
                Controller = new Controller(Tsm, Log, configFile);
                Log.Info("Initiating TsmXiL Plugin...");

                if (!ConnectToTsm())
                {
                    return false;
                }

                SubscribeToEvents();

                Log.Info("########## TsmXiL plugin initialized successfully! ##########");

                return true;
            }
            catch (Exception exception)
            {
                Disconnect();
                Log.Error($"An error occurred while initializing the TsmXiL plugin.\nError: {exception.Message}");
                throw;
            }
        }

        private bool ConnectToTsm()
        {
            Log.Info("Connecting to TransModeler...");
            if (Tsm == null)
            {
                return false;
            }

            SensorEvents = new CSensorEvents(Tsm);
            SimulationEvents = new CSimulationEvents(Tsm);
            VehicleEvents = new CVehicleEvents(Tsm);
            Log.Info("Connected to TransModeler!");
            return true;
        }

        public void Disconnect()
        {
            try
            {
                Log?.Info("Disconnecting TsmXiL plugin from Tsm...");
                if (SimulationEvents != null)
                {
                    SimulationEvents.OnSimulationStarted -= OnSimulationStarted;
                    SimulationEvents.OnAdvance -= OnSimulationAdvance;
                    SimulationEvents.OnSimulationStopped -= OnSimulationStopped;
                    SimulationEvents.Disconnect();
                    SimulationEvents = null;
                }

                if (SensorEvents != null)
                {
                    SensorEvents.Disconnect();
                    SensorEvents = null;
                }

                if (VehicleEvents != null)
                {
                    VehicleEvents.OnArrive -= OnVehicleArrival;
                    VehicleEvents.Disconnect();
                    VehicleEvents = null;
                }
                Controller?.CleanUp();
                Log?.Info("Disconnected!");
                Log?.Close();
                Log = null;
                if (Tsm != null)
                {
                    Tsm = null;
                }

                GC.Collect();
            }
            catch (Exception exception)
            {
                Log?.Error($"{exception.Message}\n{exception.StackTrace}");
            }
        }

        private void SubscribeToEvents()
        {
            Log.Info("Subscribing to simulation events...");
            SimulationEvents.OnSimulationStarted += OnSimulationStarted;
            SimulationEvents.OnAdvance += OnSimulationAdvance;
            SimulationEvents.OnSimulationStopped += OnSimulationStopped;
            VehicleEvents.OnArrive += OnVehicleArrival;
            Log.Info("Subscribed to simulation events!");
        }

        private void OnVehicleArrival(int idvehicle, double dtime)
        {
            if (Controller.Vehicle != null && idvehicle == Controller.Vehicle.id)
            {
                Tsm.Pause(true);
                Disconnect();
            }
        }

        private void OnSimulationStopped(TsmState e)
        {
            Controller?.CleanUp();
        }

        private void OnSimulationStarted()
        {
            NextTime = Tsm.StartTime;
            Controller?.Init();
        }

        private double OnSimulationAdvance(double time)
        {
            try
            {
                if (Math.Abs(time - Controller.Config.VehStartTime) < 0.1)
                {
                    Controller.AddVehicle();
                }
                if (time >= NextTime)
                {
                    NextTime = time + Controller.UpdateIntervalInSeconds;
                }

                Controller.Update(time);
                return NextTime;
            }
            catch (Exception e)
            {
                var message = "An error occurred in the OnSimulationAdvance method of the TsmXiL plugin. \n" +
                              $"Error message: {e.Message}";
                Log.Error($"{message}\nError Source: {e.StackTrace}");
                Tsm.LogErrorMessage("[TsmXiL]" + message);
                return double.MaxValue;
            }
        }
    }
}