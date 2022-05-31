namespace SX3.Tools.D3DRender64.Controls.Layouts
{
    public class NoneLayout : Layout
    {
        #region Globals

        private static NoneLayout instance = new NoneLayout();
        public static Layout Instance { get { return instance; } }

        #endregion

        #region Constructor

        private NoneLayout() : base(){}

        #endregion

        #region Methods

        public override void ApplyLayout(ContentControl parent){ }

        #endregion
    }
}
