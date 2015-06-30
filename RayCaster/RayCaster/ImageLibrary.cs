using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class ImageLibrary
    {
        readonly uint[] _colorPalette;
        readonly Dictionary<string, IndexedColorTexture> _textures;
        readonly uint[] _colorRamp;

        public ImageLibrary(Tuple<string, Texture2D>[] namedResources)
        {
            var rawTextures = namedResources.Select(namedResource =>
            {
                var buffer = new uint[namedResource.Item2.Width * namedResource.Item2.Height];
                namedResource.Item2.GetData(buffer);
                return new
                {
                    Name = namedResource.Item1,
                    Width = namedResource.Item2.Width,
                    Height = namedResource.Item2.Height,
                    RowFirstPixels = buffer,
                };
            }).ToArray();

            var uniqueColors = new HashSet<uint>();

            foreach (var tex in rawTextures)
            {
                foreach (var color in tex.RowFirstPixels)
                {
                    uniqueColors.Add(color);
                }
            }

            _colorPalette = uniqueColors.ToArray();
            var colorToIndex =
                _colorPalette.
                Select((color, index) => new { Color = color, Index = index }).
                ToDictionary(pair => pair.Color, pair => pair.Index);

            _colorRamp = new uint[_colorPalette.Length * LightLevels.NumberOfLevels];

            for (int colorIndex = 0; colorIndex < _colorPalette.Length; colorIndex++)
            {
                var currentColor = _colorPalette[colorIndex];
                var currentHsvColor = HsvColor.FromPackedRgb(currentColor);
                var offset = colorIndex * LightLevels.NumberOfLevels;

                for (int i = 0; i < LightLevels.NumberOfLevels; i++)
                {
                    var percentage = (i + 1) * LightLevels.BrightnessStep;

                    _colorRamp[offset + i] = currentHsvColor.Mutate(vx: v => Math.Min(1f, percentage * v)).ToPackedRgbColor();
                }
            }

            _textures = rawTextures.ToDictionary(
                    rt => rt.Name,
                    rt => IndexedColorTexture.FromTextureResource(rt.Width, rt.Height, rt.RowFirstPixels, colorToIndex));
        }

        public IndexedColorTexture GetTexture(string name)
        {
            return _textures[name];
        }

        public uint GetColor(int paletteIndex)
        {
            return GetColor(paletteIndex, LightLevels.FullBrightIndex);
        }

        public uint GetColor(int paletteIndex, int lightLevel)
        {
            return _colorRamp[paletteIndex * LightLevels.NumberOfLevels + lightLevel];
        }
    }
}
