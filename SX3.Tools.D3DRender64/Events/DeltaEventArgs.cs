using System;

namespace SX3.Tools.D3DRender64.Events
{
    public class DeltaEventArgs : EventArgs
    {
        public double SecondsElapsed { get; private set; }
        public UIOverlay Overlay { get; private set; }

        public DeltaEventArgs(double secondsElapsed) : base()
        {
            this.SecondsElapsed = secondsElapsed;
        }

        public DeltaEventArgs(double secondsElapsed, UIOverlay overlay) : base()
        {
            this.SecondsElapsed = secondsElapsed;
            this.Overlay = overlay;
        }
    }
}
