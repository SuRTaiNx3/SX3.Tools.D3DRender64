using SX3.Tools.D3DRender64.Events;
using System;
using WF = System.Windows.Forms;
using SharpDX;
using SX3.Tools.D3DRender64.Utils;

namespace SX3.Tools.D3DRender64.Controls
{
    public class CheckBox : Checkable
    {
        #region Constructor

        public CheckBox()
            : base()
        {
            this.Text = "<CheckBox>";
            this.MouseClickEventUp += CheckBox_MouseClickEventUp;
            this.FillParent = true;
        }

        #endregion

        #region Methods

        private void CheckBox_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button == WF.MouseButtons.Left)
                this.Checked = !this.Checked;
        }

        public override void Draw(UIRenderer renderer)
        {
            base.Draw(renderer);
            float fontSize = (float)Math.Ceiling(this.Font.FontSize);
            Size2F size = renderer.MeasureString(this.Font, this.Text);
            
            if (!this.FillParent)
                this.Width = size.Height + fontSize;

            this.Height = size.Height;
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 box = new Vector2(fontSize, fontSize);
            Vector2 boxLocation = new Vector2(location.X, location.Y + this.Height / 2f - box.Y / 2f);

            if (this.MouseOver)
            {
                renderer.DrawFilledBox(location.X - MarginLeft, location.Y - MarginTop,
                                        this.Width + MarginLeft + MarginRight, this.Height + MarginTop + MarginBottom,
                                        this.BackColor);
            }

            renderer.DrawBox(boxLocation.X, boxLocation.Y, box.X, box.Y, 1, this.ForeColor);

            if (this.Checked)
            {
                Vector2 loc = boxLocation + Vector2.One * 2;
                Vector2 boxSize = box - Vector2.One * 4;
                renderer.DrawFilledBox(loc.X, loc.Y, boxSize.X, boxSize.Y, this.ForeColor);
            }

            renderer.DrawText(this.Font, this.Text, location.X + box.X + MarginLeft, location.Y, this.ForeColor);
        }

        public override void ApplySettings(ConfigUtils config)
        {
            if (this.Tag != null)
                if (config.HasKey(this.Tag.ToString()))
                    this.Checked = config.GetValue<bool>(this.Tag.ToString());
        }

        #endregion
    }
}
