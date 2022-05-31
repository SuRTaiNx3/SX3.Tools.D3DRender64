using SharpDX;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Controls
{
    public class XWindow : Panel
    {
        #region Globals

        private bool _mouseDown;

        #endregion

        #region Properties

        public Vector2 TitleBarSize { get; set; }
        public Label Caption { get; set; }
        public Panel Panel { get; set; }

        #endregion

        #region Constructor

        public XWindow() : base()
        {
            this.Caption = new Label();
            this.Panel = new Panel();
            this.Panel.DrawBackground = false;
            this.Panel.DrawBorder = false;
            this._mouseDown = false;
            //this.DynamicHeight = false;
            //this.DynamicWidth = false;

            this.AddChildControl(this.Caption);
            this.AddChildControl(this.Panel);

            this.MouseClickEventUp += Window_MouseClickEventUp;
            this.MouseClickEventDown += Window_MouseClickEventDown;
            this.MouseLeftEvent += Window_MouseLeftEvent;
            this.MouseMovedEvent += Window_MouseMovedEvent;
            this.TextChangedEvent += Window_TextChangedEvent;
        }

        #endregion

        #region Methods

        public override void Update(double secondsElapsed, InputUtils keyUtils, Vector2 cursorPoint, bool checkMouse = false)
        {
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
        }

        private void Window_TextChangedEvent(object sender, EventArgs e)
        {
            this.Caption.Text = this.Text;
        }

        private void Window_MouseMovedEvent(object sender, MouseEventExtArgs e)
        {
            if (_mouseDown)
            {
                var lastpos = (Vector2)e.PosOnForm;
                Vector2 offset = new Vector2(lastpos.X - this.LastMousePos.X, lastpos.Y - this.LastMousePos.Y);
                this.X += offset.X;
                this.Y += offset.Y;
            }
        }

        private void Window_MouseLeftEvent(object sender, EventArgs e)
        {
            _mouseDown = false;
        }

        private void Window_MouseClickEventDown(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _mouseDown = true;
        }

        private void Window_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _mouseDown = false;
        }

        #endregion
    }
}
