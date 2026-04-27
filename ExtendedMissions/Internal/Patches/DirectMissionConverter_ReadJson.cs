using System;
using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using ExtendedMissions.Utils;
using HarmonyLib;
using JsonConverters;
using MissionConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExtendedMissions.Patches
{
    [HarmonyPatch(typeof(DirectMissionConverter), nameof(DirectMissionConverter.ReadJson), typeof(JsonReader), typeof(Type), typeof(object), typeof(JsonSerializer))]
    internal class DirectMissionConverter_ReadJson
    {
        static bool Prefix(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, DirectMissionConverter __instance, ref object? __result)
        {
            var jobject = JObject.Load(reader);
            var directMissionTypeId = (int?)jobject["typeMission"];
            if (directMissionTypeId == null)
            {
                DebugLogger.Log("[DirectMissionConverter] [ReadJson] [Prefix] Missing typeMission, returning null");
                __result = null;
                return false;
            }

            switch (directMissionTypeId.Value)
            {
                case (int)TypeMissionDirect.Tutorial:
                    __result = jobject.ToObject<TutorialMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.Credentials:
                    __result = jobject.ToObject<CredentialMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.AcademicRecord:
                    __result = jobject.ToObject<AcademicMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.PoliceRecord:
                    __result = jobject.ToObject<PoliceRecordMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.DestroyComputer:
                    __result = jobject.ToObject<DestroyComputerMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.StealFile:
                    __result = jobject.ToObject<StealFileMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.DeleteFile:
                    __result = jobject.ToObject<DeleteFileMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.FindHacker:
                    __result = jobject.ToObject<FindHackerMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.FindEvidence:
                    __result = jobject.ToObject<FindEvidenceMission>(serializer);
                    return false;

                case (int)TypeMissionDirect.Procedural:
                    __result = jobject.ToObject<ProceduralMission>(serializer);
                    return false;

                default:
                    DebugLogger.Log("[DirectMissionConverter] [ReadJson] [Prefix] Serializing Mission Types");
                    __result = null;
                    if (MissionRegistry.TryGet<IExtendedDirectMission>(directMissionTypeId.Value, out var directMission) && directMission != null)
                    {
                        __result = directMission.ReadMissionFromJson(directMissionTypeId.Value, jobject, serializer);
                    }
                    return false;
            }
        }
    }
}
