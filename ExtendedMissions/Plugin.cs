using System;
using System.Reflection;
using BepInEx;
#if BEPINEX_6
using BepInEx.Unity.Mono;
#endif
using ExtendedMissions.Registries;
using ExtendedMissions.Utils;
using HarmonyLib;

namespace ExtendedMissions
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions";
        public const string PluginName = "Extended Missions";
        public const string PluginVersion = "0.1.1";

        private static Action<string>? logMessage;
        private static bool initialized;

        internal static string? ConfigPath => Paths.ConfigPath;

        internal static void LogMessage(string text)
        {
            logMessage?.Invoke(text);
        }

        public static void Initialize()
        {
            if (initialized) return;

            if (string.IsNullOrWhiteSpace(ConfigPath))
            {
                throw new InvalidOperationException("Config path must be set before initialization.");
            }

            initialized = true;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            MissionRegistry.EnsureDiscovered();
            MissionConfigUtils.UpdateMissionBuckets();
        }

        public static void Shutdown()
        {
            initialized = false;
        }

        private void OnEnable()
        {
            logMessage = Logger.LogMessage;
            Initialize();
        }

        private void OnDisable()
        {
            Shutdown();
        }
    }
}
