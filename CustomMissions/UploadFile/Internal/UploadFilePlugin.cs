using BepInEx;
#if BEPINEX_6
using BepInEx.Unity.Mono;
#endif

namespace ExtendedMissions.CustomMissions.UploadFile
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class UploadFilePlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions.upload-file";
        public const string PluginName = "Extended Missions - Upload File";
        public const string PluginVersion = "0.1.0";
    }
}
