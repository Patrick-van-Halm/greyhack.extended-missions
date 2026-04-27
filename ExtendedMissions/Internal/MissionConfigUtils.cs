using System.Collections.Generic;
using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using UnityEngine;

namespace ExtendedMissions.Utils
{
    internal static class MissionConfigUtils
    {
        private const byte MISSION_BUCKET_NO_MISSION_PERCENTAGE = 50;
        public static Dictionary<string, int>? DefaultIPGroupManagerMissionBuckets = null;

        public static void EnsureMissionBucketsLoaded()
        {
            if (DefaultIPGroupManagerMissionBuckets != null) return;
            if (IPGroupManager.MissionCounts == null || IPGroupManager.MissionCounts.Count == 0) return;
            DefaultIPGroupManagerMissionBuckets = new Dictionary<string, int>();
            DefaultIPGroupManagerMissionBuckets.AddRange(IPGroupManager.MissionCounts);
        }

        public static void UpdateMissionBuckets()
        {
            EnsureMissionBucketsLoaded();
            if (DefaultIPGroupManagerMissionBuckets == null) return;
            var hidden = DefaultIPGroupManagerMissionBuckets["HIDDEN"] + MissionRegistry.GetByTarget<IExtendedWorldMission>(MissionRegistrationTarget.Hidden).Count;
            IPGroupManager.MissionCounts["HIDDEN"] = hidden;
            var grey = DefaultIPGroupManagerMissionBuckets["GREY"] + MissionRegistry.GetByTarget<IExtendedWorldMission>(MissionRegistrationTarget.GreyHat).Count;
            IPGroupManager.MissionCounts["GREY"] = grey;
            var black = DefaultIPGroupManagerMissionBuckets["BLACK"] + MissionRegistry.GetByTarget<IExtendedWorldMission>(MissionRegistrationTarget.BlackHat).Count;
            IPGroupManager.MissionCounts["BLACK"] = black;
            var white = DefaultIPGroupManagerMissionBuckets["WHITE"] + MissionRegistry.GetByTarget<IExtendedWorldMission>(MissionRegistrationTarget.WhiteHat).Count;
            IPGroupManager.MissionCounts["WHITE"] = white;
            var procedural = DefaultIPGroupManagerMissionBuckets["PROCEDURAL"] + MissionRegistry.GetByTarget<IExtendedProceduralMission>(MissionRegistrationTarget.Procedural).Count;
            IPGroupManager.MissionCounts["PROCEDURAL"] = procedural;

            var percentile = MISSION_BUCKET_NO_MISSION_PERCENTAGE * 0.01f;
            IPGroupManager.MissionCounts["NOMISSION"] = Mathf.CeilToInt((hidden + grey + black + white + procedural) * percentile);
        }
    }
}