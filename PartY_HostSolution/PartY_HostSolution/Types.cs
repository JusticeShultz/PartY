using System;
using System.Collections.Generic;
using System.Text;

namespace PartY_HostSolution
{
    public class Types
    {
        public class Vector2
        {
            public float x, y;

            public Vector2()
            {
                x = 0;
                y = 0;
            }

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

        public class Vector3
        {
            public float x, y, z;

            public Vector3()
            {
                x = 0;
                y = 0;
                z = 0;
            }

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
        }

        public class Quaternion
        {
            public float x, y, z, w;

            public Quaternion()
            {
                x = 0;
                y = 0;
                z = 0;
                w = 0;
            }

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