namespace RayCasterGame
{
    sealed class SectorInfo
    {
        public readonly bool Passable;

        public readonly Texture FloorTexture;
        public readonly Texture CeilingTexture;

        private readonly Texture _northTexture;
        private readonly Texture _southTexture;
        private readonly Texture _westTexture;
        private readonly Texture _eastTexture;

        public bool HasWalls
        {
            get
            {
                return
                    _northTexture != null &&
                    _southTexture != null &&
                    _westTexture != null &&
                    _eastTexture != null;
            }
        }

        public Texture GetWallTexture(SectorSide side)
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

        public SectorInfo(
            Texture northTexture,
            Texture southTexture,
            Texture westTexture,
            Texture eastTexture,
            bool passable = false,
            Texture floorTexture = null,
            Texture ceilingTexture = null)
        {
            FloorTexture = floorTexture;
            CeilingTexture = ceilingTexture;
            _northTexture = northTexture;
            _southTexture = southTexture;
            _westTexture = westTexture;
            _eastTexture = eastTexture;

            Passable = passable;
        }

        public SectorInfo(
            Texture floorTexture,
            Texture ceilingTexture,
            bool passable = true)
        {
            FloorTexture = floorTexture;
            CeilingTexture = ceilingTexture;

            Passable = passable;
        }
    }
}
