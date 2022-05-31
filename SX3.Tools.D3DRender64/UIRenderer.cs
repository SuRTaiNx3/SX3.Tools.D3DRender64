using System;
using FontFactory = SharpDX.DirectWrite.Factory;
using D2DFactory = SharpDX.Direct2D1.Factory;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

namespace SX3.Tools.D3DRender64
{
    public class UIRenderer
    {
        #region Globals

        // General
        private bool _disposing;

        // D3D
        private D2DFactory _d2dFactory { get; set; }
        private FontFactory _fontFactory { get; set; }
        private HwndRenderTargetProperties _renderTargetProperties;
        private SolidColorBrush _defaultBrush;

        // Fonts
        private Dictionary<string, TextFormat> _fonts = new Dictionary<string, TextFormat>();

        #endregion

        #region Properties

        public WindowRenderTarget Device { get; set; }

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

        private Size2 _uiSize;
        public Size2 UISize
        {
            get { return _uiSize; }
            set { _uiSize = value; }
        }

        public Color GetRendererBackColor()
        {
            return Color.Transparent;
        }

        #endregion

        #region Constructor

        public UIRenderer()
        {
            _disposing = false;
        }

        #endregion

        #region Methods

        #region General

        public void InitializeDevice(IntPtr hWnd, Size2 size)
        {
            _d2dFactory = new D2DFactory();
            _fontFactory = new FontFactory();
            _fonts = new Dictionary<string, TextFormat>();

            _renderTargetProperties = new HwndRenderTargetProperties()
            {
                Hwnd = hWnd,
                PixelSize = size,
                PresentOptions = PresentOptions.None
            };

            Device = new WindowRenderTarget(_d2dFactory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)), _renderTargetProperties);
            Device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;

            UISize = size;

            _isInitialized = true;
            CreateResources();
        }

        public void DestroyDevice()
        {
            foreach (TextFormat font in _fonts.Values)
                font.Dispose();

            _fonts.Clear();

            _d2dFactory.Dispose();
            _fontFactory.Dispose();
            Device.Dispose();
            this.Device = null;
        }

        private void CreateResources()
        {
            RegisterFont("default_font", "Museo", 12);
            RegisterFont("default_bold_font", "Museo", 12, (int)FontWeight.Bold);
            RegisterFont("fps_font", "Arial", 18, (int)FontWeight.Bold);
            RegisterFont("footer_font", "Museo", 10);
            RegisterFont("title_font", "Verdana", 10, (int)FontWeight.Bold);
            
            _defaultBrush = new SolidColorBrush(Device, Color.Red);
        }

        public void StartFrame()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("InitializeDevice() must be called first!");

            if(IsRendering)
                throw new InvalidOperationException("Cannot call StartFrame when the previous frame was not finished. Call EndFrame() first!");

            _isRendering = true;

            Device.BeginDraw();
            Clear(new Color(0,0,0,0));
        }

        public void EndFrame()
        {
            if (IsRendering)
            {
                try
                {
                    Device.EndDraw();
                    _isRendering = false;
                }
                catch
                {
                    if (!_disposing)
                    {
                        Device.Dispose();
                        Device = new WindowRenderTarget(_d2dFactory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)), _renderTargetProperties);
                        Device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
                    }
                }
            }
        }

        public void Clear(Color color)
        {
            if (Device == null)
                throw new SharpDXException("The device was not initialized yet");

            Device.Clear(color);
        }

        public void Reset(Size2 size)
        {
            if (IsInitialized && !IsRendering)
            {
                Device.Resize(size);
                UISize = size;
            }
        }

        public void Dispose()
        {
            if (this.Device != null && !_disposing)
            {
                _disposing = true;
                this.DestroyDevice();
            }
        }

        #endregion

        #region Fonts

        public void RegisterFont(string fontKey, string fontFamily, int size, int weight = 400, int style = 0)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("InitializeDevice() must be called first!");

            FontWeight fontWeight = FontWeight.Regular;
            if (Enum.IsDefined(typeof(FontWeight), weight))
                fontWeight = (FontWeight)weight;

            FontStyle fontStyle = FontStyle.Normal;
            if (Enum.IsDefined(typeof(FontStyle), style))
                fontStyle = (FontStyle)style;

            TextFormat format = new TextFormat(_fontFactory, fontFamily, fontWeight, fontStyle, size);
            _fonts.Add(fontKey, format);
        }

        public TextFormat GetFontOrDefault(string fontKey)
        {
            return _fonts.ContainsKey(fontKey) ? _fonts[fontKey] : _fonts["default_font"];
        }

        public Size2F MeasureString(string fontKey, string text)
        {
            return MeasureString(GetFontOrDefault(fontKey), text);
        }

        public Size2F MeasureString(TextFormat format, string text)
        {
            if (string.IsNullOrEmpty(text))
                return new Size2F();

            TextLayout layout = new TextLayout(_fontFactory, text, format, UISize.Width, UISize.Height);
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
            DrawText(GetFontOrDefault(fontKey), text, new RectangleF(x, y, UISize.Width, UISize.Height), color);
        }

        public void DrawText(string fontKey, string text, RectangleF rect, Color color)
        {
            DrawText(GetFontOrDefault(fontKey), text, rect, color);
        }

        public void DrawText(TextFormat format, string text, float x, float y, Color color)
        {
            DrawText(format, text, new RectangleF(x, y, UISize.Width, UISize.Height), color);
        }

        public void DrawText(TextFormat format, string text, RectangleF rect, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _defaultBrush.Color = color;
            Device.DrawText(text, format, rect, _defaultBrush);
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

            RawMatrix3x2 oldTransform = Device.Transform;

            Device.Transform = Matrix3x2.Rotation(angle, new Vector2(x, y));
            DrawText(fontKey, text, x, y, color);

            Device.Transform = oldTransform;
        }

        public void DrawTextWithWrapping(string fontKey, string text, float x, float y, float width, float height, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            TextLayout layout = new TextLayout(_fontFactory, text, GetFontOrDefault(fontKey), width, height);
            layout.WordWrapping = WordWrapping.Wrap;
            layout.ParagraphAlignment = ParagraphAlignment.Near;

            _defaultBrush.Color = color;
            Device.DrawTextLayout(new RawVector2(x, y), layout, _defaultBrush);
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
            DrawLine(new RawVector2(x1, y1), new RawVector2(x2, y2), strokeWidth, color);
        }

        public void DrawLine(RawVector2 from, RawVector2 to, float strokeWidth, Color color)
        {
            _defaultBrush.Color = color;
            Device.DrawLine(from, to, _defaultBrush, strokeWidth);
        }

        public void DrawLines(Color color, float strokeWidth, params Vector2[] points)
        {
            if (points.Length < 2)
                throw new ArgumentException("There must be at least two points to connect", "points");
            for (int i = 0; i < points.Length - 1; i++)
                DrawLine(points[i], points[i + 1], strokeWidth, color);
        }

        public void DrawLines(Color color, params Vector2[] points)
        {
            this.DrawLines(color, 1f, points);
        }


        // Circles

        public void DrawCircle(float centerX, float centerY, float radius, float strokeWidth, Color color)
        {
            _defaultBrush.Color = color;
            Device.DrawEllipse(new Ellipse(new RawVector2(centerX, centerY), radius, radius), _defaultBrush, strokeWidth);
        }

        public void DrawFilledCircle(float centerX, float centerY, float radius, Color color)
        {
            _defaultBrush.Color = color;
            Device.FillEllipse(new Ellipse(new RawVector2(centerX, centerY), radius, radius), _defaultBrush);
        }


        // Polygon

        public void FillPolygon(Color color, params RawVector2[] points)
        {
            if (Device == null)
                throw new SharpDXException("The device was not initialized yet");

            using (PathGeometry gmtry = CreatePathGeometry(points))
            {
                using (SolidColorBrush brush = new SolidColorBrush(Device, color))
                {
                    Device.FillGeometry(gmtry, brush);
                }
            }
        }

        public void DrawPolygon(Color color, float strokeWidth, params Vector2[] points)
        {
            if (points.Length < 3)
                throw new ArgumentException("A polygon must at least have three edges", "points");
            
            for (int i = 0; i < points.Length - 1; i++)
                DrawLine(points[i], points[i + 1], strokeWidth, color);

            DrawLine(points[points.Length - 1], points[0], strokeWidth, color);
        }

        public void DrawPolygon(Color color, params Vector2[] points)
        {
            this.DrawPolygon(color, 1f, points);
        }


        // Boxes

        public void DrawBox(RawRectangleF rect, float strokeWidth, Color color)
        {
            _defaultBrush.Color = color;
            Device.DrawRectangle(rect, _defaultBrush, strokeWidth);
        }

        public void DrawBox(float x, float y, float w, float h, float strokeWidth, Color color)
        {
            w = x + w;
            h = y + h;

            _defaultBrush.Color = color;
            Device.DrawRectangle(new RawRectangleF(x, y, w, h), _defaultBrush, strokeWidth);
        }

        public void DrawFilledBox(float x, float y, float w, float h, Color color)
        {
            w = x + w;
            h = y + h;

            _defaultBrush.Color = color;
            Device.FillRectangle(new RawRectangleF(x, y, w, h), _defaultBrush);
        }

        public void DrawFilledBox(RawRectangleF rect, Color color)
        {
            _defaultBrush.Color = color;
            Device.FillRectangle(rect, _defaultBrush);
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
            Device.DrawRoundedRectangle(rectangle, _defaultBrush);
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
            Device.FillRoundedRectangle(rectangle, _defaultBrush);
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
            Device.DrawBitmap(bitmap, opacity, BitmapInterpolationMode.Linear);
        }

        public void DrawBitmap(Bitmap bitmap, float opacity, RawRectangleF destinationRect)
        {
            Device.DrawBitmap(bitmap, destinationRect, opacity, BitmapInterpolationMode.Linear);
        }

        public void DrawBitmap(Bitmap bitmap, float opacity, RawRectangleF destinationRect, RawRectangleF sourceRectange)
        {
            Device.DrawBitmap(bitmap, destinationRect, opacity, BitmapInterpolationMode.Linear, sourceRectange);
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
            var bitmapProperties = new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));
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

                return new Bitmap(Device, size, tempStream, stride, bitmapProperties);
            }
        }


        // Misc

        private PathGeometry CreatePathGeometry(params RawVector2[] points)
        {
            PathGeometry gmtry = new PathGeometry(_d2dFactory);

            GeometrySink sink = gmtry.Open();
            sink.SetFillMode(FillMode.Winding);
            sink.BeginFigure(points[0], FigureBegin.Filled);
            sink.AddLines(points);
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();

            return gmtry;
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

        #endregion

        #endregion
    }
}
