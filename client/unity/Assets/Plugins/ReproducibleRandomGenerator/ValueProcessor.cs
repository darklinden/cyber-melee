
// convert from https://github.com/XJINE/Unity_RandomEx/blob/master/Assets/Packages/Extensions/RandomEx/RandomEx.cs

using System.Collections.Generic;
using UnityEngine;

namespace ReproducibleRandomGenerator
{
    public class ValueProcessor
    {

        #region Field

        private const float RADIAN_MIN = 0;
        private const float RADIAN_MAX = 6.283185307179586f;

        // NOTE:
        // ValueFunc must return 0 ~ 1 range value.

        public System.Func<double> ValueFunc { get; set; }

        #endregion Field

        #region Property

        public double Value
        {
            get
            {
                if (ValueFunc == null)
                {
                    throw new System.Exception("ValueProcessor ValueFunc Is Null");
                }
                return ValueFunc.Invoke();
            }
        }

        public float Radian { get { return Range(RADIAN_MIN, RADIAN_MAX); } }

        public int Sign { get { return Range(0, 2) == 0 ? -1 : 1; } }

        #endregion Property

        #region Method

        // NOTE:
        // Range function returns min ~ max range value.
        // If argument type is int, the result excludes max value.
        // If argument type is float, the result includes max value.

        public int Index(IList<float> rates)
        {
            int index = 0;
            var seedValue = Value;

            for (index = 0; index < rates.Count; index++)
            {
                seedValue -= rates[index];

                if (seedValue <= 0)
                {
                    break;
                }
            }

            return index;
        }

        public int Range(int min, int max)
        {
            return (int)(min + (max - min + 1) * Value);
        }

        public long Range(long min, long max)
        {
            return (long)(min + (max - min + 1) * Value);
        }

        public float Range(float min, float max)
        {
            return min + (max - min) * (float)Value;
        }

        public float Range(Vector2 range)
        {
            return Range(range.x, range.y);
        }

        public Vector2 Range(Vector2 min, Vector2 max)
        {
            return new Vector2(Range(min.x, max.x), Range(min.y, max.y));
        }

        public Vector2Int Range(Vector2Int min, Vector2Int max)
        {
            return new Vector2Int(Range(min.x, max.x), Range(min.y, max.y));
        }

        public Vector3 Range(Vector3 min, Vector3 max)
        {
            return new Vector3(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        }

        public Vector3Int Range(Vector3Int min, Vector3Int max)
        {
            return new Vector3Int(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        }

        public Vector4 Range(Vector4 min, Vector4 max)
        {
            return new Vector4(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z), Range(min.w, max.w));
        }

        public Vector2 Range(Rect rect)
        {
            return Range(rect.min, rect.max);
        }

        public Vector3 Range(Bounds bounds)
        {
            return Range(bounds.min, bounds.max);
        }

        public void OnUnitCircle(out float x, out float y)
        {
            float angle = Radian;

            x = Mathf.Cos(angle);
            y = Mathf.Sin(angle);
        }

        public Vector2 OnUnitCircle()
        {
            OnUnitCircle(out float x, out float y);

            return new Vector2(x, y);
        }

        public void InsideUnitCircle(out float x, out float y)
        {
            float angle = Radian;
            float radius = (float)Value;

            x = Mathf.Cos(angle) * radius;
            y = Mathf.Sin(angle) * radius;
        }

        public Vector2 InsideUnitCircle()
        {
            InsideUnitCircle(out float x, out float y);

            return new Vector2(x, y);
        }

        public void OnUnitSphere(out float x, out float y, out float z)
        {
            float angle1 = Radian;
            float angle2 = Radian;

            x = Mathf.Sin(angle1) * Mathf.Cos(angle2);
            y = Mathf.Sin(angle1) * Mathf.Sin(angle2);
            z = Mathf.Cos(angle1);
        }

        public Vector3 OnUnitSphere()
        {
            OnUnitSphere(out float x, out float y, out float z);

            return new Vector3(x, y, z);
        }

        public void InsideUnitSphere(out float x, out float y, out float z)
        {
            OnUnitSphere(out x, out y, out z);

            float radius = (float)Value;

            x *= radius;
            y *= radius;
            z *= radius;
        }

        public Vector3 InsideUnitSphere()
        {
            InsideUnitSphere(out float x, out float y, out float z);

            return new Vector3(x, y, z);
        }

        #endregion Method
    }
}