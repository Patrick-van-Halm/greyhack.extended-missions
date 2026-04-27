using System.Collections.Generic;
using CompressString;
using ExtendedMissions.Missions;
using ExtendedMissions.Registries;
using ExtendedMissions.Texts;
using ExtendedMissions.Utils;
using HarmonyLib;
using MailConfig;
using MissionConfig;
using NetworkMessages;
using Newtonsoft.Json;

namespace ExtendedMissions.Patches
{
    [HarmonyPatch(typeof(MissionsHelperServer), nameof(MissionsHelperServer.AcceptMissionServerRpc), typeof(string), typeof(string))]
    internal class MissionHelperServer_AcceptMissionServerRpc
    {
        public static bool Prefix(string missionID, string shopNetID, MissionsHelperServer __instance)
        {
            DebugLogger.Log($"[PlayerMissions] [AcceptMissionServerRPC] [Prefix] Check for custom accept mission logic");

            PlayerComputer computer = __instance.player.GetComputer();
            string errorText = MissionTexts.MissionError(string.Empty);
            Computer remoteComputer = ServerMap.Singleton.GetRemoteComputer(shopNetID, true, null);
            if (remoteComputer == null || !__instance.IsMissionShop(remoteComputer))
            {
                __instance.CloseConnection(-1, errorText, false);
                return false;
            }
            string missionBoardAdminMailAddress = remoteComputer.GetAdmin().GetUserMail().address;
            bool forceMinRep = computer.IsForceMinRep();
            List<DirectMission> orCreateDirectMissions = __instance.GetOrCreateDirectMissions(remoteComputer, forceMinRep);
            DirectMission directMission = DirectMission.DeleteMission(missionID, orCreateDirectMissions);
            if (directMission == null)
            {
                __instance.CloseConnection(-1, errorText, false);
                return false;
            }

            HelperAccount playerMailAccount = computer.GetPlayerConfigOS().GetMailAccount();
            if (playerMailAccount == null)
            {
                __instance.CloseConnection(-1, "You must register an email account in order to accept a mission.", false);
                return false;
            }
            
            UserMail playerMailBox = Database.Singleton.GetMailAccount(playerMailAccount.userName, out errorText, null, true);
            if (!string.IsNullOrEmpty(errorText))
            {
                __instance.CloseConnection(-1, "Unable to accept mission: " + errorText, false);
                return false;
            }

            if (directMission.isProceduralOffer)
            {
                // Procedural missions logic accepting
                __instance.dmissions.AddProceduralMission(directMission, computer);
                __instance.SendProceduralStarterMail(directMission, playerMailBox, missionBoardAdminMailAddress);
                return false;
            }
            
            var mailText = __instance.dmissions.AddMission(directMission, computer);
            string id = directMission.ID;
            FileSystem.Archivo? attachment = null;

            if (MissionRegistry.TryGet<IExtendedDirectMission>(directMission.missionType.ToMissionTypeId(), out var extendedMission) && extendedMission != null)
            {
                var activeMission = __instance.GetPlayerMissions().GetActiveMission(id);
                attachment = extendedMission.GetMailAttachment(activeMission);
            }

            playerMailBox.RecibirMail("Mission Contract", mailText, missionBoardAdminMailAddress, id, "", attachment, true, false, false);
            byte[] array = StringCompressor.Zip(JsonConvert.SerializeObject(playerMailBox));
            MessageClient messageClient = new MessageClient(IdClient.PlayerRecibeMailClientRpc);
            messageClient.AddString(new List<string> { missionBoardAdminMailAddress, "Mission Contract" });
            messageClient.AddByte(array);
            __instance.player.SendData(messageClient);
            return false;
        }
    }
}
