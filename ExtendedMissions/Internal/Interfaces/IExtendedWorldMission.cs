using MissionConfig;

namespace ExtendedMissions.Missions
{
    internal interface IExtendedWorldMission : IExtendedMission, ITranslatableMission
    {
        Mission? MissionConfig();
    }
}
