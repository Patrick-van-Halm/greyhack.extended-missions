using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using HarmonyLib;
using MissionConfig;

namespace ExtendedMissions.Patches
{
    [HarmonyPatch(typeof(PlayerMissions), nameof(PlayerMissions.AddMission), typeof(DirectMission), typeof(PlayerComputer))]
    internal class PlayerMissions_AddMission
    {
        static bool Prefix(DirectMission mission, PlayerComputer playerComputer, ref string __result, PlayerMissions __instance)
        {
            if (mission == null) return true;
            var rawMissionTypeId = (int)mission.missionType;
            if (rawMissionTypeId < MissionTypeRegistry.Instance.RangeStart) return true;
            if (!MissionRegistry.TryGet<IExtendedDirectMission>(rawMissionTypeId, out var extendedMission) || extendedMission == null) return true;

            var language = playerComputer.GetLanguage();
            var text = extendedMission.AddMission(mission, language, __instance);
            if (string.IsNullOrEmpty(text)) return false;
            
            __result = text;
            Database.Singleton.SyncMissionsPlayer(__instance, playerComputer);
            return false;
        }
    }
}
