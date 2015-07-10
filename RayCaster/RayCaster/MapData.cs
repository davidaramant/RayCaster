using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    /// <summary>
    /// Holds information about the current map
    /// </summary>
    /// <remarks>
    /// There are three different coordinate spaces relevant for the game:
    /// 
    /// World coordinates - (floating point) Represents the position of objects and the player
    /// Map coordinates - (integer) Represents the location of a sector in the map.  1 map unit = 1 world unit
    /// Light coordinates - (integer) Represents the location of a light level in the map.  1 light unit = 0.5 world unit.  
    /// 
    /// There are four light levels per sector.
    /// 
    /// All position variables include the relevant coordinate system in their name.
    /// </remarks>
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
        byte[] _lightLevels;

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
                "# #       #                 #" + // 10
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
            _lightLevels = new byte[MapHeight * MapWidth * 4];

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

            // HACK: Test that light logic is correct
            // (10,10) in map coordinates is the sector we want to surround.

            // West side
            _lightLevels[WorldPositionToLightIndex(9.5f, 10f)] = 0;
            _lightLevels[WorldPositionToLightIndex(9.5f, 10.5f)] = LightLevels.NumberOfLevels - 1;

            // North side
            _lightLevels[WorldPositionToLightIndex(10f, 9.5f)] = LightLevels.NumberOfLevels - 1;
            _lightLevels[WorldPositionToLightIndex(10.5f, 9.5f)] = 0;

            // East side
            _lightLevels[WorldPositionToLightIndex(11f, 10f)] = LightLevels.NumberOfLevels - 1;
            _lightLevels[WorldPositionToLightIndex(11f, 10.5f)] = 0;

            // South side
            _lightLevels[WorldPositionToLightIndex(10f, 11f)] = 0;
            _lightLevels[WorldPositionToLightIndex(10.5f, 11f)] = LightLevels.NumberOfLevels - 1;
        }

        public bool HasWalls(int mapX, int mapY)
        {
            return _hasWalls[MapPositionToMapIndex(mapX,mapY)];
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

        public uint Shade(int paletteIndex, byte lightLevel)
        {
            return _imageLibrary.GetColor(paletteIndex, lightLevel);
        }

        /// <summary>
        /// Walls are shaded according to the light level in front of the wall.
        /// </summary>
        /// <param name="mapX">The x coordinate of the sector hit.</param>
        /// <param name="mapY">The y coordinate of the sector hit.</param>
        /// <param name="wallX">The offset of where exactly the ray hit the wall (range 0 to 1)</param>
        /// <param name="sideHit">Which side of the sector was hit.</param>
        /// <returns>The light level.</returns>
        public byte FindWallLightLevel(int mapX, int mapY, float wallX, SectorSide sideHit)
        {
            // Map coordinates are the top left (north west) corner of the sector in world coordinates
            // To find the light sector "in front of" the wall, we have to adjust the map position

            float adjustedX = 0;
            float adjustedY = 0;

            switch (sideHit)
            {
                case SectorSide.North:
                    adjustedX = mapX + wallX;
                    adjustedY = mapY - 0.5f;
                    break;
                case SectorSide.South:
                    adjustedX = mapX + wallX;
                    adjustedY = mapY + 1f;
                    break;
                case SectorSide.East:
                    adjustedX = mapX + 1f;
                    adjustedY = mapY + wallX;
                    break;
                case SectorSide.West:
                default:
                    adjustedX = mapX - 0.5f;
                    adjustedY = mapY + wallX;
                    break;
            }

            return _lightLevels[WorldPositionToLightIndex(adjustedX, adjustedY)];
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

        private int WorldPositionToLightIndex(float worldX, float worldY)
        {
            return ((int)(2f * worldY)) * (2 * MapWidth) + (int)(2f * worldX);
        }
    }
}
