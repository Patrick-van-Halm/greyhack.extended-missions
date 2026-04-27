using System;

namespace ExtendedMissions.Utils
{
    /// <summary>
    /// Helpers for creating random number generators used by mission generation.
    /// </summary>
    public static class RandomUtils {
        /// <summary>
        /// Creates a new <see cref="Random"/> instance with a GUID-derived seed.
        /// </summary>
        /// <returns>A new random number generator.</returns>
        public static Random CreateRandom()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }
    }
}