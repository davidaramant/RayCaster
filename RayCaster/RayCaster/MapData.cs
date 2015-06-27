using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class MapData
    {
        const int _mapWidth = 24;
        const int _mapHeight = 24;

        SectorInfo[] _sectors;

        public MapData(IEnumerable<Texture> textures)
        {
            var textureLibrary = textures.ToDictionary(tex => tex.Name, tex => tex);

            // HACK: Make a different floor/ceiling texture.
            textureLibrary["otherceiling"] = Mutate("otherceiling", textureLibrary["FLOOR0_1"]);
            textureLibrary["otherfloor"] = Mutate("otherfloor", textureLibrary["FLAT3"]);


            var mapData = new char[,]
            {
//                0                                       1                                       2
//                0   1   2   3   4   5   6   7   8   9   0   1   2   3   4   5   6   7   8   9   0   1   2   3            
                {'1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1'}, //  0
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, //  1
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, //  2
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, //  3
                {'1',' ',' ',' ',' ',' ','2','2','2','2','2',' ',' ',' ',' ','3','+','3','+','3',' ',' ',' ','1'}, //  4
                {'1',' ',' ',' ',' ',' ','2',' ',' ',' ','2',' ',' ',' ',' ','+','+','+','+','+',' ',' ',' ','1'}, //  5
                {'1',' ',' ',' ',' ',' ','2',' ',' ',' ','2',' ',' ',' ',' ','3','+','+','+','3',' ',' ',' ','1'}, //  6
                {'1',' ',' ',' ',' ',' ','2',' ',' ',' ','2',' ',' ',' ',' ','+','+','+','+','+',' ',' ',' ','1'}, //  7
                {'1',' ',' ',' ',' ',' ','2','2',' ','2','2',' ',' ',' ',' ','3','+','3','+','3',' ',' ',' ','1'}, //  8 
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, //  9
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 10
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 11
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 12
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 13
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 14
                {'1',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 15 
                {'1','4','4','4','4','4','4','4','4',' ',' ',' ',' ',' ',' ',' ',' ',' ','6',' ',' ',' ',' ','1'}, // 16
                {'1','4',' ','4',' ',' ',' ',' ','4','#','#',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 17 
                {'1','4',' ',' ',' ',' ','5',' ','4','#','#',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 18
                {'1','4',' ','4',' ',' ',' ',' ','4',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 19
                {'1','4',' ','4','4','4','4','4','4',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 20
                {'1','4',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 21
                {'1','4','4','4','4','4','4','4','4',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','1'}, // 22
                {'1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1','1'}  // 23
            };

            _sectors = new SectorInfo[_mapHeight * _mapWidth];

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    _sectors[PositionToIndex(x, y)] = CreateSector(mapData[y, x], textureLibrary);
                }
            }
        }

        private static SectorInfo CreateSector(char mapCode, Dictionary<string, Texture> textureLibrary)
        {
            switch (mapCode)
            {
                case '1':
                    return new SectorInfo(
                        wallTexture: textureLibrary["BROWN96"] );

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
                        passable:true);
                
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
            return _sectors[PositionToIndex(x, y)].Passable;
        }

        public SectorInfo GetSectorInfo(Point position)
        {
            return _sectors[PositionToIndex(position)];
        }

        public SectorInfo GetSectorInfo(Point position, SectorSide side )
        {
            switch(side)
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


        private static int PositionToIndex(int x, int y)
        {
            return y * _mapHeight + x;
        }

        private static int PositionToIndex(Point point)
        {
            return PositionToIndex(point.X, point.Y);
        }
    }
}
