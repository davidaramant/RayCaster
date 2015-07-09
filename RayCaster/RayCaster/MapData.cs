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
                "# #   ## ##    # # #   ######" + //  8 
                "# #                         #" + //  9
                "# #                         #" + // 10
                "# #                    #### #" + // 11
                "# #                    #    #" + // 12
                "# #                    # ####" + // 13
                "# #                    #    #" + // 14
                "# #                    #### #" + // 15 
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
            _lightLevels = new int[MapHeight * MapWidth * 4];

            var rand = new Random();

            for (int index = 0; index < MapHeight * MapWidth; index++)
            {
                _hasWalls[index] = walls[index] == '#';

                _sectors[index] = new SectorInfo(
                    floor: rockTextures[rand.Next(5)],
                    ceiling: rockTextures[rand.Next(5)],
                    wall: rockTextures[rand.Next(5)],
                    darkWall: rockTexturesDark[rand.Next(5)]);
            }

            for (int index = 0; index < _lightLevels.Length; index++)
            {
                _lightLevels[index] = LightLevels.FullBrightIndex;
            }

            // HACK: Put in a light level gradient
            for (int i = 0; i < LightLevels.NumberOfLevels; i++)
            {
                var offset = 0.5f * i;
                _lightLevels[WorldPositionToLightIndex(worldX: 1f, worldY: 15f - offset)] = i;
                _lightLevels[WorldPositionToLightIndex(worldX: 1.5f, worldY: 15f - offset)] = i;
            }
        }

        public bool HasWalls(Point position)
        {
            return _hasWalls[MapPointToMapIndex(position)];
        }

        public bool IsPassable(int x, int y)
        {
            // Because of bounding boxes, the position passed in may be outside the bounds of the map
            if (IsInvalidPosition(x, y))
            {
                return false;
            }

            return !_hasWalls[MapPositionToMapIndex(x, y)];
        }

        public IndexedColorTexture GetWallTexture(float worldX, float worldY, SectorSide sideHit)
        {
            if (sideHit == SectorSide.North || sideHit == SectorSide.South)
                return _sectors[WorldPositionToMapIndex(worldX, worldY)].Wall;
            else
                return _sectors[WorldPositionToMapIndex(worldX, worldY)].DarkWall;
        }

        public IndexedColorTexture GetFloorTexture(float worldX, float worldY)
        {
            return _sectors[WorldPositionToMapIndex(worldX, worldY)].Floor;
        }

        public IndexedColorTexture GetCeilingTexture(float worldX, float worldY)
        {
            return _sectors[WorldPositionToMapIndex(worldX, worldY)].Ceiling;
        }

        public uint Shade(float worldX, float worldY, int paletteIndex, double distance)
        {
            return _imageLibrary.GetColor(paletteIndex, _lightLevels[WorldPositionToLightIndex(worldX, worldY)]);
        }

        public uint Shade(float worldX, float worldY, SectorSide sideHit, int paletteIndex, double distance)
        {
            var adjustedX = worldX;
            var adjustedY = worldY;

            switch (sideHit)
            {
                case SectorSide.North:
                    adjustedY -= 0.5f;
                    break;
                case SectorSide.South:
                    adjustedY += 0.5f;
                    break;
                case SectorSide.East:
                    adjustedX += 0.5f;
                    break;
                case SectorSide.West:
                default:
                    adjustedX -= 0.5f;
                    break;
            }

            return Shade(adjustedX, adjustedY, paletteIndex, distance);
        }

        private bool IsInvalidPosition(int mapX, int mapY)
        {
            return mapX < 0 || mapX >= MapWidth || mapY < 0 || mapY >= MapHeight;
        }

        private int WorldPositionToMapIndex(float worldX, float worldY)
        {
            return MapPositionToMapIndex((int)worldX, (int)worldY);
        }

        private int MapPositionToMapIndex(int mapX, int mapY)
        {
            return mapY * MapWidth + mapX;
        }

        private int MapPointToMapIndex(Point point)
        {
            return MapPositionToMapIndex(point.X, point.Y);
        }

        private int WorldPositionToLightIndex(float worldX, float worldY)
        {
            return ((int)(2f * worldY)) * (2 * MapWidth) + (int)(2f * worldX);
        }
    }
}
