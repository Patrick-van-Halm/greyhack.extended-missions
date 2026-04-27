using ExtendedMissions.Registries;

namespace ExtendedMissions.Missions
{
    /// <summary>
    /// Base class for hidden world missions.
    /// </summary>
    /// <typeparam name="TInstance">The concrete singleton mission type.</typeparam>
    public abstract class ExtendedHiddenMission<TInstance> : ExtendedWorldMission<TInstance>, IExtendedWorldMission
        where TInstance : ExtendedHiddenMission<TInstance>, new()
    {
        /// <inheritdoc />
        public override MissionRegistrationTarget RegistrationTarget => MissionRegistrationTarget.Hidden;
    }
}
