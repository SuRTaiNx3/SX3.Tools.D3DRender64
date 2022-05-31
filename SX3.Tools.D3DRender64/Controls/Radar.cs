using SharpDX;
using SX3.Tools.D3DRender64.Utils;
using System;

namespace SX3.Tools.D3DRender64.Controls
{
    public class Radar : ContentControl
    {
        #region Properties

        public Vector2[] Enemies { get; set; }

        public Vector2[] Allies { get; set; }

        public Vector2 CenterCoordinate { get; set; }

        public bool Rotating { get; set; }

        public float Scaling { get; set; }

        public Color EnemiesColor { get; set; }

        public Color AlliesColor { get; set; }

        public float DotRadius { get; set; }

        public float RotationDegrees { get; set; }

        #endregion

        #region Constructor

        public Radar() : base()
        {
            this.Enemies = null;
            this.Allies = null;
            this.EnemiesColor = Color.Red;
            this.AlliesColor = Color.Blue;
            this.CenterCoordinate = Vector2.Zero;
            this.Rotating = true;
            this.RotationDegrees = 0f;
            this.DotRadius = 4f;
        }

        #endregion

        #region Methods

        public override void Draw(UIRenderer renderer)
        {
            Vector2 location = this.GetAbsoluteLocation();
            Vector2 size = this.GetSize();
            Vector2 controlCenter = location + size / 2f;
            float dotSize = DotRadius * 2;
            
            //Background
            renderer.DrawFilledBox(location.X, location.Y, size.X, size.Y, this.BackColor);
            renderer.DrawBox(location.X, location.Y, size.X, size.Y, 1, this.ForeColor);
            
            //Zoom
            renderer.DrawText(this.Font, string.Format("Zoom: {0}", Math.Round(Scaling, 4)), location.X, location.Y, this.ForeColor);
            
            //Grid
            renderer.DrawLine(location + Vector2.UnitX * size.X / 2f, location + Vector2.UnitX * size.X / 2f + Vector2.UnitY * size.Y, 1, this.ForeColor);
            renderer.DrawLine(location + Vector2.UnitY * size.Y / 2f, location + Vector2.UnitY * size.Y / 2f + Vector2.UnitX * size.X, 1, this.ForeColor);
            
            //Enemies
            if (Enemies != null)
                foreach (Vector2 coord in Enemies)
                    DrawDot(renderer, coord, EnemiesColor, controlCenter, dotSize);

            //Allies
            if (Allies != null)
                foreach (Vector2 coord in Allies)
                    DrawDot(renderer, coord, AlliesColor, controlCenter, dotSize);

            //Center
            renderer.DrawFilledCircle(controlCenter.X, controlCenter.Y, dotSize, this.ForeColor);

            base.Draw(renderer);
        }

        protected virtual void DrawDot(UIRenderer renderer, Vector2 coordinate, Color color, Vector2 controlCenter, float dotSize)
        {
            Vector2 delta = (coordinate - CenterCoordinate) * Scaling;
            delta.X *= -1;
            if (Rotating)
            {
                delta = ConverterUtils.Vector2EUCtoSDX(
                            MathUtils.RotatePoint(
                                ConverterUtils.Vector2SDXtoEUC(delta),
                                Maths.Vector2.Zero,
                                RotationDegrees));
            }
            if (Math.Abs(delta.X) + DotRadius > this.Width / 2f)
            {
                if (delta.X > 0)
                    delta.X = this.Width / 2f - DotRadius;
                else
                    delta.X = -this.Width / 2f + DotRadius;
            }

            if (Math.Abs(delta.Y) + DotRadius > this.Height / 2f)
            {
                if (delta.Y > 0)
                    delta.Y = this.Height / 2f - DotRadius;
                else
                    delta.Y = -this.Height / 2f + DotRadius;
            }

            Vector2 center = controlCenter + delta;
            renderer.DrawFilledCircle(center.X, center.Y, dotSize, color);
            renderer.DrawCircle(center.X, center.Y, dotSize, 1, this.ForeColor);
        }

        #endregion
    }
}
