namespace SX3.Tools.D3DRender64.Controls.Layouts
{
    public class LinearLayout : Layout
    {
        #region Globals

        private static LinearLayout _instance = new LinearLayout();

        #endregion

        #region Properties

        public static Layout Instance { get { return _instance; } }

        #endregion

        #region Constructor

        private LinearLayout() : base(){}

        #endregion

        #region Methods

        public override void ApplyLayout(ContentControl parent)
        {
            for (int i = 0; i < parent.ChildControls.Count; i++)
            {
                var control = parent.ChildControls[i];
                if (!control.Visible)
                    continue;

                if (control.FillParent)
                    control.Width = parent.Width - parent.MarginLeft - parent.MarginRight - control.MarginLeft - control.MarginRight;

                if (i == 0)
                {
                    control.X = control.MarginLeft + parent.MarginLeft;
                    control.Y = control.MarginTop;
                }
                else
                {
                    var lastControl = parent.ChildControls[i - 1];
                    control.X = lastControl.X;
                    control.Y = lastControl.Y + lastControl.Height + lastControl.MarginBottom + control.MarginTop;
                }
            }
        }

        #endregion
    }
}
