using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class Texture
    {
        public readonly string Name = "Ugly";

        /// <summary>
        /// Column first ordering.
        /// </summary>
        private readonly HsvColor[] _pixels;

        public readonly int Height;
        public readonly int Width;

        public Texture()
        {
            Height = 64;
            Width = 64;

            _pixels = new HsvColor[64 * 64];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    bool xOdd = (x / 8) % 2 == 1;
                    var yOdd = (y / 8) % 2 == 1;

                    var hue = 360F * ((x + y) / (float)(Height + Width));

                    _pixels[y + x * Width] = new HsvColor(hue, 1f, xOdd ^ yOdd ? 1f : 0f);
                }
            }
        }

        public Texture(string name, Texture2D textureData)
        {
            Name = name;
            Height = textureData.Height;
            Width = textureData.Width;

            var buffer = new uint[Height * Width];
            textureData.GetData(buffer);

            _pixels = new HsvColor[Height * Width];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _pixels[y + x * Width] = HsvColor.FromPackedRgb(buffer[x + y * Height]);
                }
            }
        }


        public HsvColor this[int x, int y]
        {
            get { return _pixels[y + x * Width]; }
        }
    }
}
