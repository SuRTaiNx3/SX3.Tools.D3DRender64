using SharpDX;
using SX3.Tools.D3DRender64.Controls;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SX3.Tools.D3DRender64
{
    public class UIOverlay : Form
    {
        #region Globals

        private IntPtr _handle;

        private Updater _logicThread;
        private Updater _renderThread;

        private long _lastLogicTick;
        private long _lastRenderTick;

        private FPSCalculator _fpsCalculator;

        #endregion

        #region Properties

        public UIRenderer Renderer { get; set; }

        public IntPtr hWnd { get; protected set; }

        public bool DrawOnlyWhenInForeground { get; set; }

        public bool TrackTargetWindow { get; set; }

        public List<ControlBase> ChildControls { get; set; }

        public System.Drawing.Point CursorPosition { get { return this.PointToClient(System.Windows.Forms.Cursor.Position); } }

        #endregion

        #region Events

        public event EventHandler<DeltaEventArgs> TickEvent;

        public event EventHandler<OverlayEventArgs> BeforeDrawingEvent;
        public event EventHandler<OverlayEventArgs> AfterDrawingEvent;


        public virtual void OnTickEvent(DeltaEventArgs e)
        {
            if (TickEvent != null)
                TickEvent(this, e);
        }

        public virtual void OnBeforeDrawingEvent(OverlayEventArgs e)
        {
            if (BeforeDrawingEvent != null)
                BeforeDrawingEvent(this, e);
        }

        public virtual void OnAfterDrawingEvent(OverlayEventArgs e)
        {
            if (AfterDrawingEvent != null)
                AfterDrawingEvent(this, e);
        }

        #endregion

        #region Constructor

        public UIOverlay()
        {
            Renderer = new UIRenderer();
            _fpsCalculator = new FPSCalculator();

            // Setup form-properties
            this.BackColor = System.Drawing.Color.Black;
            this.TransparencyKey = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "";
            this.Name = "";
            this.TopMost = true;
            this.TopLevel = true;

            // Make form transparent and fully topmost
            int initialStyle = WinAPI.GetWindowLong(this.Handle, (int)WinAPI.GetWindowLongFlags.GWL_EXSTYLE);
            WinAPI.SetWindowLong(this.Handle, (int)WinAPI.GetWindowLongFlags.GWL_EXSTYLE, initialStyle | (int)WinAPI.ExtendedWindowStyles.WS_EX_LAYERED | (int)WinAPI.ExtendedWindowStyles.WS_EX_TRANSPARENT);
            WinAPI.SetWindowPos(this.Handle, (IntPtr)WinAPI.SetWindpwPosHWNDFlags.TopMost, 0, 0, 0, 0, (uint)WinAPI.SetWindowPosFlags.NOMOVE | (uint)WinAPI.SetWindowPosFlags.NOSIZE);
            WinAPI.SetLayeredWindowAttributes(this.Handle, 0, 255, (uint)WinAPI.LayeredWindowAttributesFlags.LWA_ALPHA);

            // Threading
            _logicThread = new Updater(256);
            _logicThread.TickEvent += _logicThread_TickEvent;
            _renderThread = new Updater(256);
            _renderThread.TickEvent += _renderThread_TickEvent;

            _lastLogicTick = DateTime.Now.Ticks;
            _lastRenderTick = DateTime.Now.Ticks;

            //Overlay-properties
            this.DrawOnlyWhenInForeground = true;
            this.TrackTargetWindow = true;
            this.ChildControls = new List<ControlBase>();
            this._handle = this.Handle;
        }

        private void _renderThread_TickEvent(object sender, DeltaEventArgs e)
        {
            TimeSpan deltaDraw = new TimeSpan(DateTime.Now.Ticks - _lastRenderTick);
            _lastRenderTick = DateTime.Now.Ticks;
            this.Invoke((MethodInvoker)(() => { this.OnDraw(deltaDraw.TotalSeconds); }));
        }

        private void _logicThread_TickEvent(object sender, DeltaEventArgs e)
        {
            TimeSpan deltaTimer = new TimeSpan(DateTime.Now.Ticks - _lastLogicTick);
            _lastLogicTick = DateTime.Now.Ticks;
            this.Invoke((MethodInvoker)(() => { this.OnTick(deltaTimer.TotalSeconds); }));
        }

        #endregion

        #region Methods

        public void Attach(IntPtr hWnd)
        {
            WinAPI.WINDOWINFO info = new WinAPI.WINDOWINFO();
            if (!WinAPI.GetWindowInfo(hWnd, ref info))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            this.hWnd = hWnd;
            this.Renderer.InitializeDevice(this.Handle, new Size2(info.rcClient.Right - info.rcClient.Left, info.rcClient.Bottom - info.rcClient.Top));

            _renderThread.StartUpdater();
            _logicThread.StartUpdater();
        }

        public void Detach()
        {
            //updDraw.StopUpdater();
            //updLogic.StopUpdater();
            this.Renderer.DestroyDevice();
        }

        public void ChangeHandle(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                this.Detach();
                base.Dispose(disposing);
            }
        }

        public void OnResize()
        {
            this.Renderer.Reset(new Size2(this.Width, this.Height));
        }

        protected virtual void OnDraw(double seconds)
        {
            if (this.DrawOnlyWhenInForeground)
            {
                if (WinAPI.GetForegroundWindow() != this.hWnd)
                    return;
            }

            WinAPI.MARGINS margins = new WinAPI.MARGINS();
            margins.topHeight = 0;
            margins.bottomHeight = 0;
            margins.leftWidth = this.Left;
            margins.rightWidth = this.Right;
            this.Invoke((MethodInvoker)(() => { WinAPI.DwmExtendFrameIntoClientArea(this.Handle, ref margins); }));

            this.Renderer.StartFrame();
            this.Renderer.Clear(this.Renderer.GetRendererBackColor());
            this.OnBeforeDrawingEvent(new OverlayEventArgs(this));

            foreach (ControlBase control in ChildControls)
                if (control.Visible)
                    control.Draw(this.Renderer);

            this.Renderer.DrawText("fps_font", $"{_fpsCalculator.Update().ToString("00")} FPS", this.Renderer.UISize.Width - 100, 10, Color.Red);

            this.OnAfterDrawingEvent(new OverlayEventArgs(this));
            this.Renderer.EndFrame();
        }

        protected virtual void OnTick(double seconds)
        {
            if (this.TrackTargetWindow)
            {
                WinAPI.WINDOWINFO info = new WinAPI.WINDOWINFO();
                if (WinAPI.GetWindowInfo(this.hWnd, ref info))
                {
                    int width = info.rcWindow.Right - info.rcWindow.Left;
                    int height = info.rcWindow.Bottom - info.rcWindow.Top;
                    if (this.Location.X != info.rcClient.Left ||
                        this.Location.Y != info.rcClient.Top)
                    {
                        this.Location = new System.Drawing.Point(info.rcClient.Left, info.rcClient.Top);
                    }
                    if (this.Width != info.rcClient.Right - info.rcClient.Left ||
                        this.Height != info.rcClient.Bottom - info.rcClient.Top)
                    {
                        this.Size = new System.Drawing.Size(info.rcClient.Right - info.rcClient.Left, info.rcClient.Bottom - info.rcClient.Top);
                        this.OnResize();
                    }
                    WinAPI.SetWindowPos(this.hWnd, this._handle, info.rcWindow.Left, info.rcWindow.Top, width, height, 0);
                }
            }

            OnTickEvent(new DeltaEventArgs(seconds, this));
        }

        public void UpdateControls(double secondsElapsed, InputUtils keys)
        {
            Vector2 cursor = new Vector2(this.CursorPosition.X, this.CursorPosition.Y);
            foreach (ControlBase control in this.ChildControls)
                control.Update(secondsElapsed, keys, cursor, true);
        }

        public void ShowInactiveTopmost()
        {
            WinAPI.ShowWindow(this.Handle, (int)WinAPI.WindowShowStyle.ShowNoActivate);
            WinAPI.SetWindowPos(this.Handle, (IntPtr)WinAPI.SetWindpwPosHWNDFlags.TopMost,
            this.Left, this.Top, this.Width, this.Height,
            (uint)WinAPI.SetWindowPosFlags.NOACTIVATE);
        }

        public void ResetTopmost()
        {
            WinAPI.BringWindowToTop(this.Handle);
        }

        #endregion
    }
}
