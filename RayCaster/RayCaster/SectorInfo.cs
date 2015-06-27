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

        // 0 - 2
        // 0 is dark, 1 is fullbright, 2 is overbright
        private readonly float _lightLevel;

        public bool HasWalls
        {
            get
            {
                return
                    _northTexture !=  Texture.Empty &&
                    _southTexture != Texture.Empty &&
                    _westTexture != Texture.Empty &&
                    _eastTexture != Texture.Empty;
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

        public HsvColor Shade(HsvColor color, double distance)
        {
            if (_lightLevel <= 1)
            {
                return color.ScaleValue((float)System.Math.Min(1f, 6 * _lightLevel / distance));
            }
            else
            {
                return color.Mutate(vx: v => 1f, sx: s => System.Math.Min(1, s * _lightLevel));
            }
        }

        public SectorInfo(
            Texture wallTexture,
            Texture floorTexture = null,
            Texture ceilingTexture = null,
            float lightLevel = 1f,
            bool passable = false)
            : this(
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
            Texture northTexture,
            Texture southTexture,
            Texture westTexture,
            Texture eastTexture,
            float lightLevel = 1f,
            bool passable = false,
            Texture floorTexture = null,
            Texture ceilingTexture = null)
        {
            _lightLevel = lightLevel;
            FloorTexture = floorTexture ?? Texture.Empty;
            CeilingTexture = ceilingTexture ?? Texture.Empty;
            _northTexture = northTexture ?? Texture.Empty;
            _southTexture = southTexture ?? Texture.Empty;
            _westTexture = westTexture ?? Texture.Empty;
            _eastTexture = eastTexture ?? Texture.Empty;

            Passable = passable;
        }

        public SectorInfo(
            Texture floorTexture,
            Texture ceilingTexture,
            float lightLevel,
            bool passable = true)
            : this(
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
