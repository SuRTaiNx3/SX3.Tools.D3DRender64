using SharpDX;
using System;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Label : ContentControl
    {
        #region Globals

        #endregion

        #region Properties

        public bool FixedWidth { get; set; }

        #endregion

        #region Constructor

        public Label()
            : base()
        {
            this.Text = "<Label>";
            this.FixedWidth = false;
            this.TextAlign = Label.TextAlignment.Left;
        }

        #endregion

        #region Methods

        public override void Draw(UIRenderer renderer)
        {
            base.Draw(renderer);
            float fontSize = (float)Math.Ceiling(this.Font.FontSize);
            Size2F size = renderer.MeasureString(this.Font, this.Text);
            
            if (!this.FillParent && !this.FixedWidth)
                this.Width = size.Width;

            this.Height = size.Height;
            Vector2 location = this.GetAbsoluteLocation();
            
            switch (this.TextAlign)
            {
                case TextAlignment.Center:
                    location.X += this.Width / 2f - size.Width / 2f;
                    break;
                case TextAlignment.Right:
                    location.X += this.Width - size.Height;
                    break;
            }
            renderer.DrawText(this.Font, this.Text, location.X + MarginLeft, location.Y, this.ForeColor);
        }

        #endregion
    }
}
