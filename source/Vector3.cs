using System;
using System.Runtime.InteropServices;

namespace raymarching
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero = new Vector3(0, 0, 0);

        public bool IsEqual(Vector3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.X + b.X,
                a.Y + b.Y,
                a.Z + b.Z
                );
        }

        public double Magnitude
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public void Normalize()
        {
            double scale = 1.0 / Magnitude;
            X *= scale;
            Y *= scale;
            Z *= scale;
        }

    }
}
