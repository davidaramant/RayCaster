using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayCasterGame
{
    sealed class MapData
    {
        private readonly Dictionary<string, Texture> _textureLibrary;

        const int _mapWidth = 24;
        const int _mapHeight = 24;

        int[,] _worldMap = new int[,]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,9,3,9,3,0,0,0,1},
            {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,9,9,9,9,9,0,0,0,1},
            {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,9,9,9,3,0,0,0,1},
            {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,9,9,9,9,9,0,0,0,1},
            {1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,9,3,9,3,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,9,9,9,9,0,0,0,1},
            {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,9,9,9,9,0,0,0,1},
            {1,4,0,0,0,0,5,0,4,0,0,0,0,0,0,0,9,9,9,9,0,0,0,1},
            {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,9,9,9,9,0,0,0,1},
            {1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };

        public MapData(IEnumerable<Texture> textures)
        {
            _textureLibrary = textures.ToDictionary(tex => tex.Name, tex => tex);

            // HACK: Make a different floor/ceiling texture.
            _textureLibrary["otherceiling"] = Mutate("otherceiling", _textureLibrary["FLOOR0_1"]);
            _textureLibrary["otherfloor"] = Mutate("otherfloor", _textureLibrary["FLAT3"]);
        }

        private static Texture Mutate(string newName, Texture texture)
        {
            var buffer = new HsvColor[texture.Width * texture.Height];

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    buffer[y + x * texture.Width] =
                        texture[x, y].Mutate(
                            hx: h => h - 180,
                            sx: s => Math.Min(1f, 1.5f * s),
                            vx: v => v * 0.50f);
                }
            }

            return new Texture(newName, texture.Width, texture.Height, buffer);
        }


        public bool IsEmpty(Position position)
        {
            return IsEmpty(position.X, position.Y);
        }

        public bool IsEmpty(int x, int y)
        {
            return
                _worldMap[x, y] == 0 ||
                _worldMap[x, y] == 9;
        }

        public Texture GetWallTexture(Position position)
        {
            switch (_worldMap[position.X, position.Y])
            {
                case 1: return _textureLibrary["BROWN96"];
                case 2: return _textureLibrary["COMPTILE"];
                case 3: return _textureLibrary["STARGR2"];
                case 4: return _textureLibrary["STARTAN2"];
                default: return _textureLibrary["TEKWALL1"];
            }
        }

        public Texture GetFloorTexture(Position position)
        {
            switch (_worldMap[position.X, position.Y])
            {
                case 9:
                    return _textureLibrary["otherfloor"];
                default:
                    return _textureLibrary["FLAT3"];
            }
        }

        public Texture GetCeilingTexture(Position position)
        {
            switch (_worldMap[position.X, position.Y])
            {
                case 9:
                    return _textureLibrary["otherceiling"];
                default:
                    return _textureLibrary["FLOOR0_1"];
            }
        }
    }
}
