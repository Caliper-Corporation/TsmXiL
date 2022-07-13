using System;
using System.Windows.Forms;
using Tsm;

namespace TsmXiL
{
    public class Manager
    {
        private ITsmApplication Tsm { get; set; }
        private CSensorEvents SensorEvents { get; set; }
        private CSimulationEvents SimulationEvents { get; set; }
        private Logger Logger { get; set; }
        public string LogFile => Logger?.LogFilePath;
        private double NextTime { get; set; }

        //This is the entry point of the plugin from TransModeler
        public bool Start(string configFile)
        {
            try
            {
                Tsm = new TsmApplication();
                if (Tsm == null) return false;

                Logger = new Logger();

                Logger.Log("Initiating TsmXiL Plugin...");

                if (!ConnectToTsm()) return false;

                SubscribeToEvents();

                Logger.Log("########## TsmXiL plugin initialized successfully! ##########");

                return true;
            }
            catch (Exception exception)
            {
                Disconnect();
                Logger.Error($"An error occurred while initializing the TsmXiL plugin.\nError: {exception.Message}");
                throw;
            }
        }

        private bool ConnectToTsm()
        {
            Logger.Log("Connecting to TransModeler...");
            if (Tsm == null) return false;
            SensorEvents = new CSensorEvents(Tsm);
            SimulationEvents = new CSimulationEvents(Tsm);
            Logger.Log("Connected to TransModeler!");
            return true;
        }

        public void Disconnect()
        {
            try
            {
                Logger?.Log("Disconnecting TsmXiL plugin from Tsm...");
                if (SimulationEvents != null)
                {
                    SimulationEvents.OnSimulationStarted -= OnSimulationStarted;
                    SimulationEvents.OnAdvance -= OnSimulationAdvance;
                    SimulationEvents.OnSimulationStopped -= OnSimulationStopped;
                    SimulationEvents.Disconnect();
                    SimulationEvents = null;
                }

                SensorEvents?.Disconnect();
                SensorEvents = null;
                Logger?.Log("Disconnected!");
                Logger?.Close();
                Logger = null;
                if (Tsm != null) Tsm = null;
                GC.Collect();
            }
            catch (Exception exception)
            {
                Logger?.Error($"{exception.Message}\n{exception.StackTrace}");
            }
        }

        private void SubscribeToEvents()
        {
            Logger.Log("Subscribing to simulation events...");
            SimulationEvents.OnSimulationStarted += OnSimulationStarted;
            SimulationEvents.OnAdvance += OnSimulationAdvance;
            SimulationEvents.OnSimulationStopped += OnSimulationStopped;
            Logger.Log("Subscribed to simulation events!");
        }

        private void OnSimulationStopped(TsmState e)
        {
            Disconnect();
        }

        private void OnSimulationStarted()
        {
            NextTime = Tsm.StartTime;
            MessageBox.Show("Hello, from the TsmXiL plugin!");
        }

        private double OnSimulationAdvance(double time)
        {
            try
            {
                if (time >= NextTime) NextTime = time + 1;

                return NextTime;
            }
            catch (Exception e)
            {
                var message = "An error occurred in the OnSimulationAdvance method of the TsmXiL plugin. \n" +
                              $"Error message: {e.Message}";
                Logger.Error($"{message}\nError Source: {e.StackTrace}");
                Tsm.LogErrorMessage("[TsmXiL]" + message);
                return double.MaxValue;
            }
        }
    }
}