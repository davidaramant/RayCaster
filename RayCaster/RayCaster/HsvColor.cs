using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    /// <summary>
    /// HSV Color.  From http://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both
    /// </summary>
    public struct HsvColor
    {
        /// <summary>
        /// Hue (angle in degrees)
        /// </summary>
        public readonly float Hue;
        /// <summary>
        /// Saturation (percentage)
        /// </summary>
        public readonly float Saturation;
        /// <summary>
        /// Value (percentage)
        /// </summary>
        public readonly float Value;

        public static readonly HsvColor Black = new HsvColor();

        public static readonly HsvColor Red = HsvColor.FromRgb(Color.Red);
        public static readonly HsvColor Blue = HsvColor.FromRgb(Color.Blue);
        public static readonly HsvColor Green = HsvColor.FromRgb(Color.Green);
        public static readonly HsvColor White = HsvColor.FromRgb(Color.White);
        public static readonly HsvColor Yellow = HsvColor.FromRgb(Color.Yellow);

        public HsvColor(float hue,float saturation,float value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public HsvColor AdjustValue( float valueFactor)
        {
            return new HsvColor(Hue, Saturation, valueFactor * Value);
        }

        public static HsvColor FromRgb(Color color)
        {
            float hue = 0;
            float saturation = 0;
            float value = 0;

            float min, max, delta;

            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            min = r < g ? r : g;
            min = min < b ? min : b;

            max = r > g ? r : g;
            max = max > b ? max : b;

            value = max;
            delta = max - min;
            if (max > 0.0)
            { // NOTE: if Max is == 0, this divide would cause a crash
                saturation = (delta / max);
            }
            else
            {
                // if max is 0, then r = g = b = 0              
                // s = 0, v is undefined
                return new HsvColor();
            }
            if (r >= max)
                hue = (g - b) / delta;        // between yellow & magenta
            else
                if (g >= max)
                    hue = 2f + (b - r) / delta;  // between cyan & yellow
                else
                    hue = 4f + (r - g) / delta;  // between magenta & cyan

            hue *= 60f;                              // degrees

            if (hue < 0f)
                hue += 360f;

            return new HsvColor(hue,saturation,value);
        }


        public uint ToPackedRgbColor()
        {
            float hh, p, q, t, ff;
            long i;
            float r = 0;
            float g = 0;
            float b = 0;

            if (Saturation <= 0)
            {
                r = Value;
                g = Value;
                b = Value;
                return ToPackedColor(r, g, b);
            }
            hh = Hue;
            if (hh >= 360f)
                hh = 0;
            hh /= 60f;
            i = (long)hh;
            ff = hh - i;
            p = Value * (1f - Saturation);
            q = Value * (1f - (Saturation * ff));
            t = Value * (1f - (Saturation * (1f - ff)));

            switch (i)
            {
                case 0:
                    r = Value;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = Value;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = Value;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = Value;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = Value;
                    break;
                case 5:
                default:
                    r = Value;
                    g = p;
                    b = q;
                    break;
            }
            return ToPackedColor(r, g, b);
        }

        private static uint ToPackedColor(float r, float g, float b)
        {
            var rByte = (byte)(0xFF*r);
            var gByte = (byte)(0xFF*g);
            var bByte = (byte)(0xFF*b);
            return (uint)(0xFF << 24 | rByte << 16 | gByte << 8 | bByte);
        }
    }
}
