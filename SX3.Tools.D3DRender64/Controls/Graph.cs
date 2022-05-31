using SharpDX;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Linq;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Graph : ContentControl
    {
        #region Globals

        private long _minimum;
        private long _maximum;
        private long _numberofvalues;

        #endregion

        #region Properties

        public long NumberOfValues
        {
            get { return _numberofvalues; }
            set
            {
                if (_numberofvalues != value)
                {
                    _numberofvalues = value;
                    long[] newValues = new long[_numberofvalues];
                    if (this.Values == null)
                        this.Values = newValues;
                    else if (this.Values.Length > newValues.Length)
                        Array.Copy(this.Values, 0, newValues, 0, newValues.Length);
                    else if (this.Values.Length < newValues.Length)
                        Array.Copy(this.Values, 0, newValues, 0, this.Values.Length);

                    this.Values = newValues;
                }
            }
        }

        public long Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;
                    this.Values = this.Values.Select(x => x < _minimum ? _minimum : x).ToArray();
                }
            }
        }

        public long Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;
                    this.Values = this.Values.Select(x => x > _maximum ? _maximum : x).ToArray();
                }
            }
        }

        public bool DynamicMaximum { get; set; }

        public long LastValue { get; set; }

        private long[] Values { get; set; }

        #endregion

        #region Constructor

        public Graph() : base()
        {
            this.NumberOfValues = 100;
            this.Minimum = 0;
            this.Maximum = 100;
            this.DynamicMaximum = false;
        }

        #endregion

        #region Methods

        public void AddValue(long value)
        {
            long nextValue = value - LastValue;
            LastValue = value;

            long[] newValues = new long[NumberOfValues];
            Array.Copy(Values, 1, newValues, 0, NumberOfValues - 1);
            newValues[newValues.Length - 1] = nextValue;

            this.Values = newValues;
        }

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();

            renderer.DrawFilledBox(location.X, location.Y, size.X, size.Y, this.BackColor);
            renderer.DrawBox(location.X, location.Y, size.X, size.Y, 1, this.ForeColor);

            Vector2[] points = new Vector2[NumberOfValues];
            long max = DynamicMaximum ? this.Values.Max() : this.Maximum;
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = location + new Vector2(this.Width / (points.Length - 1) * i, this.Height - (this.Height - this.MarginTop - this.MarginBottom) / max * this.Values[i]);
            }
            renderer.DrawLines(this.ForeColor, points);

            string maxString = MiscUtils.GetUnitFromSize(max, true);
            Size2F maxSize = renderer.MeasureString(this.Font, maxString);
            Vector2 maxLocation = location + new Vector2(this.Width - maxSize.Width, 0);
            renderer.DrawText(this.Font, maxString, maxLocation.X, maxLocation.Y, this.ForeColor);
            renderer.DrawText(this.Font, this.Text, location.X, location.Y, this.ForeColor);
            base.Draw(renderer);
        }

        #endregion
    }
}
