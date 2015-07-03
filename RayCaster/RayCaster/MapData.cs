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
        public readonly int MapWidth;
        public readonly int MapHeight;
        ImageLibrary _imageLibrary;

        // Use the "data structure of arrays" design pattern
        SectorType[] _sectorTypes;
        bool[] _hasWalls;
        int[] _lightLevels;

        public MapData(ImageLibrary imageLibrary)
        {
            _imageLibrary = imageLibrary;

            MapWidth = 29;
            MapHeight = 24;

            var walls =
                //0        1         2
                //1234567890123456789012345678
                "#############################" + //  0
                "#                      ######" + //  1
                "#                      ######" + //  2
                "#                      ######" + //  3
                "#     #####    # # #   ######" + //  4
                "#     #   #            ######" + //  5
                "#     #   #    #   #   ######" + //  6
                "#     #   #            ######" + //  7
                "#     ## ##    # # #   ######" + //  8 
                "#                           #" + //  9
                "#                           #" + // 10
                "#                      #### #" + // 11
                "#                      #    #" + // 12
                "#                      # ####" + // 13
                "#                      #    #" + // 14
                "#                      #### #" + // 15 
                "#########                   #" + // 16
                "## #    #              ### ##" + // 17 
                "##    # #              #    #" + // 18
                "## #    #              #    #" + // 19
                "## ######              #    #" + // 20
                "##                     #    #" + // 21
                "#########              #    #" + // 22
                "#############################";  // 23
            var lightLevels =
                //0        1         2
                //1234567890123456789012345678
                "99999999999999999999999999999" + //  0
                "99012456789ab9999999999999999" + //  1
                "99999999999999999999999999999" + //  2
                "99999999999999999999999999999" + //  3
                "99999999999999999999999999999" + //  4
                "99999999999999999999999999999" + //  5
                "99999999999999999999999999999" + //  6
                "99999999999999999999999999999" + //  7
                "99999999999999999999999999999" + //  8 
                "99999999999999999999999999999" + //  9
                "99999999999999999999999999999" + // 10
                "99999999999999999999999999999" + // 11
                "99999999999999999999999999999" + // 12
                "99999999999999999999999999999" + // 13
                "99999999999999999999999999999" + // 14
                "99999999999999999999999999999" + // 15 
                "99999999999999999999999999999" + // 16
                "99999999999999999999999999999" + // 17 
                "99999999999999999999999999999" + // 18
                "99999999999999999999999999999" + // 19
                "99999999999999999999999999999" + // 20
                "99999999999999999999999999999" + // 21
                "99999999999999999999999999999" + // 22
                "99999999999999999999999999999";  // 23

            _sectorTypes = new SectorType[MapHeight * MapWidth];
            _hasWalls = new bool[MapHeight * MapWidth];
            _lightLevels = new int[MapHeight * MapWidth];

            for (int index = 0; index < MapHeight * MapWidth; index++)
            {
                switch (walls[index])
                {
                    case '#':
                        _sectorTypes[index] = SectorType.Rock;
                        _hasWalls[index] = true;
                        break;

                    case ' ':
                    default:
                        _sectorTypes[index] = SectorType.Rock;
                        _hasWalls[index] = false;
                        break;
                }

                _lightLevels[index] = int.Parse(lightLevels[index].ToString(), System.Globalization.NumberStyles.HexNumber);
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
            return _imageLibrary.GetColor(paletteIndex, _lightLevels[PositionToIndex(position)]);
        }

        public uint Shade(Point position, SectorSide sideHit, int paletteIndex, double distance)
        {
            Point adjustedPosition;

            switch (sideHit)
            {
                case SectorSide.North:
                    adjustedPosition = new Point(position.X, position.Y - 1);
                    break;
                case SectorSide.South:
                    adjustedPosition = new Point(position.X, position.Y + 1);
                    break;
                case SectorSide.East:
                    adjustedPosition = new Point(position.X + 1, position.Y);
                    break;
                case SectorSide.West:
                default:
                    adjustedPosition = new Point(position.X - 1, position.Y);
                    break;
            }

            return _imageLibrary.GetColor(paletteIndex, _lightLevels[PositionToIndex(adjustedPosition)]);
        }

        private bool IsInvalidPosition(int x, int y)
        {
            return x < 0 || x >= MapWidth || y < 0 || y >= MapHeight;
        }

        private int PositionToIndex(int x, int y)
        {
            return y * MapWidth + x;
        }

        private int PositionToIndex(Point point)
        {
            return PositionToIndex(point.X, point.Y);
        }
    }
}
