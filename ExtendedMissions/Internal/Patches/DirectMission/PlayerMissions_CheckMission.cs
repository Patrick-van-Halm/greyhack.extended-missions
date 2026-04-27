using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using ExtendedMissions.Utils;
using HarmonyLib;
using MissionConfig;

namespace ExtendedMissions.Patches
{
    [HarmonyPatch(
        typeof(PlayerMissions), 
        nameof(PlayerMissions.CheckMission)
    )]
    internal class PlayerMissions_CheckMission
    {
        static void Postfix(string message, FileSystem.Archivo attach, string idMission, ref bool missionOk, ref bool isTutorial, ref TypeMissionDirect typeMissionDirect, ref string __result, PlayerMissions __instance)
        {
            if (missionOk) return;
            if (!string.IsNullOrEmpty(__result)) return;

            DebugLogger.Log($"[PlayerMissions] [CheckMission] [Postfix] Check if mission was completed, not a base game mission");
            var activeMission = __instance.GetActiveMission(idMission);
            if (activeMission == null) return;

            var rawMissionTypeId = (int)activeMission.typeMission;
            if (MissionRegistry.TryGet<IExtendedDirectMission>(rawMissionTypeId, out var extendedMission) && extendedMission != null)
            {
                var text = extendedMission.CheckMission(activeMission, message, attach, string.Empty, ref missionOk);
                __result = text ?? string.Empty;
                return;
            }

            DebugLogger.Log($"[PlayerMissions] [CheckMission] [Postfix] Mission outside of mod");
        }
    }
}
