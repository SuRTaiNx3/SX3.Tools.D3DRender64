namespace SX3.Tools.D3DRender64.Utils
{
    public class InputUtils
    {
        #region Globals

        #endregion

        #region Properties

        public KeyUtils Keys;
        public MouseHook Mouse;

        /// <summary>
        /// If true mouse changed since last update
        /// </summary>
        public bool MouseChanged = false;

        #endregion

        #region Constructor

        public InputUtils()
        {
            Keys = new KeyUtils();
            Mouse = new MouseHook();
            Mouse.InstallHook();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates keys and mouse
        /// </summary>
        public void Update()
        {
            Keys.Update();
            MouseChanged = Mouse.Update();
        }

        #endregion
    }
}
