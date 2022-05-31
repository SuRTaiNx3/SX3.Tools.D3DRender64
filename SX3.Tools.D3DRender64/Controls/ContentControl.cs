using SharpDX;
using SX3.Tools.D3DRender64.Utils;

namespace SX3.Tools.D3DRender64.Controls
{
    public class ContentControl : ControlBase
    {
        #region Properties

        public bool DrawBorder { get; set; }

        public bool DrawBackground { get; set; }

        #endregion

        #region Constructor

        public ContentControl()
        {
            this.ForeColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            this.BackColor = new Color(0.8f, 0.8f, 0.8f, 0.9f);
            this.MarginBottom = 2f;
            this.MarginLeft = 2f;
            this.MarginRight = 2f;
            this.MarginTop = 2f;
            this.DrawBackground = true;
            this.DrawBorder = true;
        }

        #endregion

        #region Methods

        public override bool CheckMouseOver(Vector2 cursorPoint)
        {
            Vector2 location = this.GetAbsoluteLocation();
            return
                cursorPoint.X >= location.X && cursorPoint.X <= location.X + this.Width &&
                cursorPoint.Y >= location.Y && cursorPoint.Y <= location.Y + this.Height;
        }

        public override Vector2 GetAbsoluteLocation()
        {
            if (this.Parent == null)
                return new Vector2(this.X, this.Y);
            else
                return this.Parent.GetAbsoluteLocation() + new Vector2(this.X, this.Y);
        }

        public override void Update(double secondsElapsed, InputUtils keyUtils, Vector2 cursorPoint, bool checkMouse = false)
        {
            //if (this.FillParent && this.Parent != null)
            //    this.Width = Parent.Width - Parent.MarginLeft - Parent.MarginRight - this.MarginLeft - this.MarginRight;
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
        }

        public override Vector2 GetSize()
        {
            return new Vector2(this.Width, this.Height);
        }

        public virtual void ApplySettings(ConfigUtils config){ }

        #endregion
    }
}
