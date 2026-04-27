using MissionConfig;

namespace ExtendedMissions.Missions
{
    internal interface IExtendedProceduralMission : ITranslatableMission, IExtendedMission
    {
        public int ArcheTypeId { get; }

        public string ArcheTypeTextId { get; }

        public string ArcheTypeDetailTextId { get; }

        ArchetypeConfig ProceduralMissionConfig();
    }
}
