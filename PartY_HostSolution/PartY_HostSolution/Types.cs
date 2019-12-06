using System;
using System.Collections.Generic;
using System.Text;

namespace PartY_HostSolution
{
    public class Types
    {
        /// <summary>
        /// [PartY] Representation of 2D vectors and points.
        /// </summary>
        public struct Vector2
        {
            public float x, y;

            //public Vector2()
            //{
            //    x = 0;
            //    y = 0;
            //}

            public Vector2(int _x, int _y)
            {
                x = _x;
                y = _y;
            }

            public Vector2(float _x, float _y)
            {
                x = _x;
                y = _y;
            }

            public Vector2(Vector2 vector2)
            {
                x = vector2.x;
                y = vector2.y;
            }

            public Vector2(Vector3 vector3)
            {
                x = vector3.x;
                y = vector3.y;
            }
        }

        /// <summary>
        /// [PartY] Representation of 3D vectors and points.
        /// </summary>
        public struct Vector3
        {
            public float x, y, z;

            #region Swizzling/Premutations (Vector2)
            public Vector2 xy { get { return new Vector2(x, y); } }
            public Vector2 yy { get { return new Vector2(y, y); } }
            public Vector2 yx { get { return new Vector2(x, y); } }
            public Vector2 xx { get { return new Vector2(x, x); } }
            public Vector2 xz { get { return new Vector2(x, z); } }
            public Vector2 zz { get { return new Vector2(z, z); } }
            public Vector2 yz { get { return new Vector2(y, z); } }
            public Vector2 zy { get { return new Vector2(z, y); } }
            public Vector2 zx { get { return new Vector2(z, x); } }
            #endregion

            #region Constructors
            //public Vector3()
            //{
            //    x = 0;
            //    y = 0;
            //    z = 0;
            //}

            public Vector3(int _x, int _y, int _z)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public Vector3(float _x, float _y, float _z)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public Vector3(float xyz)
            {
                x = xyz;
                y = xyz;
                z = xyz;
            }

            public Vector3(float _xy, float _z)
            {
                x = _xy;
                y = _xy;
                z = _z;
            }

            public Vector3(Vector2 vector2)
            {
                x = vector2.x;
                y = vector2.y;
                z = 0;
            }

            public Vector3(Vector3 vector3)
            {
                x = vector3.x;
                y = vector3.y;
                z = vector3.z;
            }
            #endregion

            #region Helper methods
            public bool IsZero
            {
                get { return x == 0 && y == 0 && z == 0; }
            }

            public bool IsOne
            {
                get { return x == 1 && y == 1 && z == 1; }
            }

            public float Length()
            {
                return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
            }

            public float LengthSquared()
            {
                return (x * x) + (y * y) + (z * z);
            }

            public void Normalize()
            {
                float length = Length();
                if (length != 0)
                {
                    float inv = 1.0f / length;
                    x *= inv;
                    y *= inv;
                    z *= inv;
                }
            }

            public Vector3 normalized
            {
                get
                {
                    Vector3 temp = new Vector3(x, y, z);

                    float length = Length();

                    if (length != 0)
                    {
                        float inv = 1.0f / length;
                        temp.x *= inv;
                        temp.y *= inv;
                        temp.z *= inv;
                    }

                    return temp;
                }
            }

            public float[] ToArray()
            {
                return new float[] { x, y, z };
            }

            public static Vector3 Clamp(ref Vector3 value, ref Vector3 min, ref Vector3 max)
            {
                float x = value.x;
                x = (x > max.x) ? max.x : x;
                x = (x < min.x) ? min.x : x;

                float y = value.y;
                y = (y > max.y) ? max.y : y;
                y = (y < min.y) ? min.y : y;

                float z = value.z;
                z = (z > max.z) ? max.z : z;
                z = (z < min.z) ? min.z : z;

                return(new Vector3(x, y, z));
            }

            public static void Clamp(ref Vector3 value, ref Vector3 min, ref Vector3 max, out Vector3 result)
            {
                float x = value.x;
                x = (x > max.x) ? max.x : x;
                x = (x < min.x) ? min.x : x;

                float y = value.y;
                y = (y > max.y) ? max.y : y;
                y = (y < min.y) ? min.y : y;

                float z = value.z;
                z = (z > max.z) ? max.z : z;
                z = (z < min.z) ? min.z : z;

                result = new Vector3(x, y, z);
            }

            public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
            {
                Vector3 result;
                Clamp(ref value, ref min, ref max, out result);
                return result;
            }

            public static void Cross(ref Vector3 left, ref Vector3 right, out Vector3 result)
            {
                result = new Vector3(
                    (left.y * right.z) - (left.z * right.y),
                    (left.z * right.x) - (left.x * right.z),
                    (left.x * right.y) - (left.y * right.x));
            }

            public static Vector3 Cross(Vector3 left, Vector3 right)
            {
                Vector3 result;
                Cross(ref left, ref right, out result);
                return result;
            }

            public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
            {
                float x = value1.x - value2.x;
                float y = value1.y - value2.y;
                float z = value1.z - value2.z;

                result = (float)Math.Sqrt((x * x) + (y * y) + (z * z));
            }

            public static float Distance(Vector3 value1, Vector3 value2)
            {
                float x = value1.x - value2.x;
                float y = value1.y - value2.y;
                float z = value1.z - value2.z;

                return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
            }

            public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
            {
                float x = value1.x - value2.x;
                float y = value1.y - value2.y;
                float z = value1.z - value2.z;

                result = (x * x) + (y * y) + (z * z);
            }

            public static float DistanceSquared(Vector3 value1, Vector3 value2)
            {
                float x = value1.x - value2.x;
                float y = value1.y - value2.y;
                float z = value1.z - value2.z;

                return (x * x) + (y * y) + (z * z);
            }

            public static void Dot(ref Vector3 left, ref Vector3 right, out float result)
            {
                result = (left.x * right.x) + (left.y * right.y) + (left.z * right.z);
            }

            public static float Dot(Vector3 left, Vector3 right)
            {
                return (left.x * right.x) + (left.y * right.y) + (left.z * right.z);
            }

            public static void Normalize(ref Vector3 value, out Vector3 result)
            {
                result = value;
                result.Normalize();
            }

            public static Vector3 Normalize(Vector3 value)
            {
                value.Normalize();
                return value;
            }

            public static void Lerp(ref Vector3 start, ref Vector3 end, float amount, out Vector3 result)
            {
                result = zero;

                result.x = (1 - amount) * start.x + amount * end.x;
                result.y = (1 - amount) * start.y + amount * end.y;
                result.z = (1 - amount) * start.z + amount * end.z;
            }

            public static Vector3 Lerp(Vector3 start, Vector3 end, float amount)
            {
                Vector3 result;
                Lerp(ref start, ref end, amount, out result);
                return result;
            }

            public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
            {
                result = zero;

                float squared = amount * amount;
                float cubed = amount * squared;
                float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
                float part2 = (-2.0f * cubed) + (3.0f * squared);
                float part3 = (cubed - (2.0f * squared)) + amount;
                float part4 = cubed - squared;

                result.x = (((value1.x * part1) + (value2.x * part2)) + (tangent1.x * part3)) + (tangent2.x * part4);
                result.y = (((value1.y * part1) + (value2.y * part2)) + (tangent1.y * part3)) + (tangent2.y * part4);
                result.z = (((value1.z * part1) + (value2.z * part2)) + (tangent1.z * part3)) + (tangent2.z * part4);
            }

            public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
            {
                Vector3 result;
                Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
                return result;
            }

            public static void Max(ref Vector3 left, ref Vector3 right, out Vector3 result)
            {
                result = zero;

                result.x = (left.x > right.x) ? left.x : right.x;
                result.y = (left.y > right.y) ? left.y : right.y;
                result.z = (left.z > right.z) ? left.z : right.z;
            }

            public static Vector3 Max(Vector3 left, Vector3 right)
            {
                Vector3 result;
                Max(ref left, ref right, out result);
                return result;
            }

            public static void Min(ref Vector3 left, ref Vector3 right, out Vector3 result)
            {
                result = zero;

                result.x = (left.x < right.x) ? left.x : right.x;
                result.y = (left.y < right.y) ? left.y : right.y;
                result.z = (left.z < right.z) ? left.z : right.z;
            }

            public static Vector3 Min(Vector3 left, Vector3 right)
            {
                Vector3 result;
                Min(ref left, ref right, out result);
                return result;
            }
            #endregion

            #region Helper values
            public static readonly Vector3 zero  = new Vector3(0.0f, 0.0f, 0.0f);
            public static readonly Vector3 one   = new Vector3(1.0f, 1.0f, 1.0f);
            public static readonly Vector3 up    = new Vector3(0.0f, 1.0f, 0.0f);
            public static readonly Vector3 down  = new Vector3(0.0f, -1.0f, 0.0f);
            public static readonly Vector3 left  = new Vector3(-1.0f, 0.0f, 0.0f);
            public static readonly Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
            #endregion
        }

        /// <summary>
        /// [PartY] Representation of 4D vectors and points. Use Vector3 instead and convert using euler angles.
        /// </summary>
        public struct Quaternion
        {
            public float x, y, z, w;

            //public Quaternion()
            //{
            //    x = 0;
            //    y = 0;
            //    z = 0;
            //    w = 0;
            //}

            public Quaternion(int _x, int _y, int _z, int _w)
            {
                x = _x;
                y = _y;
                z = _z;
                w = _w;
            }

            public Quaternion(float _x, float _y, float _z, float _w)
            {
                x = _x;
                y = _y;
                z = _z;
                w = _w;
            }

            public Quaternion(Quaternion quaternion)
            {
                x = quaternion.x;
                y = quaternion.y;
                z = quaternion.z;
                w = quaternion.w;
            }

            public static Quaternion identity()
            {
                return new Quaternion(0, 0, 0, 1);
            }
        }
    }
}