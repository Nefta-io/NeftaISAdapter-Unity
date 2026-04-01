using System;

namespace com.unity3d.mediation
{
    /// <summary>
    /// Defines the types of advertisement formats available in the LevelPlay SDK.
    /// </summary>
    [Obsolete("LevelPlayAdFormat has been deprecated and will be removed as part of version 9.0.0. Please transition to using the new Ad Unit APIs and refrain from passing adFormats to the Init function.")]
    public enum LevelPlayAdFormat
    {
        BANNER,
        INTERSTITIAL,
        REWARDED
    }
}

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Defines the types of advertisement formats available in the LevelPlay SDK.
    /// </summary>
    public enum LevelPlayAdFormat
    {
        BANNER,
        INTERSTITIAL,
        REWARDED
    }
}
