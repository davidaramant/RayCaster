namespace RayCasterGame
{
    /// <summary>
    /// Constants related to the light levels.
    /// </summary>
    static class LightLevels
    {
        public const int NumberOfLightLevelsPower = 4;
        public const int NumberOfLevels = 1 << NumberOfLightLevelsPower;
        public const int FullBrightIndex = (int)(0.8 * NumberOfLevels);

        public const float MinValue = 0.1f;
        public const float MaxOverbright = 2f;
    }
}
