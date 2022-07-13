//
// Events.cs
//
// Implement classes to handle TsmApi events.
//

using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using Tsm;

namespace TsmXiL
{
    internal class CTsmEvents<T>
    {
        private int _cookie = -1;
        private IConnectionPoint _icp;

        public CTsmEvents(ITsmApplication tsm)
        {
            _icp = null;
            _cookie = -1;
            Connect(tsm);
        }

        public int Cookie => _cookie;

        ~CTsmEvents()
        {
            Debug.WriteLine("Unload CTsmEvent");
            Disconnect();
        }

        // This wraps a call to IConnectionPoint::Advise(...) so that the
        // ID of the connection point can be saved in m_cookie for use
        // later in calling of Advance(...) method in ITsmApplication.
        // At the same time, subscribe Simulation and sensor Events.
        public void Connect(ITsmApplication tsm)
        {
            var id = typeof(T).GUID;
            IConnectionPointContainer icpc;
            icpc = tsm as IConnectionPointContainer;
            icpc.FindConnectionPoint(ref id, out _icp);
            _icp.Advise(this, out _cookie);
        }

        public void Disconnect()
        {
            if (_cookie > -1)
            {
                _icp.Unadvise(_cookie);
                _cookie = -1;
            }
        }
    }

    // ============================================================================

    public delegate void NoRetValueHandler<T>(T e);

    public delegate void NoRetValueNoParamHandler();

    public delegate double AdvanceEventHandler(double time);

    public delegate void StartSimulationEventHandler(short iRun, TsmRunType iRunType, bool bPreload);

    public delegate void SensorEventsHandler(int sid, int vid, double dTime, float fSpeed);

    public delegate void NewSignalStateEventHandler(int idSignal, double dTime, uint ulState);

    public delegate void StartPlanEventHandler(int cookie, TsmControlClass cc, object ids);

    public delegate void AVLEventHandler(TsmVehicle pVehicle, short iClass, short nOccupants, short iType,
        ref STsmCoord3 pLocation);

    public delegate void ArrivalEventHandler(int idVehicle, double dTime);

    public delegate void DepartureEventHandler(int idVehicle, double dTime);

    public delegate void NewLaneEventHandler(TsmVehicle pVehicle, int idLane, double dTime);

    public delegate void NewLinkEventHandler(TsmVehicle pVehicle, int idLink, double dTime);

    public delegate void NewPathEventHandler(TsmVehicle pVehicle, int idPath, int iPosition, double dTime);

    public delegate dynamic GetPropertyEventHandler(TsmVehicle pVehicle, short iCol);

    public delegate void SetPropertyEventHandler(TsmVehicle pVehicle, short iCol, object newVal);

    public delegate float TransitStopEventHandler(double dTime, TsmVehicle pVehicle, TsmRoute pRoute, TsmStop pStop,
        short nMaxCapacity, short nPassengers, float fSchedDev, float fDwellTime);

    public delegate void NewSegmentEventHandler(TsmVehicle pVehicle, int idSegment, double dTime);

    public delegate void LinkCostEventHandler(short iClass, short nOccupancy, short iGroup, uint lType, double dTime,
        TsmLink pFromLink, TsmLink idLink, ref float pVal);

    //used for child-parent communication
    public delegate void ScheduledAction();

    public delegate void OnScheduleChangeEventHandler(double time, ScheduledAction action);

    public delegate void OnSetAdvanceTimeEventHandler(double time);

    internal class CSimulationEvents : CTsmEvents<_ISimulationEvents>, _ISimulationEvents
    {
        public CSimulationEvents(ITsmApplication tsm) : base(tsm)
        {
        }

        public virtual void OpenProject(string fname)
        {
            OnOpenProject?.Invoke(fname);
        }

        // Called just before simulation is about to start
        public virtual void StartSimulation(short iRun, TsmRunType iRunType, bool bPreload)
        {
            OnStartSimulation?.Invoke(iRun, iRunType, bPreload);
        }

        // Called just before simulation loop begins, after all TransModeler
        // internal modules has been initialized
        public virtual void SimulationStarted()
        {
            OnSimulationStarted?.Invoke();
        }

        public virtual double Advance(double dTime)
        {
            if (OnAdvance != null) return OnAdvance(dTime);
            return double.MaxValue;
        }

        public virtual void EndSimulation(TsmState iState)
        {
            OnEndSimulation?.Invoke(iState);
        }

        public virtual void SimulationStopped(TsmState iState)
        {
            OnSimulationStopped?.Invoke(iState);
        }

        public virtual void CloseProject()
        {
            OnCloseProject?.Invoke();
        }

        public virtual void ExitApplication()
        {
            OnExitApplication?.Invoke();
        }

        public event NoRetValueHandler<string> OnOpenProject;
        public event NoRetValueNoParamHandler OnCloseProject;
        public event NoRetValueNoParamHandler OnSimulationStarted;
        public event NoRetValueNoParamHandler OnExitApplication;
        public event AdvanceEventHandler OnAdvance;
        public event NoRetValueHandler<TsmState> OnEndSimulation;
        public event NoRetValueHandler<TsmState> OnSimulationStopped;
        public event StartSimulationEventHandler OnStartSimulation;
    }

    internal class CSensorEvents : CTsmEvents<_ISensorEvents>, _ISensorEvents
    {
        public CSensorEvents(ITsmApplication tsm)
            : base(tsm)
        {
        }

        public virtual void Arrive(int sid, int vid, double dTime, float fSpeed)
        {
            OnArrive?.Invoke(sid, vid, dTime, fSpeed);
        }

        public virtual void Leave(int sid, int vid, double dTime, float fSpeed)
        {
            OnLeave?.Invoke(sid, vid, dTime, fSpeed);
        }

        public event SensorEventsHandler OnArrive;
        public event SensorEventsHandler OnLeave;
    }

    internal class CSignalEvent : CTsmEvents<_ISignalEvents>, _ISignalEvents
    {
        public CSignalEvent(ITsmApplication tsm)
            : base(tsm)
        {
        }

        public virtual void EndPlan(int cookie)
        {
            OnEndPlan(cookie);
        }

        public virtual void NewSignalState(int idSignal, double dTime, uint ulState)
        {
            OnNewSignalState(idSignal, dTime, ulState);
        }

        public virtual void StartPlan(int cookie, TsmControlClass cc, object ids)
        {
            OnStartPlan(cookie, cc, ids);
        }

        public void StartFares(int idEntrance)
        {
        }

        public void EndFares(int idEntrance)
        {
        }

        public event NoRetValueHandler<int> OnEndPlan = delegate { };
        public event NewSignalStateEventHandler OnNewSignalState = delegate { };
        public event StartPlanEventHandler OnStartPlan = delegate { };
    }
}