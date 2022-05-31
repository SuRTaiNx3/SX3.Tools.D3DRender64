using System;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Checkable : ContentControl
    {
        #region Properties

        private bool _isChecked;
        public bool Checked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnCheckedChangedEvent(new EventArgs());
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler CheckedChangedEvent;
        protected virtual void OnCheckedChangedEvent(EventArgs e)
        {
            if (CheckedChangedEvent != null)
                CheckedChangedEvent(this, e);
        }

        #endregion
    }
}
