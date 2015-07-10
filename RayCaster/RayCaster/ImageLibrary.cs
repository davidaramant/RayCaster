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

            var colorPalette = uniqueColors.ToArray();
            var colorToIndex =
                    colorPalette.
                    Select((color, index) => new { Color = color, Index = index }).
                    ToDictionary(pair => pair.Color, pair => pair.Index);

            _colorRamp = CreateColorRamp(colorPalette);

            _textures = allRawTextures.ToDictionary(
                    rt => rt.Name,
                    rt => IndexedColorTexture.FromTextureResource(rt.Width, rt.Height, rt.RowFirstPixels, colorToIndex));
        }

        private uint[] CreateColorRamp(uint[] colorPalette)
        {
            var colorRamp = new uint[colorPalette.Length << LightLevels.NumberOfLightLevelsPower];

            for (int colorIndex = 0; colorIndex < colorPalette.Length; colorIndex++)
            {
                var currentColor = colorPalette[colorIndex];
                var currentHsvColor = HsvColor.FromPackedRgb(currentColor);
                var offset = colorIndex << LightLevels.NumberOfLightLevelsPower;

                for (int i = 0; i < LightLevels.FullBrightIndex; i++)
                {
                    var percentDone = i / (float)LightLevels.FullBrightIndex;

                    var scale = Lerp(LightLevels.MinValue, 1f, percentDone);

                    colorRamp[offset + i] =
                        currentHsvColor.Mutate(vx: v => scale * v).ToPackedRgbColor();
                }

                var numberOfLevelsRemaining = LightLevels.NumberOfLevels - LightLevels.FullBrightIndex;

                for (int i = 0; i < numberOfLevelsRemaining; i++)
                {
                    var percentDone = i / (float)numberOfLevelsRemaining;

                    var scale = Lerp(1f, LightLevels.MaxOverbright, percentDone);

                    colorRamp[offset + i + LightLevels.FullBrightIndex] =
                        currentHsvColor.Mutate(
                            sx: s => Math.Min(1f, scale * s),
                            vx: v => Math.Min(1f, scale * v)).ToPackedRgbColor();
                }
            }

            return colorRamp;
        }

        private static float Lerp(float v0, float v1, float t)
        {
            return (1f - t) * v0 + t * v1;
        }

        public IndexedColorTexture GetTexture(string name)
        {
            return _textures[name];
        }

        public uint GetColor(int paletteIndex)
        {
            return GetColor(paletteIndex, LightLevels.FullBrightIndex);
        }

        public uint GetColor(int paletteIndex, byte lightLevel)
        {
            return _colorRamp[(paletteIndex << LightLevels.NumberOfLightLevelsPower) + lightLevel];
        }
    }
}
