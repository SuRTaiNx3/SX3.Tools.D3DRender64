using SX3.Tools.D3DRender64.Events;
using System;
using System.Threading;

namespace SX3.Tools.D3DRender64
{
    public class Updater
    {
        #region Globals

        private Thread _thread;
        private bool _work;
        private long _lastTick;
        private long _fpsTick;
        private long _begin;

        #endregion

        #region Properties

        public int Interval { get; set; }
        public int TickCount { get; private set; }
        public int FrameRate { get; private set; }
        public int LastFrameRate { get; private set; }

        #endregion

        #region Events

        public event EventHandler<DeltaEventArgs> TickEvent;
        public virtual void OnTickEvent(DeltaEventArgs e)
        {
            if (TickEvent != null)
                TickEvent(this, e);
        }

        #endregion

        #region Constructor

        public Updater() : this(60) { }

        public Updater(int tickRate)
        {
            this.Interval = 1000 / tickRate;
            this.TickCount = 0;
        }

        #endregion

        #region Methods

        public void StartUpdater()
        {
            if (_thread != null)
                StopUpdater();

            _work = true;
            _begin = DateTime.Now.Ticks;
            this._thread = new Thread(new ThreadStart(Loop));
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.Highest;
            _thread.Start();
        }

        public void StopUpdater()
        {
            _work = false;
            if (_thread == null)
                return;
            if (_thread.ThreadState == ThreadState.Running)
                _thread.Abort();
            _thread = null;
        }

        private void Loop()
        {
            _lastTick = DateTime.Now.Ticks;
            while (_work)
            {
                CalculateFPS();
                double elapsedSeconds = new TimeSpan(DateTime.Now.Ticks - _lastTick).TotalSeconds;
                this.OnTickEvent(new DeltaEventArgs(elapsedSeconds));
                this.TickCount++;
                _lastTick = DateTime.Now.Ticks;
                Thread.Sleep(this.Interval);
            }
        }

        public void CalculateFPS()
        {
            if (DateTime.Now.Ticks - _fpsTick >= TimeSpan.TicksPerSecond)
            {
                LastFrameRate = FrameRate;
                FrameRate = 0;
                _fpsTick = DateTime.Now.Ticks;
            }
            FrameRate++;
        }

        public TimeSpan GetRuntime()
        {
            return new TimeSpan(DateTime.Now.Ticks - _begin);
        }

        public int GetAverageFPS()
        {
            return (int)(this.TickCount / this.GetRuntime().TotalSeconds);
        }

        #endregion
    }
}
