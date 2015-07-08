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
        private sealed class RawTexture
        {
            public string Name;
            public int Width;
            public int Height;
            public uint[] RowFirstPixels;
        }

        readonly uint[] _colorPalette;
        readonly Dictionary<string, IndexedColorTexture> _textures;
        readonly uint[] _colorRamp;

        public ImageLibrary(Tuple<string, Texture2D>[] namedResources)
        {
            var rawTextures = namedResources.Select(namedResource =>
            {
                var buffer = new uint[namedResource.Item2.Width * namedResource.Item2.Height];
                namedResource.Item2.GetData(buffer);
                return new RawTexture
                {
                    Name = namedResource.Item1,
                    Width = namedResource.Item2.Width,
                    Height = namedResource.Item2.Height,
                    RowFirstPixels = buffer,
                };
            }).ToArray();

            //HACK: Create darkened versions of textures
            var darkenedRawTextures = rawTextures.Select(rt => new RawTexture
            {
                Name = rt.Name + "Darkened",
                Width = rt.Width,
                Height = rt.Height,
                RowFirstPixels =
                    rt.RowFirstPixels.
                        Select(color => HsvColor.FromPackedRgb(color).Mutate(vx: v => v * .75f).
                        ToPackedRgbColor()).
                        ToArray(),
            });

            var allRawTextures = rawTextures.Concat(darkenedRawTextures).ToArray();

            var uniqueColors = new HashSet<uint>();

            foreach (var tex in allRawTextures)
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
                    _colorRamp[offset + i] = currentHsvColor.Mutate(
                                sx: s => Math.Min(1f, LightLevels.SaturationFactors[i] * s),
                                vx: v => Math.Min(1f, LightLevels.ValueFactors[i] * v)).ToPackedRgbColor();
                }
            }

            _textures = allRawTextures.ToDictionary(
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
