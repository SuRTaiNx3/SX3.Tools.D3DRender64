using SharpDX;
using System;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Button : ContentControl
    {
        #region Constructor

        public Button()
        {
            this.FillParent = true;
            this.TextAlign = TextAlignment.Center;
            this.DrawBackground = true;
        }

        #endregion

        #region Methods

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();

            if (this.MouseOver || this.DrawBackground)
            {
                renderer.DrawFilledBox(location.X - MarginLeft, location.Y - MarginTop,
                                        size.X + MarginLeft + MarginRight, size.Y + MarginTop + MarginBottom,
                                        this.BackColor);
            }
            if (this.DrawBorder)
            {
                renderer.DrawBox(location.X - MarginLeft, location.Y - MarginTop,
                                size.X + MarginLeft + MarginRight, size.Y + MarginTop + MarginBottom,
                                1, this.ForeColor);
            }

            float fontSize = (float)Math.Ceiling(this.Font.FontSize);
            Size2F textSize = renderer.MeasureString(this.Font, this.Text);

            this.Height = textSize.Height;
            switch (this.TextAlign)
            {
                case TextAlignment.Center:
                    location.X += this.Width / 2f - textSize.Width / 2f;
                    break;
                case TextAlignment.Right:
                    location.X += this.Width - textSize.Width;
                    break;
            }

            renderer.DrawText(this.Font, this.Text, location.X + MarginLeft, location.Y, this.ForeColor);

            base.Draw(renderer);
        }

        #endregion
    }
}
