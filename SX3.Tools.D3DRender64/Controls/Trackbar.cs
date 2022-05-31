using SharpDX;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using WF = System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Trackbar : ContentControl
    {
        #region Globals

        private float _value;
        private Vector2 _trackbarLocation;

        #endregion

        #region Properties

        public float Minimum { get; set; }

        public float Maximum { get; set; }

        public float Value
        {
            get { return this._value; }
            set
            {
                if (this._value != value)
                {
                    if (value < Minimum)
                        value = Minimum;
                    if (value > Maximum)
                        value = Maximum;
                    this._value = value;
                    OnValueChangedEvent(new EventArgs());
                }
            }
        }

        public int NumberOfDecimals { get; set; }

        public float TrackbarHeight { get; set; }

        public float Percent
        {
            get
            {
                return 1f / Math.Abs(this.Minimum - this.Maximum) * (this._value - (float)Math.Abs(this.Minimum));
            }
        }

        #endregion

        #region Events

        public event EventHandler ValueChangedEvent;
        protected virtual void OnValueChangedEvent(EventArgs e)
        {
            if (ValueChangedEvent != null)
                ValueChangedEvent(this, e);
        }

        #endregion

        #region Constructor

        public Trackbar() : base()
        {
            this.Minimum = 0;
            this.Maximum = 100;
            this._value = 50;
            this.NumberOfDecimals = 2;
            this.TrackbarHeight = 16f;
            this.FillParent = true;
            this.MouseMovedEvent += Trackbar_MouseMovedEvent;
            this.MouseWheelEvent += Trackbar_MouseWheelEvent;
        }

        #endregion

        #region Methods

        private void Trackbar_MouseMovedEvent(object sender, MouseEventExtArgs e)
        {
            if (e.Button != WF.MouseButtons.Left)
                return;

            Vector2 size = this.GetSize();

            Vector2 trackbarSize = new Vector2(size.X - TrackbarHeight, 0);
            Vector2 cursorPos = new Vector2(((Vector2)e.PosOnForm).X - _trackbarLocation.X, ((Vector2)e.PosOnForm).Y - _trackbarLocation.Y);


            if (cursorPos.X >= 0 && cursorPos.X <= trackbarSize.X)
            {
                if (cursorPos.Y >= -TrackbarHeight && cursorPos.Y <= TrackbarHeight)
                {
                    float percent = 1f / trackbarSize.X * cursorPos.X;
                    float range = Math.Abs(this.Minimum - this.Maximum);
                    float val = range * percent;
                    this.Value = this.Minimum + val;
                }
            }

        }

        private void Trackbar_MouseWheelEvent(object sender, MouseEventExtArgs e)
        {
            if (!e.Wheel)
                return;

            float percent = this.Value / this.Maximum;
            if (e.UpOrDown == MouseEventExtArgs.UpDown.Up)
                percent += 0.01f;
            if (e.UpOrDown == MouseEventExtArgs.UpDown.Down)
                percent -= 0.01f;
            float range = Math.Abs(this.Minimum - this.Maximum);
            float val = range * percent;
            this.Value = this.Minimum + val;
        }

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();
            Vector2 marginLocation = location - Vector2.UnitX * this.MarginLeft - Vector2.UnitY * this.MarginTop;
            Vector2 marginSize = size + Vector2.UnitX * this.MarginLeft + Vector2.UnitX * this.MarginRight + Vector2.UnitY * this.MarginTop + Vector2.UnitY * this.MarginBottom;

            //renderer.FillRectangle(this.BackColor, marginLocation, marginSize);
            //renderer.DrawRectangle(this.ForeColor, marginLocation, marginSize);

            string text = string.Format("{0} {1}", this.Text, Math.Round(this._value, this.NumberOfDecimals));
            Size2F textSize = renderer.MeasureString(this.Font, text);

            renderer.DrawText(this.Font, text, location.X, location.Y, this.ForeColor);

            _trackbarLocation = location + Vector2.UnitY * textSize.Height;
            Vector2 trackBarHandleSize = new Vector2(TrackbarHeight, TrackbarHeight);

            _trackbarLocation = location + Vector2.UnitY * (textSize.Height + MarginTop + TrackbarHeight / 2f) + Vector2.UnitX * TrackbarHeight / 2f;
            Vector2 trackbarSize = new Vector2(size.X - TrackbarHeight, 0);
            Vector2 trackbarMarkerLocation = new Vector2(_trackbarLocation.X + trackbarSize.X * Percent, _trackbarLocation.Y);
            Vector2 trackbarMarkerSize = new Vector2(TrackbarHeight / 4f, TrackbarHeight);

            renderer.DrawLine(_trackbarLocation, _trackbarLocation + trackbarSize, TrackbarHeight / 4f + 2f, this.ForeColor);
            renderer.DrawLine(_trackbarLocation, _trackbarLocation + trackbarSize, TrackbarHeight / 4f, this.BackColor);

            Vector2 loc = trackbarMarkerLocation - (trackbarMarkerSize + 2f);
            Vector2 boxSize = trackbarMarkerSize + 2f;

            renderer.DrawFilledBox(loc.X, loc.Y, boxSize.X, boxSize.Y, this.ForeColor);

            Vector2 loc2 = trackbarMarkerLocation - trackbarMarkerSize / 2f;
            renderer.DrawFilledBox(loc2.X, loc2.Y, trackbarMarkerSize.X, trackbarMarkerSize.Y, this.BackColor);

            this.Height = textSize.Height + TrackbarHeight + MarginTop + MarginBottom;

            base.Draw(renderer);
        }

        public override void ApplySettings(ConfigUtils config)
        {
            if (this.Tag != null)
                if (config.HasKey(this.Tag.ToString()))
                    this.Value = config.GetValue<float>(this.Tag.ToString());
        }

        public void SetValue(float value)
        {
            if (value < Minimum)
                value = Minimum;
            if (value > Maximum)
                value = Maximum;
            this._value = value;
        }

        #endregion
    }
}
