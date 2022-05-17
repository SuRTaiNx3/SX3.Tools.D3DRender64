using SX3.Tools.D3DRender.Menu;
using System;
using SD = System.Drawing;
using WF = System.Windows.Forms;
using FontFactory = SharpDX.DirectWrite.Factory;
using D2DFactory = SharpDX.Direct2D1.Factory;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SX3.Tools.D3DRender.Menu.Items;
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

namespace SX3.Tools.D3DRender
{
    public class UIRenderer
    {
        #region Globals

        // General
        private IntPtr _hostWindowHandle = IntPtr.Zero;
        private static UIRenderer _currentInstance = null;

        // D3D
        private WindowRenderTarget _device = null;
        private D2DFactory Factory { get; set; }
        private FontFactory FontBase { get; set; }
        private HwndRenderTargetProperties _presentParameters;
        private SolidColorBrush _defaultBrush;

        // FPS
        private FPSCalculator _fpsCalculator;

        // Fonts
        private Dictionary<string, TextFormat> _fonts = new Dictionary<string, TextFormat>();

        // Keyboard hook
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc = hookProc;
        private static IntPtr _hhook = IntPtr.Zero;

        private const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x0101;

        #endregion

        #region Events

        public delegate void KeyWasPressedEventHandler(object sender, int keyCode, IntPtr wParam);
        public event KeyWasPressedEventHandler OnKeyPressed;

        #endregion


        #region DllImports

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        #endregion

        #region Properties

        private bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        private bool _isRendering = false;
        public bool IsRendering
        {
            get { return _isRendering; }
        }

        private int _deviceWidth;
        public int DeviceWidth
        {
            get { return _deviceWidth; }
        }

        private int _deviceHeight;
        public int DeviceHeight
        {
            get { return _deviceHeight; }
        }

        private RootMenu _menu;
        public RootMenu Menu 
        { 
            get { return _menu; } 
        }


        // Input related

        private static bool _controlIsPressed = false;
        public bool ControlIsPressed
        {
            get { return _controlIsPressed; }
        }

        private static bool _altIsPressed = false;
        public bool AltIsPressed
        {
            get { return _altIsPressed; }
        }

        private static bool _shiftIsPressed = false;
        public bool ShiftIsPressed
        {
            get { return _shiftIsPressed; }
        }

        #endregion

        #region Constructor

        public UIRenderer(IntPtr hostWindowHandle)
        {
            _hostWindowHandle = hostWindowHandle;
            _currentInstance = this;
            _menu = new RootMenu(this);
        }

        #endregion

        #region Methods

        #region General

        public void InitializeDevice(int width, int height)
        {
            Factory = new D2DFactory();
            FontBase = new FontFactory();

            _presentParameters = new HwndRenderTargetProperties()
            {
                Hwnd = _hostWindowHandle,
                PixelSize = new Size2(width, height),
                PresentOptions = PresentOptions.None,
            };

            _device = new WindowRenderTarget(Factory, new RenderTargetProperties(new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), _presentParameters);
            _device.TextAntialiasMode = TextAntialiasMode.Cleartype;
            _device.AntialiasMode = AntialiasMode.PerPrimitive;

            _deviceHeight = height;
            _deviceWidth = width;

            _isInitialized = true;
            CreateResources();
            SetHook();
        }

        private void CreateResources()
        {
            _fpsCalculator = new FPSCalculator();
            RegisterFont("default_font", "Museo", 12);
            RegisterFont("default_bold_font", "Museo", 12, FontWeight.Bold);
            RegisterFont("fps_font", "Arial", 12, FontWeight.Bold);
            RegisterFont("footer_font", "Museo", 10);
            RegisterFont("title_font", "Verdana", 10, FontWeight.Bold);
            
            _defaultBrush = new SolidColorBrush(_device, Color.Red);
        }

        public void StartFrame(int width, int height)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("InitializeDevice() must be called first!");

            if(IsRendering)
                throw new InvalidOperationException("Cannot call StartFrame when the previous frame was not finished. Call EndFrame() first!");

            _isRendering = true;

            _device.BeginDraw();
            _device.Clear(new RawColor4(0,0,0,0));

            _device.DrawText($"{_fpsCalculator.Update().ToString("00")} FPS", GetFontOrDefault("fps_font"), new RawRectangleF(width - 110, 10, 0, 0), _defaultBrush);

            Menu.Draw(this);
        }

        public void EndFrame()
        {
            if (IsRendering)
            {
                _device.EndDraw();
                _isRendering = false;
            }
        }

        public void Reset(int width, int height)
        {
            if (IsInitialized && !IsRendering)
            {
                _deviceHeight = height;
                _deviceWidth = width;
                _device.Resize(new Size2(width, height));
            }
        }

        public void Dispose()
        {
            foreach (var font in _fonts)
                font.Value.Dispose();

            _device.Dispose();

            UnHook();
        }

        #endregion

        #region Fonts

        public void RegisterFont(string fontKey, string fontFamily, int size, FontWeight weight = FontWeight.Regular, FontStyle style = FontStyle.Normal)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("InitializeDevice() must be called first!");

            _fonts.Add(fontKey, new TextFormat(FontBase, fontFamily, weight, style, size));
        }

        private TextFormat GetFontOrDefault(string fontKey)
        {
            return _fonts.ContainsKey(fontKey) ? _fonts[fontKey] : _fonts["default_font"];
        }

        public Size2F MeasureString(string fontKey, string text)
        {
            if (string.IsNullOrEmpty(text))
                return new Size2F();

            TextLayout layout = new TextLayout(FontBase, text, GetFontOrDefault(fontKey), DeviceWidth, DeviceHeight);
            layout.ParagraphAlignment = ParagraphAlignment.Center;

            Size2F size = new Size2F(layout.Metrics.Width, layout.Metrics.Height);
            layout.Dispose();
            return size;
        }

        #endregion

        #region Basic drawing

        // Text

        public void DrawText(string fontKey, string text, float x, float y, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _defaultBrush.Color = color;
            _device.DrawText(text, GetFontOrDefault(fontKey), new RectangleF(x, y, DeviceWidth, DeviceHeight), _defaultBrush);
        }

        public void DrawText(string fontKey, string text, RectangleF rect, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _defaultBrush.Color = color;
            _device.DrawText(text, GetFontOrDefault(fontKey), rect, _defaultBrush);
        }

        public void DrawShadowText(string fontKey, string text, float x, float y, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            DrawText(fontKey, text, x + 1, y + 1, Color.Black);
            DrawText(fontKey, text, x, y, color);
        }

        public void DrawShadowText(string fontKey, string text, RectangleF rect, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            RectangleF rect2 = new RectangleF(rect.X + 1, rect.Y + 2, rect.Width, rect.Height);

            DrawText(fontKey, text, rect, Color.Black);
            DrawText(fontKey, text, rect2, color);
        }

        public void DrawRotatedText(string fontKey, string text, float x, float y, float angle, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            RawMatrix3x2 oldTransform = _device.Transform;

            _device.Transform = Matrix3x2.Rotation(angle, new Vector2(x, y));
            DrawText(fontKey, text, x, y, color);

            _device.Transform = oldTransform;
        }

        public void DrawTextWithWrapping(string fontKey, string text, float x, float y, float width, float height, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            TextLayout layout = new TextLayout(FontBase, text, GetFontOrDefault(fontKey), width, height);
            layout.WordWrapping = WordWrapping.Wrap;
            layout.ParagraphAlignment = ParagraphAlignment.Near;

            _defaultBrush.Color = color;
            _device.DrawTextLayout(new RawVector2(x, y), layout, _defaultBrush);
            layout.Dispose();
        }

        public void DrawCenterText(string fontKey, string text, RectangleF rect, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            float centerX = rect.Width / 2;
            float centerY = rect.Height / 2;

            Size2F textSize = MeasureString(fontKey, text);

            float x = (centerX - (textSize.Width / 2)) + rect.X;
            float y = (centerY - (textSize.Height / 2)) + rect.Y - 1;

            DrawText(fontKey, text, x, y, color);
        }

        public void DrawRightText(string fontKey, string text, RectangleF rect, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            Size2F textSize = MeasureString(fontKey, text);

            float x = rect.X + rect.Width - textSize.Width;
            float y = rect.Y;

            DrawText(fontKey, text, x, y, color);
        }


        // Lines

        public void DrawLine(float x1, float y1, float x2, float y2, float strokeWidth, Color color)
        {
            _defaultBrush.Color = color;
            _device.DrawLine(new RawVector2(x1, y1), new RawVector2(x2, y2), _defaultBrush, strokeWidth);
        }


        // Circles

        public void DrawCircle(float centerX, float centerY, float radius, float strokeWidth, Color color)
        {
            _defaultBrush.Color = color;
            _device.DrawEllipse(new Ellipse(new RawVector2(centerX, centerY), radius, radius), _defaultBrush, strokeWidth);
        }

        private void DrawFilledCircle(float centerX, float centerY, float radius, Color color)
        {
            _defaultBrush.Color = color;
            _device.FillEllipse(new Ellipse(new RawVector2(centerX, centerY), radius, radius), _defaultBrush);
        }


        // Boxes

        public void DrawBox(RawRectangleF rect, float strokeWidth, Color color)
        {
            _defaultBrush.Color = color;
            _device.DrawRectangle(rect, _defaultBrush, strokeWidth);
        }

        public void DrawBox(float x, float y, float w, float h, float strokeWidth, Color color)
        {
            w = x + w;
            h = y + h;

            _defaultBrush.Color = color;
            _device.DrawRectangle(new RawRectangleF(x, y, w, h), _defaultBrush, strokeWidth);
        }

        public void DrawFilledBox(float x, float y, float w, float h, Color color)
        {
            w = x + w;
            h = y + h;

            _defaultBrush.Color = color;
            _device.FillRectangle(new RawRectangleF(x, y, w, h), _defaultBrush);
        }

        public void DrawFilledBox(RawRectangleF rect, Color color)
        {
            _defaultBrush.Color = color;
            _device.FillRectangle(rect, _defaultBrush);
        }

        public void DrawRoundedBox(float x, float y, float w, float h, float radius, Color color)
        {
            w = x + w;
            h = y + h;

            RoundedRectangle rectangle = new RoundedRectangle();
            rectangle.RadiusX = radius;
            rectangle.RadiusY = radius;
            rectangle.Rect = new RawRectangleF(x, y, w, h);

            _defaultBrush.Color = color;
            _device.DrawRoundedRectangle(rectangle, _defaultBrush);
        }

        public void DrawFilledRoundedBox(float x, float y, float w, float h, float radius, Color color)
        {
            w = x + w;
            h = y + h;

            RoundedRectangle rectangle = new RoundedRectangle();
            rectangle.RadiusX = radius;
            rectangle.RadiusY = radius;
            rectangle.Rect = new RawRectangleF(x, y, w, h);

            _defaultBrush.Color = color;
            _device.FillRoundedRectangle(rectangle, _defaultBrush);
        }

        public void DrawBorderEdges(float x, float y, float w, float h, float strokeWidth, Color color)
        {
            float topGapWidth = w / 2;
            float leftGapheight = h / 3;

            float horizontalLineLength = (topGapWidth / 2);
            float verticalLineLength = (leftGapheight / 2);


            float topLeftX = x;
            float topLeftY = y;

            float topRightX = x + w;
            float topRightY = y;

            float bottomRightX = x + w;
            float bottomRightY = y + h;

            float bottomLeftX = x;
            float bottomLeftY = y + h;


            // Top Left
            float line1X2 = x + horizontalLineLength;
            float line2Y2 = y + verticalLineLength;

            DrawLine(topLeftX, topLeftY, line1X2, topLeftY, strokeWidth, color);
            DrawLine(topLeftX, topLeftY, topLeftX, line2Y2, strokeWidth, color);


            // Top Right
            float line3X2 = topRightX - horizontalLineLength;
            float line4Y2 = y + verticalLineLength;

            DrawLine(topRightX, topLeftY, line3X2, topLeftY, strokeWidth, color);
            DrawLine(topRightX, topLeftY, topRightX, line4Y2, strokeWidth, color);


            // Bottom Left
            float line5Y2 = bottomRightY - verticalLineLength;
            float line6X2 = x + horizontalLineLength;

            DrawLine(topLeftX, bottomRightY, topLeftX, line5Y2, strokeWidth, color);
            DrawLine(topLeftX, bottomRightY, line6X2, bottomRightY, strokeWidth, color);


            // Bottom Right
            float line7X2 = topRightX - horizontalLineLength;
            float line8Y2 = bottomRightY - verticalLineLength;

            DrawLine(topRightX, bottomRightY, line7X2, bottomRightY, strokeWidth, color);
            DrawLine(topRightX, bottomRightY, topRightX, line8Y2, strokeWidth, color);
        }


        // Bitmaps

        public void DrawBitmap(Bitmap bitmap, float opacity)
        {
            _device.DrawBitmap(bitmap, opacity, BitmapInterpolationMode.Linear);
        }

        public void DrawBitmap(Bitmap bitmap, float opacity, RawRectangleF destinationRect)
        {
            _device.DrawBitmap(bitmap, destinationRect, opacity, BitmapInterpolationMode.Linear);
        }

        public void DrawBitmap(Bitmap bitmap, float opacity, RawRectangleF destinationRect, RawRectangleF sourceRectange)
        {
            _device.DrawBitmap(bitmap, destinationRect, opacity, BitmapInterpolationMode.Linear, sourceRectange);
        }

        public Bitmap LoadImageFromBitmap(System.Drawing.Bitmap bitmap)
        {
            return CreateBitmap(bitmap);
        }

        public Bitmap LoadImageFromFile(string file)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file))
            {
                return CreateBitmap(bitmap);
            }
        }

        private Bitmap CreateBitmap(System.Drawing.Bitmap bitmap)
        {
            var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapProperties = new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
            var size = new Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Not optimized 
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        tempStream.Write(rgba);
                    }

                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                return new Bitmap(_device, size, tempStream, stride, bitmapProperties);
            }
        }


        #endregion

        #region Advanced drawing

        public void DrawList(HashSet<string> items, string title, string footer, float x, float y, int width)
        {
            const int titleHeight = 23;

            const int lineHeight = 16;
            int playerTextHeight = items.Count * lineHeight;
            int totalHeight = playerTextHeight + titleHeight + 23;

            //Content box
            DrawFilledBox(x, y, width, totalHeight, new Color(216, 216, 216));
            DrawBox(x, y, width, totalHeight, 1, new Color(60, 60, 60));

            //Title
            DrawBox(x, y, width, titleHeight, 248, new Color(40, 40, 40, 255));
            DrawBox(x, y, width, titleHeight, 1, Color.Black);
            DrawShadowText("title_font", title, x + 5, y + 3, Color.White);

            string text = "";
            foreach (string item in items)
                text += item + "\n";

            DrawText("default_bold_font", text, x + 3, y + 27, new Color(10, 10, 10));

            DrawBox(x, y + playerTextHeight + 28, width, 20, 1, Color.Black);
            DrawFilledBox(x, y + playerTextHeight + 28, width, 20, new Color(60, 60, 60));

            RectangleF rect = new RectangleF(x, y + 1, width, 55);
            DrawCenterText("footer_font", footer, rect, Color.White);
        }

        public void DrawProgressbar(string fontKey, string title, float x, float y, float width, float height, double maxValue, double value, bool drawText)
        {
            float totalHeight = height;
            float totalWidth = width;
            int textFieldWidth = 50;
            float textX = x + 5;
            float textY = y + 2;

            // Left box with label
            if (!string.IsNullOrEmpty(title))
            {
                DrawFilledBox(x, y, textFieldWidth, totalHeight, new Color(40, 40, 40));
                DrawText("default_font", title, textX, textY, Color.White);
            }
            else
            {
                textFieldWidth = 0;
            }

            // MainBox
            float mainBoxX = x + textFieldWidth;
            float mainBoxY = y;
            float mainBoxWidth = totalWidth - textFieldWidth;
            double percentageValue = Math.Ceiling(((value * 100) / maxValue));

            DrawFilledBox(mainBoxX, mainBoxY, mainBoxWidth, totalHeight, new Color(104, 104, 104));

            // Calculate how much the progressbar is filled
            int coloredWidth = (int)((percentageValue / 100) * mainBoxWidth);

            Color fillColor = Color.DimGray;
            if (percentageValue > 80)
                fillColor = Color.DarkGreen;
            else if (percentageValue > 50)
                fillColor = Color.Gold;
            else if (percentageValue > 20)
                fillColor = Color.Orange;
            else if (percentageValue > 0)
                fillColor = Color.DarkRed;

            if (coloredWidth > 0)
                DrawFilledBox(mainBoxX, mainBoxY, coloredWidth, totalHeight, fillColor);

            // Draw Text
            if (drawText)
            {
                string valueText = value.ToString("0");
                RectangleF valueTextRect = new RectangleF(mainBoxX, mainBoxY + 2, mainBoxWidth, totalHeight);
                DrawCenterText("default_font", valueText, valueTextRect, Color.White);
            }
        }

        public void DrawCrosshair(float centerX, float centerY, float size, float thickness, Color color, CrosshairTypeItem.Type type)
        {
            switch (type)
            {
                case CrosshairTypeItem.Type.FullCross:
                    DrawLine(centerX - size, centerY, centerX + size, centerY, thickness, color);
                    DrawLine(centerX, centerY - size, centerX, centerY + size, thickness, color);
                    break;
                case CrosshairTypeItem.Type.Cross:
                    DrawLine(centerX - size, centerY, centerX - 3, centerY, thickness, color);
                    DrawLine(centerX + size, centerY, centerX + 3, centerY, thickness, color);
                    DrawLine(centerX, centerY - size, centerX, centerY - 3, thickness, color);
                    DrawLine(centerX, centerY + size, centerX, centerY + 3, thickness, color);
                    break;
                case CrosshairTypeItem.Type.Circle:
                    DrawCircle(centerX, centerY, size, 1, color);
                    break;
                case CrosshairTypeItem.Type.CircleAndCross:
                    DrawLine(centerX - size, centerY, centerX + size, centerY, thickness, color);
                    DrawLine(centerX, centerY - size, centerX, centerY + size, thickness, color);
                    DrawCircle(centerX, centerY, size - 5, thickness, color);
                    break;
                case CrosshairTypeItem.Type.FilledCircle:
                    DrawFilledCircle(centerX, centerY, size / 5, color);
                    break;
                case CrosshairTypeItem.Type.TiltedCross:
                    DrawLine(centerX + size, centerY + size, centerX + 3, centerY + 3, thickness, color);
                    DrawLine(centerX - size, centerY + size, centerX - 3, centerY + 3, thickness, color);
                    DrawLine(centerX + size, centerY - size, centerX + 3, centerY - 3, thickness, color);
                    DrawLine(centerX - size, centerY - size, centerX - 3, centerY - 3, thickness, color);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Input

        private void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            _hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0);
        }

        private void UnHook()
        {
            UnhookWindowsHookEx(_hhook);
        }

        public static IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            if (_currentInstance.OnKeyPressed != null)
                _currentInstance.OnKeyPressed(_currentInstance, vkCode, wParam);

            // Control Pressed
            if (vkCode == WF.Keys.LControlKey.GetHashCode() && wParam == (IntPtr)WM_KEYUP)
                _controlIsPressed = false;
            if (vkCode == WF.Keys.LControlKey.GetHashCode() && wParam == (IntPtr)WM_KEYDOWN)
                _controlIsPressed = true;

            // ALt pressed
            if (vkCode == WF.Keys.Alt.GetHashCode() && wParam == (IntPtr)WM_KEYUP)
                _altIsPressed = false;
            if (vkCode == WF.Keys.Alt.GetHashCode() && wParam == (IntPtr)WM_KEYDOWN)
                _altIsPressed = true;

            // Shift Pressed
            if (vkCode == WF.Keys.LShiftKey.GetHashCode() && wParam == (IntPtr)WM_KEYUP)
                _shiftIsPressed = false;
            if (vkCode == WF.Keys.LShiftKey.GetHashCode() && wParam == (IntPtr)WM_KEYDOWN)
                _shiftIsPressed = true;

            bool ValidKeyDown = false;
            if (vkCode == WF.Keys.Up.GetHashCode()
                || vkCode == WF.Keys.Down.GetHashCode() || vkCode == WF.Keys.Right.GetHashCode()
                || vkCode == WF.Keys.Left.GetHashCode() || vkCode == WF.Keys.Insert.GetHashCode()
                || vkCode == WF.Keys.NumPad2.GetHashCode() || vkCode == WF.Keys.NumPad8.GetHashCode()
                || vkCode == WF.Keys.NumPad5.GetHashCode() || vkCode == WF.Keys.NumPad9.GetHashCode()
                || vkCode == WF.Keys.NumPad3.GetHashCode())
                ValidKeyDown = true;

            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN && ValidKeyDown)
            {
                // Hide/Show Menu
                if (vkCode == WF.Keys.Insert.GetHashCode())
                    _currentInstance.Menu.IsVisible = !_currentInstance.Menu.IsVisible;

                if (!_currentInstance.Menu.IsVisible)
                    return CallNextHookEx(_hhook, code, (int)wParam, lParam);

                _currentInstance.Menu.ProcessKeyInput(vkCode);

                return (IntPtr)1;
            }
            else
                return CallNextHookEx(_hhook, code, (int)wParam, lParam);
        }

        #endregion

        #endregion
    }
}
