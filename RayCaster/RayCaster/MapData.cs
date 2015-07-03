using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class MapData
    {
        enum SectorType : byte
        {
            Rock,
            Tech
        }
        readonly int _mapWidth;
        readonly int _mapHeight;
        ImageLibrary _imageLibrary;

        // Use the "data structure of arrays" design pattern
        SectorType[] _sectorTypes;
        bool[] _hasWalls;

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

            _sectorTypes = new SectorType[_mapHeight * _mapWidth];
            _hasWalls = new bool[_mapHeight * _mapWidth];

            for (int index = 0; index < _mapHeight * _mapWidth; index++)
            {
                switch (mapData[index])
                {
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                        _sectorTypes[index] = SectorType.Rock;
                        _hasWalls[index] = true;
                        break;

                    case ' ':
                    default:
                        _sectorTypes[index] = SectorType.Rock;
                        _hasWalls[index] = false;
                        break;
                }
            }
        }

        public bool HasWalls(Point position)
        {
            return _hasWalls[PositionToIndex(position)];
        }

        public bool IsPassable(int x, int y)
        {
            // Because of bounding boxes, the position passed in may be outside the bounds of the map
            if (IsInvalidPosition(x, y))
            {
                return false;
            }

            return !_hasWalls[PositionToIndex(x, y)];
        }

        public IndexedColorTexture GetWallTexture(Point position, SectorSide sideHit)
        {
            switch (_sectorTypes[PositionToIndex(position)])
            {
                case SectorType.Rock:
                    if (sideHit == SectorSide.North | sideHit == SectorSide.South)
                        return _imageLibrary.GetTexture("STARGR2");
                    else
                        return _imageLibrary.GetTexture("STARTAN2");

                case SectorType.Tech:
                default:
                    throw new Exception("What");
            }
        }

        public IndexedColorTexture GetFloorTexture(Point position)
        {
            switch (_sectorTypes[PositionToIndex(position)])
            {
                case SectorType.Rock:
                    return _imageLibrary.GetTexture("FLOOR0_1");

                case SectorType.Tech:
                default:
                    throw new Exception("What");
            }
        }

        public IndexedColorTexture GetCeilingTexture(Point position)
        {
            switch (_sectorTypes[PositionToIndex(position)])
            {
                case SectorType.Rock:
                    return _imageLibrary.GetTexture("FLAT3");

                case SectorType.Tech:
                default:
                    throw new Exception("What");
            }
        }

        public uint Shade(Point position, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, LightLevels.FullBrightIndex);
        }

        public uint Shade(Point position, SectorSide sideHit, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, LightLevels.FullBrightIndex);
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
