using SharpDX;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using WF = System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Controls
{
    public class RadioButton : Checkable
    {
        #region Properties

        public string GroupName { get; set; }

        #endregion

        #region Constructor

        public RadioButton() : base()
        {
            this.Text = "<RadioButton>";
            this.FillParent = true;
            this.MouseClickEventUp += RadioButton_MouseClickEventUp;
            this.CheckedChangedEvent += RadioButton_CheckedChangedEvent;
        }

        #endregion

        #region Methods

        private void RadioButton_CheckedChangedEvent(object sender, EventArgs e)
        {
            if (this.Checked && this.Parent != null)
            {
                foreach (ContentControl control in this.Parent.ChildControls)
                {
                    if (control == this)
                        continue;

                    if (control is RadioButton)
                    {
                        RadioButton rdb = (RadioButton)control;
                        if (rdb.Checked && rdb.GroupName == this.GroupName)
                            rdb.Checked = false;
                    }
                }
            }
        }

        private void RadioButton_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button == WF.MouseButtons.Left && !this.Checked)
                this.Checked = true;
        }

        public override void Draw(UIRenderer renderer)
        {
            base.Draw(renderer);
            float fontSize = (float)Math.Ceiling(this.Font.FontSize);
            Size2F size = renderer.MeasureString(this.Font, this.Text);
            
            if (!this.FillParent)
                this.Width = size.Width + fontSize;

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

            renderer.DrawCircle(boxLocation.X, boxLocation.Y, fontSize, 1, this.ForeColor);

            if (this.Checked)
            {
                float rad = (box - Vector2.One * 4).X;
                Vector2 loc = boxLocation + Vector2.One * 2;
                renderer.DrawFilledCircle(loc.X, loc.Y, rad, this.ForeColor);
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
