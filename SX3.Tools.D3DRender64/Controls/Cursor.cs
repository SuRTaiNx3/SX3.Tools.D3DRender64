using SharpDX;
using SX3.Tools.D3DRender64.Utils;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Cursor : ContentControl
    {
        #region Globals

        private Vector2 LastCursorPoint { get; set; }

        #endregion

        #region Properties

        public float Angle { get; set; }
        public float AngleRotation { get; set; }

        #endregion

        #region Constructor

        public Cursor() : base()
        {
            this.Angle = 36.9f;
            this.AngleRotation = 25.6f;
            this.Width = 28f;
            this.LastCursorPoint = Vector2.Zero;
        }

        #endregion

        #region Methods

        public override bool CheckMouseOver(SharpDX.Vector2 cursorPoint)
        {
            return false;
        }

        public override void Update(double secondsElapsed, InputUtils keyUtils, Vector2 cursorPoint, bool checkMouse = false)
        {
            this.LastCursorPoint = cursorPoint;
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
        }

        public override void Draw(UIRenderer renderer)
        {
            Vector2 center = this.LastCursorPoint;
            Vector2 right = this.LastCursorPoint + Vector2.UnitX * this.Width;
            Vector2 left = ConverterUtils.Vector2EUCtoSDX(MathUtils.RotatePoint(
                            ConverterUtils.Vector2SDXtoEUC(right),
                            ConverterUtils.Vector2SDXtoEUC(center),
                            Angle));

            right = ConverterUtils.Vector2EUCtoSDX(MathUtils.RotatePoint(
                            ConverterUtils.Vector2SDXtoEUC(right),
                            ConverterUtils.Vector2SDXtoEUC(center),
                            AngleRotation));
            left = ConverterUtils.Vector2EUCtoSDX(MathUtils.RotatePoint(
                            ConverterUtils.Vector2SDXtoEUC(left),
                            ConverterUtils.Vector2SDXtoEUC(center),
                            AngleRotation));

            renderer.FillPolygon(this.BackColor, left, center, right);
            renderer.DrawPolygon(this.ForeColor, left, center, right);
            base.Draw(renderer);
        }

        #endregion
    }
}
