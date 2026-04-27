using System.Collections.Generic;
using System.Linq;
using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using HarmonyLib;
using MissionConfig;
using Newtonsoft.Json;
using UnityEngine;

namespace ExtendedMissions.Patches
{
    [HarmonyPatch(typeof(XmlGlobal.CacheText), nameof(XmlGlobal.CacheText.Configure))]
    internal class XmlGlobal_CacheText_Configure
    {
        static void Postfix(List<TextAsset> origTextFiles, TextAsset origDefaultFile,
            List<TextAsset> origCommands, List<TextAsset> origExploitParts, List<TextAsset> origConfigGeneration,
            List<TextAsset> forceDefaultCommand)
        {
            Plugin.LogMessage("Initializing Mission Data");
            Plugin.LogMessage("> Reading Current Mission Data");
            var currentHackShopMissionConfig = ReadDirectMissionConfig("DirectMissions");
            var currentPoliceMissionConfig = ReadDirectMissionConfig("DirectMissionsPolice");
            var currentGreyHatMissionConfig = ReadWorldMissionConfig("MissionKarma");
            var currentBlackHatMissionConfig = ReadWorldMissionConfig("MissionKarmaBlack");
            var currentWhiteHatMissionConfig = ReadWorldMissionConfig("MissionKarmaWhite");
            var currentHiddenMissionConfig = ReadWorldMissionConfig("MissionHidden");
            var currentProceduralMissionConfig = ReadProceduralMissionConfig("MissionProcedural");

            currentHackShopMissionConfig.AddRange(GetDirectMissionConfigs(DirectMissionTarget.Hackshop));
            currentPoliceMissionConfig.AddRange(GetDirectMissionConfigs(DirectMissionTarget.Police));
            currentGreyHatMissionConfig.AddRange(GetWorldMissionConfigs(WorldMissionTarget.Grey));
            currentBlackHatMissionConfig.AddRange(GetWorldMissionConfigs(WorldMissionTarget.Black));
            currentWhiteHatMissionConfig.AddRange(GetWorldMissionConfigs(WorldMissionTarget.White));
            currentHiddenMissionConfig.AddRange(GetWorldMissionConfigs(WorldMissionTarget.Hidden));
            currentProceduralMissionConfig.archetypes.AddRange(GetProceduralMissionConfigs());

            Plugin.LogMessage("< Writing Mission Data to Game");
            XmlGlobal.CacheText.configGeneration["DirectMissions"] = JsonConvert.SerializeObject(currentHackShopMissionConfig, Formatting.Indented);
            XmlGlobal.CacheText.configGeneration["DirectMissionsPolice"] = JsonConvert.SerializeObject(currentPoliceMissionConfig, Formatting.Indented);
            XmlGlobal.CacheText.configGeneration["MissionKarma"] = JsonConvert.SerializeObject(currentGreyHatMissionConfig, Formatting.Indented);
            XmlGlobal.CacheText.configGeneration["MissionKarmaBlack"] = JsonConvert.SerializeObject(currentBlackHatMissionConfig, Formatting.Indented);
            XmlGlobal.CacheText.configGeneration["MissionKarmaWhite"] = JsonConvert.SerializeObject(currentWhiteHatMissionConfig, Formatting.Indented);
            XmlGlobal.CacheText.configGeneration["MissionHidden"] = JsonConvert.SerializeObject(currentHiddenMissionConfig, Formatting.Indented);
            XmlGlobal.CacheText.configGeneration["MissionProcedural"] = JsonConvert.SerializeObject(currentProceduralMissionConfig, Formatting.Indented);

            Plugin.LogMessage("Initializing Language Data");
            Plugin.LogMessage("> Reading Current Language Data [ENG]");
            var englishLanguageConfig = ReadConfig<TranslationManager.ContentTexts>("GameTexts_en");

            foreach (var mission in MissionRegistry.GetAll<ITranslatableMission>())
            {
                mission.RegisterTranslations(englishLanguageConfig.content);
            }

            Plugin.LogMessage("< Writing Language Data to Game [ENG]");
            XmlGlobal.CacheText.configGeneration["GameTexts_en"] = JsonConvert.SerializeObject(englishLanguageConfig, Formatting.Indented);
        }

        private static IEnumerable<DirectMissionPreview> GetDirectMissionConfigs(DirectMissionTarget target)
        {
            return MissionRegistry.GetByTarget<IExtendedDirectMission>((MissionRegistrationTarget)target)
                .Select(mission => mission.MissionConfig());
        }

        private static IEnumerable<Mission> GetWorldMissionConfigs(WorldMissionTarget target)
        {
            return MissionRegistry.GetByTarget<IExtendedWorldMission>((MissionRegistrationTarget)target)
                .Select(mission => mission.MissionConfig())
                .Where(mission => mission != null)
                .Select(mission => mission!);
        }

        private static IEnumerable<ArchetypeConfig> GetProceduralMissionConfigs()
        {
            return MissionRegistry.GetByTarget<IExtendedProceduralMission>(MissionRegistrationTarget.Procedural)
                .Select(mission => mission.ProceduralMissionConfig())
                .Where(mission => mission != null)
                .Select(mission => mission!);
        }

        private static List<DirectMissionPreview> ReadDirectMissionConfig(string key)
        {
            return ReadConfig<List<DirectMissionPreview>>(key);
        }

        private static List<Mission> ReadWorldMissionConfig(string key)
        {
            return ReadConfig<List<Mission>>(key);
        }

        private static ProcMissionsConfig ReadProceduralMissionConfig(string key)
        {
            return ReadConfig<ProcMissionsConfig>(key);
        }

        private static TCollection ReadConfig<TCollection>(string key)
            where TCollection : class, new()
        {
            if (!XmlGlobal.CacheText.configGeneration.TryGetValue(key, out var serializedConfig))
            {
                return new TCollection();
            }

            return JsonConvert.DeserializeObject<TCollection>(serializedConfig) ?? new TCollection();
        }
    }
}
