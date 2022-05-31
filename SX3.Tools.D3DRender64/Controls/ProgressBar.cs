using SharpDX;

namespace SX3.Tools.D3DRender64.Controls
{
    public class ProgressBar : ContentControl
    {
        #region Globals

        private float _minimum;
        private float _maximum;
        private float _curValue;

        #endregion

        #region Properties

        public float Maximum
        {
            get { return _maximum; }
            set
            {
                if (value > Minimum)
                {
                    _maximum = value;
                    if (Value > Maximum)
                        _curValue = Maximum;
                }
            }
        }

        public float Minimum
        {
            get { return _minimum; }
            set
            {
                if (value < Maximum)
                {
                    _minimum = value;
                    if (Value < Minimum)
                        _curValue = Minimum;
                }
            }
        }

        public float Value
        {
            get { return _curValue; }
            set
            {
                if (Minimum > value)
                    _curValue = Minimum;
                else if (Maximum < value)
                    _curValue = Maximum;
                else
                    _curValue = value;
            }
        }

        public Color FillColor { get; set; }

        #endregion

        #region Constructor

        public ProgressBar()
        {
            _minimum = 0;
            _curValue = 50;
            _maximum = 100;
            this.FillColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            this.Height = 14f;
        }

        #endregion

        #region Methods

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();
            Vector2 fillSize = new Vector2(size.X * (this.Value / this.Maximum), size.Y);

            renderer.DrawFilledBox(location.X, location.Y, size.X, size.Y, this.BackColor);
            renderer.DrawFilledBox(location.X, location.Y, fillSize.X, fillSize.Y, this.FillColor);
            renderer.DrawBox(location.X, location.Y, size.X, size.Y, 1, this.ForeColor);

            this.FillParent = true;
            base.Draw(renderer);
        }

        #endregion
    }
}
