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
    public sealed class HsvColor
    {
        /// <summary>
        /// Hue (angle in degrees)
        /// </summary>
        public double Hue;
        /// <summary>
        /// Saturation (percentage)
        /// </summary>
        public double Saturation;
        /// <summary>
        /// Value (percentage)
        /// </summary>
        public double Value;

        public static HsvColor FromRgb(Color color)
        {
            HsvColor output = new HsvColor();
            double min, max, delta;

            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            min = r < g ? r : g;
            min = min < b ? min : b;

            max = r > g ? r : g;
            max = max > b ? max : b;

            output.Value = max;
            delta = max - min;
            if (max > 0.0)
            { // NOTE: if Max is == 0, this divide would cause a crash
                output.Saturation = (delta / max);
            }
            else
            {
                // if max is 0, then r = g = b = 0              
                // s = 0, v is undefined
                output.Saturation = 0.0;
                output.Hue = 0;
                output.Value = 0;
                return output;
            }
            if (r >= max)
                output.Hue = (g - b) / delta;        // between yellow & magenta
            else
                if (g >= max)
                    output.Hue = 2.0 + (b - r) / delta;  // between cyan & yellow
                else
                    output.Hue = 4.0 + (r - g) / delta;  // between magenta & cyan

            output.Hue *= 60.0;                              // degrees

            if (output.Hue < 0.0)
                output.Hue += 360.0;

            return output;
        }


        public uint ToPackedRgbColor()
        {
            double hh, p, q, t, ff;
            long i;
            double r = 0;
            double g = 0;
            double b = 0;

            if (Saturation <= 0.0)
            {
                r = Value;
                g = Value;
                b = Value;
                return ToPackedColor(r, g, b);
            }
            hh = Hue;
            if (hh >= 360.0)
                hh = 0.0;
            hh /= 60.0;
            i = (long)hh;
            ff = hh - i;
            p = Value * (1.0 - Saturation);
            q = Value * (1.0 - (Saturation * ff));
            t = Value * (1.0 - (Saturation * (1.0 - ff)));

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

        private static uint ToPackedColor(double r, double g, double b)
        {
            return new Color((float)r, (float)g, (float)b).PackedValue;
        }
    }
}
