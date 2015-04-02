using System;

namespace UniversalSystem
{
    public class Vector3D
    {
        protected bool Equals(Vector3D other)
        {
            return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Vector3D) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = R.GetHashCode();
                hashCode = (hashCode*397) ^ G.GetHashCode();
                hashCode = (hashCode*397) ^ B.GetHashCode();
                return hashCode;
            }
        }

        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }

        public Vector3D() {}
        public Vector3D(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            Vector3D res = new Vector3D();
            res.R = v1.R + v2.R;
            res.G = v1.G + v2.G;
            res.B = v1.B + v2.B;
            return res;
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            Vector3D res = new Vector3D();
            res.R = v1.R - v2.R;
            res.G = v1.G - v2.G;
            res.B = v1.B - v2.B;
            return res;
        }

        public static Vector3D operator *(Vector3D v, double n)
        {
            Vector3D res = new Vector3D();
            res.R = v.R*n;
            res.G = v.G*n;
            res.B = v.B*n;
            return res;
        }

        public static Vector3D operator *(Vector3D v1, Vector3D v2)
        {
            Vector3D res = new Vector3D();
            res.R = v1.R * v2.R;
            res.G = v1.G * v2.G;
            res.B = v1.B * v2.B;
            return res;
        }

        public static Vector3D operator /(double n, Vector3D v)
        {
            Vector3D res = new Vector3D();
            res.R = v.R / n;
            res.G = v.G / n;
            res.B = v.B / n;
            return res;
        }

        public static Vector3D operator /(Vector3D v1, Vector3D v2)
        {
            Vector3D res = new Vector3D();
            res.R = v1.R / v2.R;
            res.G = v1.G / v2.G;
            res.B = v1.B / v2.B;
            return res;
        }

        public static bool operator <(Vector3D v, double n)
        {
            if (v.R < n && v.G < n && v.B < n)
                return true;
            return false;
        }

        public static bool operator >(Vector3D v, double n)
        {
            if (v.R > n && v.G > n && v.B > n)
                return true;
            return false;
        }

        public static bool operator ==(Vector3D v, double n)
        {
            if (Math.Abs(v.R-n)<0.00001 && Math.Abs(v.G-n)<0.00001 && Math.Abs(v.B-n)<0.00001)
                return true;
            return false;
        }

        public static bool operator !=(Vector3D v, double n)
        {
            if (Math.Abs(v.R - n) > 0.00001 && Math.Abs(v.G - n) > 0.00001 && Math.Abs(v.B - n) > 0.00001)
                return true;
            return false;
        }

        public static bool operator <=(Vector3D v, double n)
        {
            if (v < n || v == n)
                return true;
            return false;
        }

        public static bool operator >=(Vector3D v, double n)
        {
            if (v > n || v == n)
                return true;
            return false;
        }

        public static Vector3D Abs(Vector3D v1)
        {
            Vector3D res = new Vector3D();
            res.R = Math.Abs(v1.R);
            res.G = Math.Abs(v1.G);
            res.B = Math.Abs(v1.B);
            return res;
        }
    }
}
