using SharpDX;
using SX3.Tools.D3DRender64.Utils;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Spacer : ContentControl
    {
        #region Constructor

        public Spacer() : base()
        {
            this.Height = 2f;
        }

        #endregion

        #region Methods

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();
            renderer.DrawFilledBox(location.X, location.Y, size.X, size.Y, this.ForeColor);
            base.Draw(renderer);
        }

        public override void Update(double secondsElapsed, InputUtils keyUtils, Vector2 cursorPoint, bool checkMouse = false)
        {
            if (this.Parent != null)
                this.Width = Parent.Width - this.MarginLeft - this.MarginRight - Parent.MarginLeft - Parent.MarginRight;
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
        }

        #endregion
    }
}
