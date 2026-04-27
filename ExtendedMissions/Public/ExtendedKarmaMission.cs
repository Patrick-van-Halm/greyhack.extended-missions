using ExtendedMissions.Missions;

namespace ExtendedMissions.Registries
{
    /// <summary>
    /// Describes the karma custom world mission should register under.
    /// </summary>
    public enum KarmaMissionTarget
    {
        /// <summary>Register the mission as a black hat mission.</summary>
        Black = MissionRegistrationTarget.BlackHat,
        /// <summary>Register the mission as a white hat mission.</summary>
        White = MissionRegistrationTarget.WhiteHat,
        /// <summary>Register the mission as a grey hat mission.</summary>
        Grey = MissionRegistrationTarget.GreyHat,
    }

    /// <summary>
    /// Base class for world missions registered on as a karma mission.
    /// </summary>
    /// <typeparam name="TInstance">The concrete singleton mission type.</typeparam>
    public abstract class ExtendedKarmaMission<TInstance> : ExtendedWorldMission<TInstance>, IExtendedWorldMission
        where TInstance : ExtendedKarmaMission<TInstance>, new()
    {
        /// <summary>
        /// Gets the karma target the mission registers with.
        /// </summary>
        public abstract KarmaMissionTarget MissionTarget { get; }

        /// <inheritdoc />
        public override MissionRegistrationTarget RegistrationTarget => (MissionRegistrationTarget)MissionTarget;
    }
}
