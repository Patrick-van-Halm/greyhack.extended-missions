using BepInEx;

namespace ExtendedMissions.CustomMissions.WebsiteDefacement
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class WebsiteDefacementPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions.website-defacement";
        public const string PluginName = "Extended Missions - Website Defacement";
        public const string PluginVersion = "0.1.0";
    }
}
