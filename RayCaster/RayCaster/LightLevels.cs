namespace RayCasterGame
{
    /// <summary>
    /// Constants related to the light levels.
    /// </summary>
    static class LightLevels
    {
        public const int NumberOfLevels = 12;
        public const int FullBrightIndex = 9;
        public static readonly float[] SaturationFactors =
            new[] { 0.5f, 0.7f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.2f, 1.4f };
        public static readonly float[] ValueFactors = 
            new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.4f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.2f, 1.4f };
    }
}
