using System;

namespace SX3.Tools.D3DRender64.Events
{
    public class OverlayEventArgs : EventArgs
    {
        public UIOverlay Overlay { get; private set; }
        
        public OverlayEventArgs(UIOverlay overlay)
            : base()
        {
            this.Overlay = overlay;
        }
    }
}
