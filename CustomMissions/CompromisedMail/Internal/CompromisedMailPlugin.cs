using BepInEx;
#if BEPINEX_6
using BepInEx.Unity.Mono;
#endif

namespace ExtendedMissions.CustomMissions.CompromisedMail
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class CompromisedMailPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions.compromised-mail";
        public const string PluginName = "Extended Missions - Compromised Mail";
        public const string PluginVersion = "0.1.0";

        private void Awake() { }
    }
}
