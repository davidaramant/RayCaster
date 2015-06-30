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
        public static readonly Texture Empty = new Texture("empty", 1, 1, new uint[1] { uint.MaxValue });

        public readonly string Name = "Ugly";

        /// <summary>
        /// Column first ordering.
        /// </summary>
        private readonly uint[] _pixels;

        public readonly int Height;
        public readonly int Width;

        public Texture(string name, int width, int height, uint[] pixels)
        {
            if (pixels.Length != width * height)
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

            var pixels = new uint[textureData.Height * textureData.Width];

            for (int x = 0; x < textureData.Width; x++)
            {
                for (int y = 0; y < textureData.Height; y++)
                {
                    pixels[y + x * textureData.Width] = buffer[x + y * textureData.Height];
                }
            }

            return new Texture(name, width: textureData.Width, height: textureData.Height, pixels: pixels);
        }


        public uint this[int x, int y]
        {
            get { return _pixels[y + x * Width]; }
        }
    }
}
