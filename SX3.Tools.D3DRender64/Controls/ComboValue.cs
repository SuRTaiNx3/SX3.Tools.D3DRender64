using SharpDX;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;

namespace SX3.Tools.D3DRender64.Controls
{
    public class ComboValue<T> : ContentControl
    {
        #region Globals

        private int _selectedIndex;

        #endregion

        #region Properties

        public Tuple<string, T>[] Values { get; set; }

        public int SelectedIndex
        {
            get { return this._selectedIndex; }
            set
            {
                if (this._selectedIndex != value)
                {
                    if (value >= Values.Length)
                        value %= Values.Length;
                    if (value < 0)
                        value = Values.Length + value % Values.Length;
                    this._selectedIndex = value;
                    this.OnSelectedIndexChangedEvent(new ComboValueEventArgs(this.Tag, this.Value));
                }
            }
        }

        public T Value
        {
            get { return Values[this.SelectedIndex].Item2; }
        }

        #endregion

        #region Events

        public event EventHandler<ComboValueEventArgs> SelectedIndexChangedEvent;
        protected virtual void OnSelectedIndexChangedEvent(ComboValueEventArgs e)
        {
            if (SelectedIndexChangedEvent != null)
                SelectedIndexChangedEvent(this, e);
        }

        #endregion

        #region Constructor

        public ComboValue() : base()
        {
            this.FillParent = true;
            this.TextAlign = TextAlignment.Center;
            this.MouseClickEventUp += ComboValue_MouseClickEventUp;
        }

        #endregion

        #region Methods

        private void ComboValue_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();

            Vector2 clickPoint = (Vector2)e.PosOnForm - location;
            if (clickPoint.X < size.X / 2f)
                this.SelectedIndex--;
            else
                this.SelectedIndex++;
        }

        public override void Draw(UIRenderer renderer)
        {

            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();

            if (this.MouseOver)
            {
                renderer.DrawFilledBox(location.X - MarginLeft, location.Y - MarginTop,
                                        size.X + MarginLeft + MarginRight, size.Y + MarginTop + MarginBottom,
                                        this.BackColor);
            }

            renderer.DrawBox(location.X - MarginLeft, location.Y - MarginTop,
                            size.X + MarginLeft + MarginRight, size.Y + MarginTop + MarginBottom,
                            1, this.ForeColor);
                
            float fontSize = (float)Math.Ceiling(this.Font.FontSize);
            string display = string.Format("{0}: {1}", this.Text, this.Values != null ? this.Values[this.SelectedIndex].Item1 : "<none>");
            Size2F textSize = renderer.MeasureString(this.Font, display);
            Vector2 textLocation = location;
            this.Height = textSize.Height;
            switch (this.TextAlign)
            {
                case TextAlignment.Center:
                    textLocation.X += this.Width / 2f - textSize.Width / 2f;
                    break;
                case TextAlignment.Right:
                    textLocation.X += this.Width - textSize.Width;
                    break;
            }
            renderer.DrawText(this.Font, display, textLocation.X + MarginLeft, textLocation.Y, this.ForeColor);

            renderer.DrawText(this.Font, "<", location.X, location.Y, this.ForeColor);
            textSize = renderer.MeasureString(this.Font, ">");
            textLocation = location + Vector2.UnitX * size.X - Vector2.UnitX * textSize.Width;
            renderer.DrawText(this.Font, ">", textLocation.X, textLocation.Y, this.ForeColor);
            base.Draw(renderer);
        }

        public override void ApplySettings(ConfigUtils config)
        {
            if (this.Tag != null)
            {
                if (config.HasKey(this.Tag.ToString()))
                {
                    T value = config.GetValue<T>(this.Tag.ToString());
                    for (int i = 0; i < Values.Length; i++)
                    {
                        if (Values[i].Item2.Equals(value))
                        {
                            this.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
