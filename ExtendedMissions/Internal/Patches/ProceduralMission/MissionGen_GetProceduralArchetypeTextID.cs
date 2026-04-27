using System.Linq;
using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using HarmonyLib;
using MissionConfig;

namespace ExtendedMissions.Patches
{
    [HarmonyPatch(
        typeof(MissionGen), 
        nameof(MissionGen.GetProceduralArchetypeTextID)
    )]
    internal class MissionGen_GetProceduralArchetypeTextID
    {
        static bool Prefix(Archetype archetype, ref string __result)
        {
            var rawArcheType = (int)archetype;
            if (rawArcheType < ProceduralArchetypesRegistry.Instance.RangeStart) return true;
            
            var proceduralMission = MissionRegistry.GetByTarget<IExtendedProceduralMission>(MissionRegistrationTarget.Procedural).FirstOrDefault(p => p.ArcheTypeId == rawArcheType);
            if (proceduralMission == null) return true;

            __result = proceduralMission.ArcheTypeTextId;
            return false;
        }
    }
}
