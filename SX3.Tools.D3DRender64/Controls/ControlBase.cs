using SharpDX;
using SharpDX.DirectWrite;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Controls
{
    public abstract class ControlBase
    {
        #region Globals

        private bool _mouseOver;
        private bool _visible;
        private string _text;

        private bool _reassignFont = false;
        private string _fontKey;
        private TextFormat _font;

        public enum TextAlignment { Left, Center, Right };

        #endregion

        #region Properties

        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public Color BackColor { get; set; }

        public Color ForeColor { get; set; }

        public TextFormat Font
        {
            get { return this._font; }
            set
            {
                if (this._font == null || !this._font.Equals(value))
                {
                    this._font = value;
                    OnFontChangedEvent(new EventArgs());
                }
            }
        }

        public RectangleF Rectangle { get { return new RectangleF(this.X, this.Y, this.Width, this.Height); } }

        public ControlBase Parent { get; set; }

        public List<ControlBase> ChildControls { get; set; }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    OnTextChangedEvent(new EventArgs());
                }
            }
        }

        public bool MouseOver
        {
            get
            {
                return _mouseOver;
            }
            protected set
            {
                if (_mouseOver != value)
                {
                    _mouseOver = value;
                    if (value)
                        OnMouseEnteredEvent(new EventArgs());
                    else
                        OnMouseLeftEvent(new EventArgs());
                }
            }
        }

        public float MarginTop { get; set; }
        public float MarginBottom { get; set; }
        public float MarginLeft { get; set; }
        public float MarginRight { get; set; }

        public bool Visible
        {
            get { return this._visible; }
            set
            {
                if (this._visible != value)
                {
                    this._visible = value;
                    OnVisibleChangedEvent(new EventArgs());
                }
            }
        }

        public bool FillParent { get; set; }

        public Vector2 LastMousePos { get; private set; }

        public object Tag { get; set; }

        public TextAlignment TextAlign { get; set; }

        #endregion

        #region Structs

        protected struct MouseEvent
        {
            public bool Handled;
            public int Depth;
        }

        #endregion

        #region Events

        public event EventHandler MouseEnteredEvent;
        protected virtual void OnMouseEnteredEvent(EventArgs e)
        {
            if (MouseEnteredEvent != null)
                MouseEnteredEvent(this, e);
        }

        public event EventHandler MouseLeftEvent;
        protected virtual void OnMouseLeftEvent(EventArgs e)
        {
            if (MouseLeftEvent != null)
                MouseLeftEvent(this, e);
        }

        public event EventHandler TextChangedEvent;
        protected virtual void OnTextChangedEvent(EventArgs e)
        {
            if (TextChangedEvent != null)
                TextChangedEvent(this, e);
        }

        public event EventHandler FontChangedEvent;
        protected virtual void OnFontChangedEvent(EventArgs e)
        {
            if (FontChangedEvent != null)
                FontChangedEvent(this, e);
        }

        public event EventHandler VisibleChangedEvent;
        protected virtual void OnVisibleChangedEvent(EventArgs e)
        {
            if (VisibleChangedEvent != null)
                VisibleChangedEvent(this, e);
        }

        public event EventHandler<MouseEventExtArgs> MouseMovedEvent;
        protected virtual void OnMouseMovedEvent(MouseEventExtArgs e)
        {
            if (MouseMovedEvent != null)
                MouseMovedEvent(this, e);
        }

        public event EventHandler<MouseEventExtArgs> MouseClickEventDown;
        protected virtual void OnMouseClickEventDown(MouseEventExtArgs e)
        {
            if (MouseClickEventDown != null)
                MouseClickEventDown(this, e);
        }

        public event EventHandler<MouseEventExtArgs> MouseClickEventUp;
        protected virtual void OnMouseClickEventUp(MouseEventExtArgs e)
        {
            if (MouseClickEventUp != null)
                MouseClickEventUp(this, e);
        }

        public event EventHandler<MouseEventExtArgs> MouseWheelEvent;
        protected virtual void OnMouseWheelEvent(MouseEventExtArgs e)
        {
            if (MouseWheelEvent != null)
                MouseWheelEvent(this, e);
        }

        #endregion

        #region Constructor

        public ControlBase()
        {
            this.X = 0f;
            this.Y = 0f;
            this.Width = 0f;
            this.Height = 0f;
            this.Parent = null;
            this.ChildControls = new List<ControlBase>();
            this.Text = "<Control>";
            this.Visible = true;
        }

        #endregion

        #region Methods

        public virtual void SetFontByKey(string fontKey)
        {
            _fontKey = fontKey;
            _reassignFont = true;
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="renderer"></param>
        public virtual void Draw(UIRenderer renderer)
        {
            if (_reassignFont)
                this.Font = renderer.GetFontOrDefault(_fontKey);

            foreach (ControlBase control in ChildControls)
                if (control.Visible)
                    control.Draw(renderer);
        }

        /// <summary>
        /// Performs an update on this control and its chilcontrols
        /// </summary>
        /// <param name="secondsElapsed"></param>
        /// <param name="cursorPoint"></param>
        public virtual void Update(double secondsElapsed, InputUtils inputUtils, Vector2 cursorPoint, bool checkMouse = false)
        {
            bool canUpdate = this._visible;
            if (this.Parent != null)
                if (!this.Parent.Visible)
                    canUpdate = false;

            if (canUpdate && checkMouse)
            {
                MouseEvent result = new MouseEvent() { Handled = false, Depth = 0 };
                CheckMouseEvents(cursorPoint, inputUtils, ref result);
            }
            #region CHILDCONTROLS
            foreach (ControlBase control in ChildControls)
                control.Update(secondsElapsed, inputUtils, cursorPoint, false);
            #endregion
        }

        /// <summary>
        /// Checks whether the mouse left or entered this control (or one of its childcontrols)
        /// </summary>
        /// <param name="cursorPoint"></param>
        /// <param name="result"></param>
        protected void CheckMouseEvents(Vector2 cursorPoint, InputUtils inputUtils, ref MouseEvent result)
        {
            inputUtils.Mouse.CurrentMouseArgs.PosOnForm = cursorPoint;
            foreach (ControlBase control in ChildControls)
            {
                if (result.Handled)
                    return;
                if (!control.Visible)
                    continue;
                if (!inputUtils.MouseChanged)
                    continue;
                result.Depth++;
                control.CheckMouseEvents(cursorPoint, inputUtils, ref result);
                result.Depth--;
            }
            if (!result.Handled)
            {
                this.MouseOver = this.CheckMouseOver(cursorPoint);
                if (this.MouseOver)
                {
                    result.Handled = true;
                    if (!inputUtils.MouseChanged)
                        return;
                    if ((inputUtils.Mouse.CurrentMouseArgs.Button == MouseButtons.Left
                        || inputUtils.Mouse.CurrentMouseArgs.Button == MouseButtons.Right
                        || inputUtils.Mouse.CurrentMouseArgs.Button == MouseButtons.Middle
                        || inputUtils.Mouse.CurrentMouseArgs.Wheel)
                        && inputUtils.Mouse.CurrentMouseArgs.UpOrDown == MouseEventExtArgs.UpDown.Down)
                        OnMouseClickEventDown(inputUtils.Mouse.CurrentMouseArgs);
                    if ((inputUtils.Mouse.CurrentMouseArgs.Button == MouseButtons.Left
                        || inputUtils.Mouse.CurrentMouseArgs.Button == MouseButtons.Right
                        || inputUtils.Mouse.CurrentMouseArgs.Button == MouseButtons.Middle
                        || inputUtils.Mouse.CurrentMouseArgs.Wheel)
                        && inputUtils.Mouse.CurrentMouseArgs.UpOrDown == MouseEventExtArgs.UpDown.Up)
                        OnMouseClickEventUp(inputUtils.Mouse.CurrentMouseArgs);
                    if (inputUtils.Mouse.CurrentMouseArgs.Wheel)
                        OnMouseWheelEvent(inputUtils.Mouse.CurrentMouseArgs);

                    if (!LastMousePos.Equals(cursorPoint))
                    {
                        OnMouseMovedEvent(inputUtils.Mouse.CurrentMouseArgs);
                        LastMousePos = cursorPoint;
                    }
                }
            }
        }

        /// <summary>
        /// Whether the mouse is over this control
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract bool CheckMouseOver(Vector2 cursorPoint);

        /// <summary>
        /// Adds a control to the childcontrols of this control
        /// </summary>
        /// <param name="control"></param>
        public virtual void AddChildControl(ControlBase control)
        {
            control.Parent = this;
            this.ChildControls.Add(control);
        }

        /// <summary>
        /// Removes a control from this control's childcontrols
        /// </summary>
        /// <param name="control"></param>
        public virtual void RemoveChildControl(ControlBase control)
        {
            this.ChildControls.Remove(control);
            control.Parent = null;
        }

        /// <summary>
        /// Removes a control at the given index from this control's list of childcontrols
        /// </summary>
        /// <param name="index"></param>
        public virtual void RemoveChildControlAt(int index)
        {
            this.RemoveChildControl(this.ChildControls[index]);
        }

        /// <summary>
        /// Returns the location of this control
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetAbsoluteLocation();

        /// <summary>
        /// Returns the size of this control
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetSize();

        #endregion
    }
}
