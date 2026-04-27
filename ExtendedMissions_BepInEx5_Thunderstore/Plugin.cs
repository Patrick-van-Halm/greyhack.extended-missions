using BepInEx;
using CorePlugin = global::ExtendedMissions.Plugin;

namespace ExtendedMissions
{
    [BepInPlugin(CorePlugin.PluginGuid, CorePlugin.PluginName, CorePlugin.PluginVersion)]
    public class BepInExPlugin : BaseUnityPlugin
    {
        private void OnEnable()
        {
            CorePlugin.SetLogger(base.Logger.LogMessage);
            CorePlugin.Initialize();
        }

        private void OnDisable()
        {
            CorePlugin.Shutdown();
        }
    }
}
