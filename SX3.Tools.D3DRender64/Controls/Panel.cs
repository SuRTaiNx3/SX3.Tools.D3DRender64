using SharpDX;
using SX3.Tools.D3DRender64.Controls.Layouts;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Linq;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Panel : ContentControl
    {
        #region Properties

        /// <summary>
        /// Whether this panel tightly wraps around its childcontrols or has a fixed width
        /// </summary>
        public bool DynamicWidth { get; set; }
        
        /// <summary>
        /// Whether this panel tightly wraps around its childcontrols or has a fixed height
        /// </summary>
        public bool DynamicHeight { get; set; }

        /// <summary>
        /// The layout used to automatically relocating childcontrols
        /// </summary>
        public Layout ContentLayout { get; set; }

        #endregion

        #region Constructor

        public Panel() : base()
        {
            this.DynamicWidth = true;
            this.DynamicHeight = true;
            this.BackColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            this.ContentLayout = LinearLayout.Instance;
            this.FontChangedEvent += Panel_FontChangedEvent;
        }

        #endregion

        #region Methods

        private void Panel_FontChangedEvent(object sender, EventArgs e)
        {
            foreach (ContentControl control in this.ChildControls)
                control.Font = this.Font;
        }

        public override void Update(double secondsElapsed, InputUtils keyUtils, SharpDX.Vector2 cursorPoint, bool checkMouse = false)
        {
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
            if (this.Visible)
            {
                this.ContentLayout.ApplyLayout(this);
                float width = 0, height = 0;
                ControlBase lastControl = null;

                for (int i = 0; i < this.ChildControls.Count; i++)
                {
                    var control = this.ChildControls[i];
                    if (!control.Visible)
                        continue;

                    lastControl = control;
                    if (this.DynamicWidth)
                        if (control.Width + control.MarginLeft + control.MarginRight > width)
                            width = control.Width + control.MarginLeft + control.MarginRight;
                }

                if (this.DynamicHeight)
                {
                    if (ChildControls.Count(x => x.Visible) > 0)
                        height = ChildControls.Where(x => x.Visible).Max(x => x.Y + x.Height);
                }
                //if (lastControl != null)
                //    height = lastControl.Y + lastControl.Height + lastControl.MarginBottom;
                if (this.DynamicWidth)
                    this.Width = width + this.MarginLeft + this.MarginRight;
                if (this.DynamicHeight)
                    this.Height = height + this.MarginBottom;
            }
            else
            {
                this.Height = 0;
            }
        }

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 boxLocation = new Vector2(location.X - this.MarginLeft, location.Y - this.MarginTop);
            Vector2 boxSize = new Vector2(this.Width + this.MarginLeft + this.MarginRight, this.Height + this.MarginBottom + this.MarginTop);
            if (this.DrawBackground)
                renderer.DrawFilledBox(boxLocation.X, boxLocation.Y, boxSize.X, boxSize.Y, this.BackColor);
            if (this.DrawBackground)
                renderer.DrawBox(boxLocation.X, boxLocation.Y, boxSize.X, boxSize.Y, 1, this.ForeColor);
            base.Draw(renderer);
        }

        public void InsertSpacer()
        {
            this.AddChildControl(new Spacer());
        }

        public override void ApplySettings(ConfigUtils config)
        {
            base.ApplySettings(config);
            foreach (ContentControl control in ChildControls)
                control.ApplySettings(config);
        }

        #endregion
    }
}
