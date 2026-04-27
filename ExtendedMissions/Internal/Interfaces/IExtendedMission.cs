using ExtendedMissions.Registries;

namespace ExtendedMissions.Missions
{
    /// <summary>
    /// Common contract implemented by all public ExtendedMissions mission types.
    /// </summary>
    internal interface IExtendedMission
    {
        /// <summary>
        /// Gets the reserved mission type identifier used by the game and the mod.
        /// </summary>
        int MissionTypeId { get; }

        /// <summary>
        /// Gets the mission board or registration bucket that should expose this mission.
        /// </summary>
        MissionRegistrationTarget RegistrationTarget { get; }
    }
}
