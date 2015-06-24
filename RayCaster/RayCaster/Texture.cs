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

        public static Texture GenerateTexture()
        {
            int height = 64;
            int width = 64;

            var pixels = new HsvColor[64 * 64];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool xOdd = (x / 8) % 2 == 1;
                    var yOdd = (y / 8) % 2 == 1;

                    var hue = 360F * ((x + y) / (float)(height + width));

                    pixels[y + x * width] = new HsvColor(hue, 1f, xOdd ^ yOdd ? 1f : 0f);
                }
            }

            return new Texture("Ugly", width: width, height: height, pixels: pixels);
        }

        public Texture( string name, int width, int height, HsvColor[] pixels)
        {
            if( pixels.Length != width * height )
            {
                throw new ArgumentException("Pixel buffer of texture doesn't match size.");
            }

            Name = name;
            Width = width;
            Height = height;
            _pixels = pixels;
        }

        public static Texture FromTextureResource(string name, Texture2D textureData)
        {
            var buffer = new uint[textureData.Height * textureData.Width];
            textureData.GetData(buffer);

            var pixels = new HsvColor[textureData.Height * textureData.Width];

            for (int x = 0; x < textureData.Width; x++)
            {
                for (int y = 0; y < textureData.Height; y++)
                {
                    pixels[y + x * textureData.Width] = HsvColor.FromPackedRgb(buffer[x + y * textureData.Height]);
                }
            }

            return new Texture(name, width: textureData.Width, height: textureData.Height, pixels: pixels);
        }


        public HsvColor this[int x, int y]
        {
            get { return _pixels[y + x * Width]; }
        }
    }
}
