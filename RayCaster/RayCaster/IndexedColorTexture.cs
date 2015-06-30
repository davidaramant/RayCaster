using System;
using System.Collections.Generic;

namespace RayCasterGame
{
    sealed class IndexedColorTexture
    {
        public static readonly IndexedColorTexture Empty = new IndexedColorTexture(1, 1, new int[1]);

        /// <summary>
        /// Column first ordering.
        /// </summary>
        private readonly int[] _colorIndices;

        public readonly int Height;
        public readonly int Width;

        public IndexedColorTexture(int width, int height, int[] colorIndexes)
        {
            if (colorIndexes.Length != width * height)
            {
                throw new ArgumentException("Pixel buffer of texture doesn't match size.");
            }

            Width = width;
            Height = height;
            _colorIndices = colorIndexes;
        }

        public static IndexedColorTexture FromTextureResource(int width, int height, uint[] rowFirstColors, Dictionary<uint, int> colorToIndex)
        {
            var pixels = new int[height * width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixels[y + x * width] = colorToIndex[rowFirstColors[x + y * height]];
                }
            }

            return new IndexedColorTexture(width: width, height: height, colorIndexes: pixels);
        }


        public int this[int x, int y]
        {
            get { return _colorIndices[y + x * Width]; }
        }
    }
}
