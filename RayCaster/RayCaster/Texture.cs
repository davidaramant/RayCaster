using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayCasterGame
{
    sealed class Texture
    {
        /// <summary>
        /// Column first ordering.
        /// </summary>
        private readonly HsvColor[] _pixels;

        public readonly int Height = 64;
        public readonly int Width = 64;

        public Texture()
        {
            _pixels = new HsvColor[64 * 64];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    bool xOdd = (x / 8) % 2 == 1;
                    var yOdd = (y / 8) % 2 == 1;

                    var hue = 360F * ((x + y) / (float)(Height + Width));

                    _pixels[y + x * Width] = new HsvColor(hue,1f, xOdd ^ yOdd ? 1f : 0f);
                }
            }
        }

        public HsvColor this[int x, int y]
        {
            get { return _pixels[y + x * Width]; }
        }
    }
}
