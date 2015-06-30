using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace RayCasterGame
{
    /// <summary>
    /// HSV Color.
    /// </summary>
    /// <remarks>
    /// HSV conversions taken from http://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both
    /// </remarks>
    [DebuggerDisplay("H = {H}, S = {S}, V = {V}")]
    public struct HsvColor
    {
        /// <summary>
        /// Hue (angle in degrees)
        /// </summary>
        public readonly float H;
        /// <summary>
        /// Saturation (percentage)
        /// </summary>
        public readonly float S;
        /// <summary>
        /// Value (percentage)
        /// </summary>
        public readonly float V;

        public static readonly HsvColor Black = new HsvColor();

        public static readonly HsvColor Red = HsvColor.FromRgb(Color.Red);
        public static readonly HsvColor Blue = HsvColor.FromRgb(Color.Blue);
        public static readonly HsvColor Green = HsvColor.FromRgb(Color.Green);
        public static readonly HsvColor White = HsvColor.FromRgb(Color.White);
        public static readonly HsvColor Yellow = HsvColor.FromRgb(Color.Yellow);

        public HsvColor(float hue, float saturation, float value)
        {
            H = hue;
            S = saturation;
            V = value;
        }

        public HsvColor ScaleValue(float percentage)
        {
            return new HsvColor(H, S, percentage * V);
        }

        public HsvColor Mutate(
                            Func<float, float> hx = null,
                            Func<float, float> sx = null,
                            Func<float, float> vx = null)
        {
            Func<float, float> passthrough = _ => _;
            hx = hx ?? passthrough;
            vx = vx ?? passthrough;
            sx = sx ?? passthrough;
            return new HsvColor(hx(H), sx(S), vx(V));
        }

        public static HsvColor FromPackedRgb(uint packedColor)
        {
            byte rByte = (byte)(packedColor & 0xFF);
            byte gByte = (byte)(packedColor >> 8 & 0xFF);
            byte bByte = (byte)(packedColor >> 16 & 0xFF);

            float r = rByte / 255f;
            float g = gByte / 255f;
            float b = bByte / 255f;

            return FromRgb(r, g, b);
        }

        public static HsvColor FromRgb(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            return FromRgb(r, g, b);
        }

        public static HsvColor FromRgb(float r, float g, float b)
        {
            float h = 0;
            float s = 0;
            float v = 0;



            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));

            v = max;
            var delta = max - min;

            if (max > 0f)
            { // NOTE: if Max is == 0, this divide would cause a crash
                s = (delta / max);
            }
            else
            {
                // if max is 0, then r = g = b = 0              
                // s = 0, v is undefined
                return HsvColor.Black;
            }

            if (r >= max)
            {
                // between yellow & magenta
                h = (g - b) / delta;
            }
            else
            {
                if (g >= max)
                {
                    // between cyan & yellow
                    h = 2f + (b - r) / delta;
                }
                else
                {
                    // between magenta & cyan
                    h = 4f + (r - g) / delta;
                }
            }

            // degrees
            h *= 60f;

            if (h < 0f)
            {
                h += 360f;
            }

            return new HsvColor(h, s, v);
        }


        public uint ToPackedRgbColor()
        {
            float r = 0;
            float g = 0;
            float b = 0;

            if (S <= 0)
            {
                return ToPackedColor(V, V, V);
            }

            var hh = H;
            while (hh < 0) hh += 360f;
            while (hh >= 360f) hh -= 360f;
            hh /= 60f;

            var i = (long)hh;
            var ff = hh - i;
            var p = V * (1f - S);
            var q = V * (1f - (S * ff));
            var t = V * (1f - (S * (1f - ff)));

            switch (i)
            {
                case 0:
                    r = V;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = V;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = V;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = V;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = V;
                    break;
                case 5:
                default:
                    r = V;
                    g = p;
                    b = q;
                    break;
            }
            return ToPackedColor(r, g, b);
        }

        private static uint ToPackedColor(float r, float g, float b)
        {
            var rByte = (byte)(0xFF * r);
            var gByte = (byte)(0xFF * g);
            var bByte = (byte)(0xFF * b);
            return (uint)(0xFF << 24 | bByte << 16 | gByte << 8 | rByte);
        }
    }
}
