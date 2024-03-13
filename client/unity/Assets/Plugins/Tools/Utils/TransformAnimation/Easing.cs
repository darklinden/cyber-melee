using System;
using UnityEngine;

namespace Wtf
{
    public static class Easing
    {
        public enum Function
        {
            LINEAR,
            SMOOTHSTEP,
            IN_QUAD,
            OUT_QUAD,
            IN_CUBIC,
            OUT_CUBIC,
            IN_QUART,
            OUT_QUART,
            IN_QUINT,
            OUT_QUINT,
            SIN,
            INV_SIN,
            WEIGHTED_AVERAGE,
            OVERSHOOT,
            OUT_BACK,
            OUT_BOUNCE,
            IN_BACK,
            IN_OUT_BACK,
            OUT_ELASTIC,
            IN_EXPO,
            OUT_EXPO
        }

        public static float Apply(float v, Function func)
        {
            switch (func)
            {
                case Function.LINEAR:
                    return v;
                case Function.SMOOTHSTEP:
                    return Smoothstep(v);
                case Function.IN_QUAD:
                    return InQuad(v);
                case Function.OUT_QUAD:
                    return OutQuad(v);
                case Function.IN_CUBIC:
                    return InCubic(v);
                case Function.OUT_CUBIC:
                    return OutCubic(v);
                case Function.IN_QUART:
                    return InQuart(v);
                case Function.OUT_QUART:
                    return OutQuart(v);
                case Function.IN_QUINT:
                    return InQuint(v);
                case Function.OUT_QUINT:
                    return OutQuint(v);
                case Function.SIN:
                    return Sin(v);
                case Function.INV_SIN:
                    return InvSin(v);
                case Function.WEIGHTED_AVERAGE:
                    return WeightedAverage(v);
                case Function.OVERSHOOT:
                    return Overshoot(v);
                case Function.OUT_BACK:
                    return OutBack(v, 0f);
                case Function.OUT_BOUNCE:
                    return OutBounce(v);
                case Function.IN_BACK:
                    return InBack(v, 0f);
                case Function.IN_OUT_BACK:
                    return InOutBack(v, 0f);
                case Function.OUT_ELASTIC:
                    return OutElastic(v, 1f);
                case Function.IN_EXPO:
                    return InExpo(v);
                case Function.OUT_EXPO:
                    return OutExpo(v);
                default:
                    return 0f;
            }
        }

        public static float Smoothstep(float v, int iterations = 1)
        {
            iterations = Mathf.Clamp(iterations, 1, 5);
            for (int i = 0; i < iterations; i++)
            {
                v = v * v * (3f - 2f * v);
            }
            return v;
        }

        public static float InQuad(float v)
        {
            return v * v;
        }

        public static float OutQuad(float v)
        {
            float num = v;
            v = 1f - num;
            v *= 1f - num;
            v = 1f - v;
            return v;
        }

        public static float InCubic(float v)
        {
            float num = v;
            v *= num;
            v *= num;
            return v;
        }

        public static float OutCubic(float v)
        {
            float num = v;
            v = 1f - num;
            v *= 1f - num;
            v *= 1f - num;
            v = 1f - v;
            return v;
        }

        public static float InQuart(float v)
        {
            float num = v;
            v *= num;
            v *= num;
            v *= num;
            return v;
        }

        public static float OutQuart(float v)
        {
            float num = v;
            v = 1f - num;
            v *= 1f - num;
            v *= 1f - num;
            v *= 1f - num;
            v = 1f - v;
            return v;
        }

        public static float InQuint(float v)
        {
            float num = v;
            v *= num;
            v *= num;
            v *= num;
            v *= num;
            return v;
        }

        public static float OutQuint(float v)
        {
            float num = v;
            v = 1f - num;
            v *= 1f - num;
            v *= 1f - num;
            v *= 1f - num;
            v *= 1f - num;
            v = 1f - v;
            return v;
        }

        public static float Sin(float v)
        {
            return Mathf.Sin(v * (float)Math.PI / 2f);
        }

        public static float InvSin(float v)
        {
            return 1f - Mathf.Sin(v * (float)Math.PI / 2f);
        }

        public static float WeightedAverage(float v, int factor = 2)
        {
            factor = Mathf.Clamp(factor, 2, 5);
            return (v * (float)(factor - 1) + 1f) / (float)factor;
        }

        public static float Overshoot(float v, float factor = 1f)
        {
            factor = Mathf.Clamp(factor, 0.01f, float.MaxValue);
            float num = v * v;
            return (0f - factor) * num * v + (factor + 1f) * num;
        }

        public static float OutBack(float v, float factor = 0f)
        {
            float num = ((!(factor <= 1f)) ? factor : 1.70158f);
            v -= 1f;
            return v * v * ((num + 1f) * v + num) + 1f;
        }

        public static float OutBounce(float v)
        {
            if (v < 0.36363637f)
            {
                return 7.5625f * v * v;
            }
            if (v < 0.72727275f)
            {
                v -= 0.54545456f;
                return 7.5625f * v * v + 0.75f;
            }
            if (v < 0.90909094f)
            {
                v -= 0.8181818f;
                return 7.5625f * v * v + 0.9375f;
            }
            v -= 21f / 22f;
            return 7.5625f * v * v + 63f / 64f;
        }

        public static float InBack(float v, float factor = 0f)
        {
            float num = ((!(factor <= 1f)) ? factor : 1.70158f);
            return v * v * ((num + 1f) * v - num);
        }

        public static float InOutBack(float v, float factor = 0f)
        {
            float num = ((!(factor <= 1f)) ? factor : 1.70158f);
            num *= 1.525f;
            v *= 2f;
            if (v < 1f)
            {
                return 0.5f * (v * v * ((num + 1f) * v - num));
            }
            v -= 2f;
            return 0.5f * (v * v * ((num + 1f) * v + num) + 2f);
        }

        public static float OutElastic(float v, float amplitude = 1f, float period = 0.3f)
        {
            if (v == 0f)
            {
                return 0f;
            }
            if (v == 1f)
            {
                return 1f;
            }
            if (period == 0f)
            {
                period = 0.3f;
            }
            float num;
            if (amplitude == 0f || amplitude < 1f)
            {
                amplitude = 1f;
                num = period / 4f;
            }
            else
            {
                num = period / ((float)Math.PI * 2f) * Mathf.Asin(1f / amplitude);
            }
            return amplitude * Mathf.Pow(2f, -10f * v) * Mathf.Sin((v - num) * ((float)Math.PI * 2f) / period) + 1f;
        }

        public static float InExpo(float v)
        {
            return (v != 0f) ? Mathf.Pow(2f, 10f * (v - 1f)) : 0f;
        }

        public static float OutExpo(float v)
        {
            return (v != 1f) ? (0f - Mathf.Pow(2f, -10f * v) + 1f) : 1f;
        }
    }
}