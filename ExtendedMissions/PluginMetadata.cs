using System;
using System.Reflection;
using HarmonyLib;
using ExtendedMissions.Registries;
using ExtendedMissions.Utils;

namespace ExtendedMissions
{
    public static class Plugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions";
        public const string PluginName = "Extended Missions";
        public const string PluginVersion = "0.1.0";

        private static Action<string>? logMessage;
        private static bool initialized;

        public static void SetLogger(Action<string> logger)
        {
            logMessage = logger;
        }

        internal static void LogMessage(string text)
        {
            logMessage?.Invoke(text);
        }

        public static void Initialize()
        {
            if (initialized) return;

            initialized = true;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            MissionRegistry.EnsureDiscovered();
            MissionConfigUtils.UpdateMissionBuckets();
        }

        public static void Shutdown()
        {
            initialized = false;
        }
    }
}
