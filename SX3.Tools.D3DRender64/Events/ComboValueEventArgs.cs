using System;

namespace SX3.Tools.D3DRender64.Events
{
    public class ComboValueEventArgs : EventArgs
    {
        public object Tag { get; private set; }
        public object Value { get; private set; }

        public ComboValueEventArgs(object tag, object value)
        {
            this.Tag = tag;
            this.Value = value;
        }
    }
}
