using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class MapData
    {
        sealed class SectorInfo
        {
            public readonly IndexedColorTexture Floor;
            public readonly IndexedColorTexture Ceiling;
            public readonly IndexedColorTexture Wall;
            public readonly IndexedColorTexture DarkWall;

            public SectorInfo(
                IndexedColorTexture floor,
                IndexedColorTexture ceiling,
                IndexedColorTexture wall,
                IndexedColorTexture darkWall)
            {
                Floor = floor;
                Ceiling = ceiling;
                Wall = wall;
                DarkWall = darkWall;
            }
        }
        public readonly int MapWidth;
        public readonly int MapHeight;
        ImageLibrary _imageLibrary;

        SectorInfo[] _sectors;
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

            var rockSectorTypes = Enumerable.Range(1, 5).Select(i =>
                new SectorInfo(
                    floor: _imageLibrary.GetTexture("RockMiddle" + i),
                    ceiling: _imageLibrary.GetTexture("RockMiddle" + i),
                    wall: _imageLibrary.GetTexture("RockMiddle" + i),
                    darkWall: _imageLibrary.GetTexture("RockMiddle" + i + "Darkened"))).ToArray();

            _sectors = new SectorInfo[MapHeight * MapWidth];
            _hasWalls = new bool[MapHeight * MapWidth];
            _lightLevels = new int[MapHeight * MapWidth];

            var rand = new Random();

            for (int index = 0; index < MapHeight * MapWidth; index++)
            {
                _hasWalls[index] = walls[index] == '#';
                _sectors[index] = rockSectorTypes[rand.Next(5)];
                _lightLevels[index] = LightLevels.FullBrightIndex;
            }

            // HACK: Put in a light level gradient across the top wall
            for( int i = 0; i < LightLevels.NumberOfLevels; i++ )
            {
                var offset = 1 + i;
                _lightLevels[PositionToIndex(x: offset, y: 1)] = i;
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
            if (sideHit == SectorSide.North || sideHit == SectorSide.South)
                return _sectors[PositionToIndex(position)].Wall;
            else
                return _sectors[PositionToIndex(position)].DarkWall;
        }

        public IndexedColorTexture GetFloorTexture(Point position)
        {
            return _sectors[PositionToIndex(position)].Floor;
        }

        public IndexedColorTexture GetCeilingTexture(Point position)
        {
            return _sectors[PositionToIndex(position)].Ceiling;
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
