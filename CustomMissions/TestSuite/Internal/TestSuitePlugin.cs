using BepInEx;
#if BEPINEX_6
using BepInEx.Unity.Mono;
#endif

namespace ExtendedMissions.CustomMissions.TestSuite
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class TestSuitePlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions.test-suite";
        public const string PluginName = "Extended Missions - Testing Suite";
        public const string PluginVersion = "0.1.0";
    }
}
