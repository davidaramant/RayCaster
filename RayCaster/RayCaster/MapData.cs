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
        ImageLibrary _imageLibrary;

        public MapData(ImageLibrary imageLibrary)
        {
            _imageLibrary = imageLibrary;

            _mapWidth = 29;
            _mapHeight = 24;

            var mapData =
                //0        1         2
                //1234567890123456789012345678
                "11111111111111111111111111111" + //  0
                "1                      111111" + //  1
                "1                      111111" + //  2
                "1                      111111" + //  3
                "1     22222    3 3 3   111111" + //  4
                "1     2   2            111111" + //  5
                "1     2   2    3   3   111111" + //  6
                "1     2   2            111111" + //  7
                "1     22 22    3 3 3   111111" + //  8 
                "1                           1" + //  9
                "1                           1" + // 10
                "1                      1111 1" + // 11
                "1                      1    1" + // 12
                "1                      1 1111" + // 13
                "1                      1    1" + // 14
                "1                      1111 1" + // 15 
                "144444444                   1" + // 16
                "14 4    4              111 11" + // 17 
                "14    5 4              1    1" + // 18
                "14 4    4              1    1" + // 19
                "14 444444              1    1" + // 20
                "14                     1    1" + // 21
                "144444444              1    1" + // 22
                "11111111111111111111111111111";  // 23

            _sectors = new SectorInfo[_mapHeight * _mapWidth];

            for (int index = 0; index < _mapHeight * _mapWidth; index++)
            {
                _sectors[index] = CreateSector(mapData[index], _imageLibrary);

            }
        }

        private static SectorInfo CreateSector(char mapCode, ImageLibrary imageLibrary)
        {
            switch (mapCode)
            {
                case '1':
                    return new SectorInfo(
                        library: imageLibrary,
                        wallTexture: imageLibrary.GetTexture("BROWN96"));

                case '2':
                    return new SectorInfo(
                        library: imageLibrary,
                        wallTexture: imageLibrary.GetTexture("COMPTILE"));

                case '3':
                    return new SectorInfo(
                        library: imageLibrary,
                        wallTexture: imageLibrary.GetTexture("STARGR2"));

                case '4':
                    return new SectorInfo(
                        library: imageLibrary,
                        wallTexture: imageLibrary.GetTexture("STARTAN2"));

                case '5':
                    return new SectorInfo(
                        library: imageLibrary,
                        wallTexture: imageLibrary.GetTexture("TEKWALL1"));

                case ' ':
                default:
                    return new SectorInfo(
                        library: imageLibrary,
                        lightLevel: 9,
                        floorTexture: imageLibrary.GetTexture("FLAT3"),
                        ceilingTexture: imageLibrary.GetTexture("FLOOR0_1"),
                        passable: true);
            }
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

        public IndexedColorTexture GetWallTexture(Point position, SectorSide sideHit)
        {
            return _sectors[PositionToIndex(position)].GetWallTexture(sideHit);
        }

        public IndexedColorTexture GetFloorTexture(Point position)
        {
            return _sectors[PositionToIndex(position)].FloorTexture;
        }

        public IndexedColorTexture GetCeilingTexture(Point position)
        {
            return _sectors[PositionToIndex(position)].CeilingTexture;
        }

        public uint Shade(Point position, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, LightLevels.FullBrightIndex);
        }

        public uint Shade(Point position, SectorSide sideHit, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, LightLevels.FullBrightIndex);
        }

        private SectorInfo GetSectorInfo(Point position)
        {
            return _sectors[PositionToIndex(position)];
        }

        private SectorInfo GetSectorInfo(Point position, SectorSide side)
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
