namespace RayCasterGame
{
    sealed class SectorInfo
    {
        private readonly ImageLibrary _library;

        // TODO: Kill this class

        public readonly bool Passable;

        public readonly IndexedColorTexture FloorTexture;
        public readonly IndexedColorTexture CeilingTexture;

        private readonly IndexedColorTexture _northTexture;
        private readonly IndexedColorTexture _southTexture;
        private readonly IndexedColorTexture _westTexture;
        private readonly IndexedColorTexture _eastTexture;

        // 0 - 2
        // 0 is dark, 1 is fullbright, 2 is overbright
        private readonly int _lightLevel = LightLevels.FullBrightIndex;

        public bool HasWalls
        {
            get
            {
                return
                    _northTexture != IndexedColorTexture.Empty &&
                    _southTexture != IndexedColorTexture.Empty &&
                    _westTexture != IndexedColorTexture.Empty &&
                    _eastTexture != IndexedColorTexture.Empty;
            }
        }

        public IndexedColorTexture GetWallTexture(SectorSide side)
        {
            switch (side)
            {
                case SectorSide.North:
                    return _northTexture;
                case SectorSide.South:
                    return _southTexture;
                case SectorSide.West:
                    return _westTexture;
                case SectorSide.East:
                default:
                    return _eastTexture;
            }
        }

        public uint Shade(int paletteIndex, double distance)
        {
            return _library.GetColor(paletteIndex,_lightLevel);
        }

        public SectorInfo(
            ImageLibrary library,
            IndexedColorTexture wallTexture,
            IndexedColorTexture floorTexture = null,
            IndexedColorTexture ceilingTexture = null,
            int lightLevel = LightLevels.FullBrightIndex,
            bool passable = false)
            : this(
                library: library,
                lightLevel: lightLevel,
                northTexture: wallTexture,
                southTexture: wallTexture,
                westTexture: wallTexture,
                eastTexture: wallTexture,
                passable: passable,
                floorTexture: floorTexture,
                ceilingTexture: ceilingTexture)
        {
        }

        public SectorInfo(
            ImageLibrary library,
            IndexedColorTexture northTexture,
            IndexedColorTexture southTexture,
            IndexedColorTexture westTexture,
            IndexedColorTexture eastTexture,
            int lightLevel = LightLevels.FullBrightIndex,
            bool passable = false,
            IndexedColorTexture floorTexture = null,
            IndexedColorTexture ceilingTexture = null)
        {
            _library = library;
            _lightLevel = lightLevel;
            FloorTexture = floorTexture ?? IndexedColorTexture.Empty;
            CeilingTexture = ceilingTexture ?? IndexedColorTexture.Empty;
            _northTexture = northTexture ?? IndexedColorTexture.Empty;
            _southTexture = southTexture ?? IndexedColorTexture.Empty;
            _westTexture = westTexture ?? IndexedColorTexture.Empty;
            _eastTexture = eastTexture ?? IndexedColorTexture.Empty;

            Passable = passable;
        }

        public SectorInfo(
            ImageLibrary library,
            IndexedColorTexture floorTexture,
            IndexedColorTexture ceilingTexture,
            int lightLevel,
            bool passable = true)
            : this(
                library: library,
                lightLevel: lightLevel,
                northTexture: null,
                southTexture: null,
                westTexture: null,
                eastTexture: null,
                passable: passable,
                floorTexture: floorTexture,
                ceilingTexture: ceilingTexture)
        {
        }
    }
}
