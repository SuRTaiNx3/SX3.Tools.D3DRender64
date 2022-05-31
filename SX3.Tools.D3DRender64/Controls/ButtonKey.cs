using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using WF = System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Controls
{
    public class ButtonKey : Button
    {
        #region Globals

        private int _skip;
        private bool _listen;
        private WinAPI.VirtualKeyShort _key;

        #endregion

        #region Properties

        public WinAPI.VirtualKeyShort Key
        {
            get { return this._key; }
            set
            {
                if (this._key != value)
                {
                    this._key = value;
                    OnKeyChangedEvent(new EventArgs());
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler KeyChangedEvent;
        protected virtual void OnKeyChangedEvent(EventArgs e)
        {
            if (KeyChangedEvent != null)
                KeyChangedEvent(this, e);
        }

        #endregion

        #region Constructor

        public ButtonKey() : base()
        {
            _listen = false;
            this._key = WinAPI.VirtualKeyShort.XBUTTON1;
            this.MouseClickEventUp += ButtonKey_MouseClickEventUp;
        }

        #endregion

        #region Methods

        private void ButtonKey_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button != WF.MouseButtons.Left)
                return;
            _listen = true;
            _skip = 10;
        }

        public override void Draw(UIRenderer renderer)
        {
            string text = null;
            string orig = this.Text;
            if (_listen)
                text = string.Format("{0} <press key>", this.Text);
            else
                text = string.Format("{0} {1}", this.Text, this.Key);
            this.Text = text;
            base.Draw(renderer);
            this.Text = orig;
        }

        public override void Update(double secondsElapsed, InputUtils keyUtils, SharpDX.Vector2 cursorPoint, bool checkMouse = false)
        {
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
            if (_listen)
            {
                if (_skip > 0)
                {
                    _skip--;
                    return;
                }
                WinAPI.VirtualKeyShort[] buttons = keyUtils.Keys.KeysThatWentUp();
                if (buttons.Length > 0)
                {
                    Key = buttons[0];
                    _listen = false;
                }
            }
        }

        public override void ApplySettings(ConfigUtils config)
        {
            if (this.Tag != null)
                if (config.HasKey(this.Tag.ToString()))
                    this.Key = config.GetValue<WinAPI.VirtualKeyShort>(this.Tag.ToString());
        }

        #endregion
    }
}
