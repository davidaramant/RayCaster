using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class MapData
    {
        struct SectorInfo
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

            var rockTextures = 
                Enumerable.Range(1, 5).
                Select(i => _imageLibrary.GetTexture("RockMiddle" + i)).
                ToArray();
            var rockTexturesDark = 
                Enumerable.Range(1, 5).
                Select(i => _imageLibrary.GetTexture("RockMiddle" + i + "Darkened")).
                ToArray();

            _sectors = new SectorInfo[MapHeight * MapWidth];
            _hasWalls = new bool[MapHeight * MapWidth];
            _lightLevels = new int[MapHeight * MapWidth];

            var rand = new Random();

            for (int index = 0; index < MapHeight * MapWidth; index++)
            {
                _hasWalls[index] = walls[index] == '#';

                _sectors[index] = new SectorInfo(
                    floor: rockTextures[rand.Next(5)], 
                    ceiling: rockTextures[rand.Next(5)], 
                    wall: rockTextures[rand.Next(5)], 
                    darkWall: rockTexturesDark[rand.Next(5)]);
                _lightLevels[index] = LightLevels.FullBrightIndex;
            }

            // HACK: Put in a light level gradient across the top wall
            for (int i = 0; i < LightLevels.NumberOfLevels; i++)
            {
                var offset = 1 + i;
                _lightLevels[PositionToMapIndex(x: offset, y: 1)] = i;
            }
        }

        public bool HasWalls(Point position)
        {
            return _hasWalls[PositionToMapIndex(position)];
        }

        public bool IsPassable(int x, int y)
        {
            // Because of bounding boxes, the position passed in may be outside the bounds of the map
            if (IsInvalidPosition(x, y))
            {
                return false;
            }

            return !_hasWalls[PositionToMapIndex(x, y)];
        }

        public IndexedColorTexture GetWallTexture(float x, float y, SectorSide sideHit)
        {
            if (sideHit == SectorSide.North || sideHit == SectorSide.South)
                return _sectors[PositionToMapIndex(x,y)].Wall;
            else
                return _sectors[PositionToMapIndex(x,y)].DarkWall;
        }

        public IndexedColorTexture GetFloorTexture(float x, float y)
        {
            return _sectors[PositionToMapIndex(x,y)].Floor;
        }

        public IndexedColorTexture GetCeilingTexture(float x, float y)
        {
            return _sectors[PositionToMapIndex(x,y)].Ceiling;
        }

        public uint Shade(float x, float y, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, _lightLevels[PositionToLightIndex(x, y)]);
        }

        public uint Shade(float x, float y, SectorSide sideHit, int paletteIndex, double distance)
        {
            var adjustedX = x;
            var adjustedY = y;

            switch (sideHit)
            {
                case SectorSide.North:
                    adjustedY -= 1f;
                    break;
                case SectorSide.South:
                    adjustedY += 1f;
                    break;
                case SectorSide.East:
                    adjustedX += 1;
                    break;
                case SectorSide.West:
                default:
                    adjustedX -= 1;
                    break;
            }

            return Shade(adjustedX, adjustedY, paletteIndex, distance);
        }

        #region TODO: Kill
        public IndexedColorTexture GetWallTexture(Point position, SectorSide sideHit)
        {
            if (sideHit == SectorSide.North || sideHit == SectorSide.South)
                return _sectors[PositionToMapIndex(position)].Wall;
            else
                return _sectors[PositionToMapIndex(position)].DarkWall;
        }

        public IndexedColorTexture GetFloorTexture(Point position)
        {
            return _sectors[PositionToMapIndex(position)].Floor;
        }

        public IndexedColorTexture GetCeilingTexture(Point position)
        {
            return _sectors[PositionToMapIndex(position)].Ceiling;
        }

        public uint Shade(Point position, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, _lightLevels[PositionToMapIndex(position)]);
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

            return Shade(adjustedPosition, paletteIndex, distance);
        }

        #endregion KILL

        private bool IsInvalidPosition(int x, int y)
        {
            return x < 0 || x >= MapWidth || y < 0 || y >= MapHeight;
        }

        private int PositionToMapIndex(float x, float y)
        {
            return PositionToMapIndex((int)x, (int)y);
        }

        private int PositionToMapIndex(int x, int y)
        {
            return y * MapWidth + x;
        }

        private int PositionToMapIndex(Point point)
        {
            return PositionToMapIndex(point.X, point.Y);
        }

        private int PositionToLightIndex(float x, float y)
        {
            return ((int)y) * MapWidth + (int)x;
        }
    }
}
