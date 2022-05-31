using SX3.Tools.D3DRender64.Controls.Misc;
using SX3.Tools.D3DRender64.Utils;
using System;

namespace SX3.Tools.D3DRender64.Controls
{
    public class ColorControl : Panel
    {
        #region Globals

        private SX3Color _color;
        private Trackbar A, R, G, B;
        private Button _preview;

        #endregion

        #region Properties

        public SharpDX.Color SDXColor { get; private set; }

        public SX3Color Color
        {
            get { return this._color; }
            set
            {
                if (value.ToARGB() != _color.ToARGB())
                {
                    this._color = value;
                    SDXColor = new SharpDX.Color(_color.R, _color.G, _color.B, _color.A);
                    this.A.SetValue(this._color.A / 255f);
                    this.R.SetValue(this._color.R / 255f);
                    this.G.SetValue(this._color.G / 255f);
                    this.B.SetValue(this._color.B / 255f);
                    this._preview.BackColor = SDXColor;
                    this._preview.ForeColor = new SharpDX.Color(255 - SDXColor.R, 255 - SDXColor.G, 255 - SDXColor.B);
                    this.OnColorChangedEvent(new EventArgs());
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler ColorChangedEvent;
        protected virtual void OnColorChangedEvent(EventArgs e)
        {
            if (ColorChangedEvent != null)
                ColorChangedEvent(this, e);
        }

        #endregion

        #region Constructor

        public ColorControl() : base()
        {
            this._color = new SX3Color(0, 0, 0, 0);
            this.TextChangedEvent += ColorControl_TextChangedEvent;
            this.DynamicWidth = false;
            this.FillParent = true;

            _preview = new Button();
            _preview.FillParent = true;
            _preview.Text = "";
            this.AddChildControl(_preview);

            SetupChannel(ref this.A, "Alpha-channel", this._color.A);
            SetupChannel(ref this.R, "R-channel", this._color.R);
            SetupChannel(ref this.G, "G-channel", this._color.G);
            SetupChannel(ref this.B, "B-channel", this._color.B);

            this.Color = new SX3Color(1f, 1f, 1f, 1f);
        }

        #endregion

        #region Methods

        private void ColorControl_TextChangedEvent(object sender, EventArgs e)
        {
            _preview.Text = this.Text;
        }

        private void SetupChannel(ref Trackbar control, string channel, byte value)
        {
            control = new Trackbar();
            control.Minimum = 0;
            control.Maximum = 1;
            control.NumberOfDecimals = 4;
            control.Value = value / 255f;
            control.ValueChangedEvent += control_ValueChangedEvent;
            control.FillParent = true;
            control.Text = channel;
            this.AddChildControl(control);
        }

        private void control_ValueChangedEvent(object sender, EventArgs e)
        {
            this.Color = new SX3Color(A.Value, R.Value, G.Value, B.Value);
        }

        public override void ApplySettings(ConfigUtils config)
        {
            if (this.Tag != null)
                if (config.HasKey(this.Tag.ToString()))
                    this.Color = SX3Color.FromFormat(config.GetValue<uint>(this.Tag.ToString()), SX3Color.ColorFormat.RGBA);
        }

        #endregion
    }
}
