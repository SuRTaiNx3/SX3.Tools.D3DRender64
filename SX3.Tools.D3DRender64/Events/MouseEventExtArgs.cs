using System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Events
{
    public class MouseEventExtArgs : System.Windows.Forms.MouseEventArgs
    {
        public MouseEventExtArgs()
            : base(MouseButtons.None, 0, 0, 0, 0)
        {
        }

        public MouseEventExtArgs(MouseButtons b, int clickcount, WinAPI.POINT point, int delta)
            : base(b, clickcount, point.X, point.Y, delta)
        {
        }
        public MouseEventExtArgs(MouseButtons b, int clickcount, int x, int y, int delta)
            : base(b, clickcount, x, y, delta)
        {
        }

        /// <summary>
        /// Used by UI to save cursor position on current form
        /// </summary>
        public object PosOnForm;

        /// <summary>
        /// If mouse wheel moved
        /// </summary>
        public bool Wheel;

        /// <summary>
        /// Used to check if button is released or pressed
        /// If Wheel equals true then shows which way wheel is being turned
        /// </summary>
        public UpDown UpOrDown = UpDown.None;
        public enum UpDown
        {
            None,
            Up,
            Down
        }
    }
}
