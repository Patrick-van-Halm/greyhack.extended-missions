using System;
using System.Collections.Generic;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;
using Random = System.Random;

namespace ExtendedMissions.Registries
{
    public class ChangePasswordMission : ExtendedDirectMission<ChangePasswordMission, ChangePasswordMission.Data>
    {
        public class Data
        {
            public string TargetUser { get; set; } = string.Empty;
            public string OriginalPassword { get; set; } = string.Empty;
            public string? RequiredPassword { get; set; } = null;
        }

        private const string CONDITION_RECOVERY_EXACT_KEY = "RECOVERY_EXACT";
        private const string CONDITION_RECOVERY_ANY_KEY = "RECOVERY_ANY";
        private const string CONDITION_SABOTAGE_EXACT_KEY = "SABOTAGE_EXACT";
        private const string CONDITION_ANY_CHANGE_KEY = "ANY_CHANGE";

        protected override string Title => "Change a password";
        protected override string Preview => "Alter a target account by setting a new required password.";
        protected override string Details => "Different contracts may require an exact password reset or simply locking the user out by changing the password.";
        protected override string Mail => "The client needs a password changed on a remote workstation.\n\nThe target public IP is $PUBLIC_IP and the LAN address is $LOCAL_IP.\nThe target username is $USERNAME.";

        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;
        override protected int MinReputation => 1;
        override protected int MaxReputation => 1;

        protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
        {
            [CONDITION_RECOVERY_EXACT_KEY] = "This is an account recovery job. Change the password to exactly $NEW_PASSWORD.",
            [CONDITION_RECOVERY_ANY_KEY] = "This is an account recovery job. Change the password to any new value. And return the new password to the client.",
            [CONDITION_SABOTAGE_EXACT_KEY] = "The client wants the user locked out. Change the password to exactly $NEW_PASSWORD.",
            [CONDITION_ANY_CHANGE_KEY] = "The client only cares that the current password stops working. Change the password to any new value."
        };

        protected override PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            DebugLogger.Log("[ChangePasswordMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var router = SpawnMissionRouter(mission, random);
            var computer = MissionUtils.GetRandomMissionComputer(router, mission.rep != 1);
            var targetUser = MissionUtils.GetRandomUser(computer, random);
            var originalPassword = targetUser.GetPassPlano();
            var conditionId = mission.condition.condition.ToExtendedConditionId();
            var exactPasswordRequired = conditionId == GetConditionId(CONDITION_RECOVERY_EXACT_KEY) || conditionId == GetConditionId(CONDITION_SABOTAGE_EXACT_KEY);
            var requiredPassword = exactPasswordRequired ? GenerateRequiredPassword(random, originalPassword) : string.Empty;
            var karma = conditionId switch
            {
                _ when conditionId == GetConditionId(CONDITION_RECOVERY_EXACT_KEY) => KarmaSystem.KarmaType.WHITE,
                _ when conditionId == GetConditionId(CONDITION_RECOVERY_ANY_KEY) => KarmaSystem.KarmaType.WHITE,
                _ => KarmaSystem.KarmaType.BLACK
            };
            
            var text = BuildMissionText(
                mission,
                language,
                ("$PUBLIC_IP", router.GetPublicIP()!.BoldString()),
                ("$LOCAL_IP", computer.GetLocalIP()!.BoldString()),
                ("$USERNAME", targetUser.nombreUsuario.BoldString()),
                ("$NEW_PASSWORD", requiredPassword.BoldString())
            );

            return new PreparedMission
            {
                TargetComputer = computer,
                KarmaType = karma,
                MissionData = new Data
                {
                    TargetUser = targetUser.nombreUsuario,
                    OriginalPassword = originalPassword,
                    RequiredPassword = exactPasswordRequired ? requiredPassword : null,
                }
            };
        }

        protected override bool ValidateMission(ActiveMission mission, string message, FileSystem.Archivo attachment)
        {
            var data = GetData(mission);
            var computer = ServerMap.Singleton.GetRemoteComputer(mission.targetComputerID);
            if (computer == null) return false;

            var user = computer.GetUser(data.TargetUser);
            if (user == null) return false;
            if (!computer.IsFilePasswordOk()) return false;

            if (data.RequiredPassword != null &&
                user.GetPassPlano().Equals(data.RequiredPassword, StringComparison.Ordinal))
            {
                return true;
            }

            return !user.GetPassPlano().Equals(data.OriginalPassword, StringComparison.Ordinal);
        }
        
        private static string GenerateRequiredPassword(Random random, string originalPassword)
        {
            string candidate;
            do
            {
                candidate = WordGenerator.GetNextWord(WordGenerator.word.passwords, random) + random.Next(10, 100);
            } while (candidate.Equals(originalPassword, StringComparison.Ordinal));

            return candidate;
        }
    }
}
