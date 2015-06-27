using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class MapData
    {
        readonly int _mapWidth;
        readonly int _mapHeight;
        SectorInfo[] _sectors;

        public MapData(IEnumerable<Texture> textures)
        {
            var textureLibrary = textures.ToDictionary(tex => tex.Name, tex => tex);

            // HACK: Make a different floor/ceiling texture.
            textureLibrary["otherceiling"] = Mutate("otherceiling", textureLibrary["FLOOR0_1"]);
            textureLibrary["otherfloor"] = Mutate("otherfloor", textureLibrary["FLAT3"]);

            _mapWidth = 29;
            _mapHeight = 24;

            var mapData =
                //0        1         2
                //1234567890123456789012345678
                "11111111111111111111111111111" + //  0
                "1                      111111" + //  1
                "1                      111111" + //  2
                "1                      111111" + //  3
                "1     22222    3+3+3   111111" + //  4
                "1     2   2    +++++   111111" + //  5
                "1     2   2    3+++3   111111" + //  6
                "1     2   2    +++++   111111" + //  7
                "1     22 22    3+3+3   111111" + //  8 
                "1                           1" + //  9
                "1                           1" + // 10
                "1                      1111 1" + // 11
                "1                      1    1" + // 12
                "1                      1 1111" + // 13
                "1                      1    1" + // 14
                "1                      1111 1" + // 15 
                "144444444         6         1" + // 16
                "14 4    4##            111 11" + // 17 
                "14    5 4##            1    1" + // 18
                "14 4    4              1    1" + // 19
                "14 444444              1    1" + // 20
                "14                     1    1" + // 21
                "144444444              1    1" + // 22
                "11111111111111111111111111111";  // 23

            _sectors = new SectorInfo[_mapHeight * _mapWidth];

            for (int index = 0; index < _mapHeight * _mapWidth;index++ )
            {
                _sectors[index] = CreateSector(mapData[index], textureLibrary);

            }
        }

        private static SectorInfo CreateSector(char mapCode, Dictionary<string, Texture> textureLibrary)
        {
            switch (mapCode)
            {
                case '1':
                    return new SectorInfo(
                        wallTexture: textureLibrary["BROWN96"]);

                case '2':
                    return new SectorInfo(
                        wallTexture: textureLibrary["COMPTILE"]);

                case '3':
                    return new SectorInfo(
                        wallTexture: textureLibrary["STARGR2"]);

                case '4':
                    return new SectorInfo(
                        wallTexture: textureLibrary["STARTAN2"]);

                case '5':
                    return new SectorInfo(
                        wallTexture: textureLibrary["TEKWALL1"]);

                case '6':
                    return new SectorInfo(
                        lightLevel: 0.5f,
                        floorTexture: textureLibrary["otherfloor"],
                        ceilingTexture: textureLibrary["otherceiling"],
                        northTexture: textureLibrary["FLOOR0_1"],
                        southTexture: textureLibrary["FLOOR0_1"],
                        westTexture: textureLibrary["FLAT3"],
                        eastTexture: textureLibrary["FLAT3"],
                        passable: true);

                case '+':
                    return new SectorInfo(
                        lightLevel: 0.5f,
                        floorTexture: textureLibrary["otherfloor"],
                        ceilingTexture: textureLibrary["otherceiling"],
                        passable: true);

                case '#':
                    return new SectorInfo(
                        lightLevel: 1.25f,
                        floorTexture: textureLibrary["otherfloor"],
                        ceilingTexture: textureLibrary["otherceiling"],
                        passable: false);

                case ' ':
                default:
                    return new SectorInfo(
                        lightLevel: 0.95f,
                        floorTexture: textureLibrary["FLAT3"],
                        ceilingTexture: textureLibrary["FLOOR0_1"],
                        passable: true);
            }
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



        public bool HasWalls(Point position)
        {
            return _sectors[PositionToIndex(position)].HasWalls;
        }

        public bool IsPassable(int x, int y)
        {
            // Because of bounding boxes, the position passed in may be outside the bounds of the map
            if (IsInvalidPosition(x, y))
            {
                return false;
            }

            return _sectors[PositionToIndex(x, y)].Passable;
        }

        public SectorInfo GetSectorInfo(Point position)
        {
            return _sectors[PositionToIndex(position)];
        }

        public SectorInfo GetSectorInfo(Point position, SectorSide side)
        {
            switch (side)
            {
                case SectorSide.North:
                    position = new Point(position.X, position.Y - 1);
                    break;
                case SectorSide.South:
                    position = new Point(position.X, position.Y + 1);
                    break;
                case SectorSide.East:
                    position = new Point(position.X + 1, position.Y);
                    break;
                case SectorSide.West:
                default:
                    position = new Point(position.X - 1, position.Y);
                    break;
            }

            return _sectors[PositionToIndex(position)];
        }


        private bool IsInvalidPosition(int x, int y)
        {
            return x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight;
        }

        private int PositionToIndex(int x, int y)
        {
            return y * _mapWidth + x;
        }

        private int PositionToIndex(Point point)
        {
            return PositionToIndex(point.X, point.Y);
        }
    }
}
