using SX3.Tools.D3DRender64.Controls;
using SX3.Tools.D3DRender64.Events;
using SX3.Tools.D3DRender64.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SX3.Tools.D3DRender64.Example
{
    class Program
    {
        #region Globals

        private static Cursor cursor;
        
        //Menu-window
        private static XWindow windowMenu;
        private static Label label;
        private static Button buttonToggle;
        private static Panel panelContent;
        private static CheckBox checkBox;
        private static Trackbar track;

        private static InputUtils input;

        #endregion

        #region Properties

        public static UIOverlay Overlay;

        #endregion

        static void Main()
        {
            Console.WriteLine("Change overlay attached window by pressing F5");
            Console.WriteLine("It will attach to currently active window");
            Console.WriteLine("Trackbar can be controled with mouse wheel");
            
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            input = new InputUtils();
            var hWnd = WinAPI.GetForegroundWindow();

            using (Overlay = new UIOverlay())
            {
                Overlay.Attach(hWnd);
                Overlay.TickEvent += overlay_TickEvent;
                InitializeComponents();
                UIRenderer renderer = Overlay.Renderer;

                renderer.RegisterFont("small_font", "Century Gothic", 10);
                renderer.RegisterFont("large_font", "Century Gothic", 14);
                renderer.RegisterFont("heavy_font", "Century Gothic", 14, 900);

                windowMenu.SetFontByKey("small_font"); ;
                windowMenu.Caption.SetFontByKey("large_font");
                Overlay.ChildControls.Add(windowMenu);

                System.Windows.Forms.Application.Run(Overlay);
            }
        }

        private static void InitializeComponents()
        {
            windowMenu = new XWindow();
            windowMenu.Caption.Text = "Overlay sample";
            windowMenu.X = 0;
            windowMenu.Panel.DynamicWidth = true;
            windowMenu.Panel.Width = 200;
            windowMenu.Panel.Height = 200;
            InitPanel(ref panelContent);
            InitLabel(ref label, "EXAMPLE", true, 200);
            InitCheckBox(ref checkBox, "Checkbox", "lel", true);
            InitTrackBar(ref track, "Trackbar", "");
            InitCursor(ref cursor);
            panelContent.AddChildControl(label);
            panelContent.InsertSpacer();
            panelContent.AddChildControl(checkBox);
            panelContent.InsertSpacer();
            panelContent.AddChildControl(track);
            windowMenu.Panel.AddChildControl(panelContent);
            windowMenu.AddChildControl(cursor);
        }

        private static void overlay_TickEvent(object sender, DeltaEventArgs e)
        {
            var overlay = (UIOverlay)sender;
            input.Update();
            if (input.Keys.KeyWentUp(WinAPI.VirtualKeyShort.F5))
            {
                overlay.ChangeHandle(WinAPI.GetForegroundWindow());
            }
            overlay.UpdateControls(e.SecondsElapsed, input);

            if (input.Keys.KeyIsDown(WinAPI.VirtualKeyShort.END))
                e.Overlay.Close();
            // e.Overlay.ShowInactiveTopmost();

        }

        private static void InitCursor(ref Cursor control)
        {
            control = new Cursor();
            control.Angle = 45;
        }

        private static void InitRadioButton(ref RadioButton control, string text, object tag, bool bChecked)
        {
            control = new RadioButton();
            control.Text = text;
            control.Tag = tag;
            control.Checked = bChecked;
            control.CheckedChangedEvent += radioButton_CheckedChanged;
        }

        private static void InitLabel(ref Label control, string text, bool fixedWidth = false, float width = 0f, Label.TextAlignment alignment = Label.TextAlignment.Left)
        {
            control = new Label();
            control.FixedWidth = fixedWidth;
            control.Width = width;
            control.TextAlign = alignment;
            control.Text = text;
            control.Tag = null;
        }

        private static void InitCheckBox(ref CheckBox control, string text, object tag, bool bChecked)
        {
            control = new CheckBox();
            control.Text = text;
            control.Tag = tag;
            control.Checked = bChecked;
            control.CheckedChangedEvent += checkBox_CheckedChanged;
            control.MouseClickEventUp += (sender, e) =>
            {
                if (e.Wheel)
                    ((CheckBox)sender).Checked = !((CheckBox)sender).Checked;
            };
        }

        private static void InitPanel(ref Panel control, bool dynamicWidth = true, bool dynamicHeight = true, bool fillParent = true, bool visible = true)
        {
            control = new Panel();
            control.DynamicHeight = dynamicHeight;
            control.DynamicWidth = dynamicWidth;
            control.FillParent = fillParent;
            control.Visible = visible;
        }

        private static void InitTrackBar(ref Trackbar control, string text, object tag, float min = 0, float max = 100, float value = 50, int numberofdecimals = 2)
        {
            control = new Trackbar();
            control.Text = text;
            control.Tag = tag;
            control.Minimum = min;
            control.Maximum = max;
            control.Value = value;
            control.NumberOfDecimals = numberofdecimals;
        }

        static void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox control = (CheckBox)sender;
        }

        private static void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton control = (RadioButton)sender;
        }
    }
}
