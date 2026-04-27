using System.Reflection;
using BepInEx;
#if BEPINEX_6
using BepInEx.Unity.Mono;
#endif
using BepInEx.Logging;
using HarmonyLib;

namespace ExtendedMissions.CustomMissions.SendMoney
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class SendMoneyPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nl.pvanhalm.plugins.greyhack.extended-missions.send-money";
        public const string PluginName = "Extended Missions - Send Money";
        public const string PluginVersion = "0.1.0";
        internal new static ManualLogSource? Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
