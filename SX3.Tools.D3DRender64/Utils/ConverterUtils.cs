using SX3.Tools.D3DRender64.Controls.Misc;

namespace SX3.Tools.D3DRender64.Utils
{
    public static class ConverterUtils
    {
        public static Maths.Vector2 Vector2SDXtoEUC(this SharpDX.Vector2 vec)
        {
            return new Maths.Vector2(vec.X, vec.Y);
        }

        public static SharpDX.Vector2 Vector2EUCtoSDX(this Maths.Vector2 vec)
        {
            return new SharpDX.Vector2(vec.X, vec.Y);
        }

        public static Maths.Vector3 Vector3SDXtoEUC(this SharpDX.Vector3 vec)
        {
            return new Maths.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static SharpDX.Vector3 Vector3EUCtoSDX(this Maths.Vector3 vec)
        {
            return new SharpDX.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Maths.Vector2[] Vector2SDXtoEUC(this SharpDX.Vector2[] vec)
        {
            Maths.Vector2[] vecs = new Maths.Vector2[vec.Length];
            for (int i = 0; i < vecs.Length; i++)
                vecs[i] = Vector2SDXtoEUC(vec[i]);
            return vecs;
        }

        public static SharpDX.Vector2[] Vector2EUCtoSDX(this Maths.Vector2[] vec)
        {
            SharpDX.Vector2[] vecs = new SharpDX.Vector2[vec.Length];
            for (int i = 0; i < vecs.Length; i++)
                vecs[i] = Vector2EUCtoSDX(vec[i]);
            return vecs;
        }

        public static Maths.Vector3[] Vector3SDXtoEUC(this SharpDX.Vector3[] vec)
        {
            Maths.Vector3[] vecs = new Maths.Vector3[vec.Length];
            for (int i = 0; i < vecs.Length; i++)
                vecs[i] = Vector3SDXtoEUC(vec[i]);
            return vecs;
        }

        public static SharpDX.Vector3[] Vector3EUCtoSDX(this Maths.Vector3[] vec)
        {
            SharpDX.Vector3[] vecs = new SharpDX.Vector3[vec.Length];
            for (int i = 0; i < vecs.Length; i++)
                vecs[i] = Vector3EUCtoSDX(vec[i]);
            return vecs;
        }

        public static SharpDX.Color ColorEUCtoSDX(SX3Color color)
        {
            return new SharpDX.Color(color.R, color.G, color.B, color.A);
        }

        public static SX3Color ColorSDXtoDSX(SharpDX.Color color)
        {
            return new SX3Color(color.R, color.G, color.B, color.A);
        }
    }
}
