using BepInEx;
#if BEPINEX_6
using BepInEx.Unity.Mono;
#endif

namespace ExtendedMissions.CustomMissions.ChangePassword
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class ChangePasswordPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions.change-password";
        public const string PluginName = "Extended Missions - Change Password";
        public const string PluginVersion = "0.1.0";

        private void Awake() { }
    }
}
