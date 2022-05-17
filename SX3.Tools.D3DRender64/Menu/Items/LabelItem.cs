﻿

using SharpDX;

namespace SX3.Tools.D3DRender.Menu.Items
{
    public class LabelItem : MenuItem
    {
        #region Constructor

        public LabelItem(string title)
        {
            this.Title = title;
        }

        #endregion

        #region Methods

        public override void Draw(UIRenderer ui, int valueX, int labelX, int y)
        {
            ui.DrawText("default_font", Title, labelX, y, new Color(20, 20, 20));
        }

        #endregion
    }
}
