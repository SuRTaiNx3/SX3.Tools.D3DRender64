using SharpDX;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Linq;
using WF = System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Controls
{
    public class TabControl : Panel
    {
        #region Globals

        private int _selectedIndex;

        #endregion

        #region Properties

        public float MinimumHeaderWidth { get; set; }

        private RectangleF[] TabHeaders { get; set; }

        public int SelectedIndex
        {
            get { return this._selectedIndex; }
            set
            {
                if (this._selectedIndex != value && value >= 0 && value < ChildControls.Count)
                {
                    foreach (ContentControl panel in ChildControls)
                        panel.Visible = false;

                    this._selectedIndex = value;
                    ChildControls[this._selectedIndex].Visible = true;
                }
            }
        }

        #endregion

        #region Constructor

        public TabControl() : base()
        {
            this.MouseClickEventUp += TabControl_MouseClickEventUp;
            this.FontChangedEvent += TabControl_FontChangedEvent;
            this.MinimumHeaderWidth = 50f;
        }

        #endregion

        #region Methods

        void TabControl_FontChangedEvent(object sender, EventArgs e)
        {
            foreach (ContentControl control in this.ChildControls)
                control.Font = this.Font;
        }

        private void TabControl_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button != WF.MouseButtons.Left)
                return;
            if (TabHeaders == null)
                return;

            Vector2 cursorPoint = (Vector2)e.PosOnForm - this.GetAbsoluteLocation();
            RectangleF cursor = new RectangleF(cursorPoint.X, cursorPoint.Y, 1, 1);
            for (int i = 0; i < TabHeaders.Length; i++)
            {
                if (TabHeaders[i].Intersects(cursor))
                {
                    this.SelectedIndex = i;
                    break;
                }
            }
        }
        public override void Update(double secondsElapsed, InputUtils inputUtils, SharpDX.Vector2 cursorPoint, bool checkMouse = false)
        {
            base.Update(secondsElapsed, inputUtils, cursorPoint, checkMouse);
            if (TabHeaders == null)
                return;

            float maxHeight = TabHeaders.Max(x => x.Height);
            this.Height = ChildControls[this.SelectedIndex].Height + maxHeight;
            this.ChildControls[this.SelectedIndex].Y = maxHeight;
            this.Width = TabHeaders[TabHeaders.Length - 1].X + TabHeaders[TabHeaders.Length - 1].Width;
        }

        public override void Draw(UIRenderer renderer)
        {
            base.Draw(renderer);

            if (this.ChildControls.Count == 0)
                return;

            TabHeaders = new RectangleF[ChildControls.Count];
            int idx = 0;
            Vector2 location = this.GetAbsoluteLocation();

            foreach (ContentControl panel in ChildControls)
            {
                Size2F size = renderer.MeasureString(this.Font, panel.Text);
                if (idx == 0)
                    TabHeaders[idx] = new RectangleF(0, 0, (float)Math.Max(MinimumHeaderWidth, size.Width + this.MarginLeft + this.MarginRight), size.Height);
                else
                    TabHeaders[idx] = new RectangleF(TabHeaders[idx - 1].X + TabHeaders[idx - 1].Width, TabHeaders[idx - 1].Y, (float)Math.Max(MinimumHeaderWidth, size.Width + this.MarginLeft + this.MarginRight), size.Height);

                Vector2 tabLocation = location + new Vector2(TabHeaders[idx].X, TabHeaders[idx].Y);

                renderer.DrawFilledBox(tabLocation.X, tabLocation.Y, TabHeaders[idx].Width, TabHeaders[idx].Height, this.BackColor);

                if (this.SelectedIndex == idx)
                    renderer.DrawFilledBox(tabLocation.X, tabLocation.Y, TabHeaders[idx].Width, TabHeaders[idx].Height, this.ForeColor * 0.1f);

                renderer.DrawBox(tabLocation.X, tabLocation.Y, TabHeaders[idx].Width, TabHeaders[idx].Height, 1, this.ForeColor);
                Vector2 loc = tabLocation + Vector2.UnitX * this.MarginLeft;
                renderer.DrawText(this.Font, panel.Text, loc.X, loc.Y, this.ForeColor);
                idx++;
            }
        }

        public override void AddChildControl(ControlBase control)
        {
            base.AddChildControl(control);
            control.Visible = this.SelectedIndex == this.ChildControls.IndexOf(control);
        }

        #endregion
    }
}
