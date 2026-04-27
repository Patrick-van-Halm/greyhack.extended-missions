using System;
using System.Collections.Generic;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;

namespace ExtendedMissions.Registries
{
    public class UploadFileMission : ExtendedDirectMission<UploadFileMission, UploadFileMission.Data>
    {
        public class Data
        {
            public string Path { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        private const string CONDITION_SPECIFIC_USER_HOME_KEY = "SPECIFIC_USER";
        private const string CONDITION_SPECIFIC_PATH_KEY = "SPECIFIC_PATH";
        private const string FILE_NAME = "suspicious-file.so";
        
        protected override string Title => "Upload a file";
        protected override string Preview => "The client wants you to upload a file to a remote PC.";
        protected override string Mail => "The client wants you to upload a file to a remote PC.\n\nThe remote IP of the victim is $PUBLIC_IP and the LAN address is $LOCAL_IP.";
        protected override string Details => "The client wants you to upload a file to a remote PC. The required file is attached to the contract mail.";

        private string FileContent { get; set; } = string.Empty;

        public override FileSystem.Archivo? GetMailAttachment(ActiveMission mission) => GenerationUtils.CreateMailAttachment(FILE_NAME, FileContent, true);

        protected override int MinReputation => 0;
        override protected int MaxReputation => 0;

        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;
        protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
        {
            [CONDITION_SPECIFIC_USER_HOME_KEY] = "Client wants you to upload the file to the home directory of $TARGET.",
            [CONDITION_SPECIFIC_PATH_KEY] = "Client wants you to upload the file to $TARGET."
        };

        public UploadFileMission()
        {
            FileContent = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        protected override PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            DebugLogger.Log("[UploadFileMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var router = SpawnMissionRouter(mission, random);
            var computer = MissionUtils.GetRandomMissionComputer(router, false);
            string target = "";
            string targetPath = "";
            if(mission.condition.condition.ToExtendedConditionId() == GetConditionId(CONDITION_SPECIFIC_USER_HOME_KEY))
            {
                var user = MissionUtils.GetRandomUser(computer, random);
                target = user.nombreUsuario;
                targetPath = $"/home/{target}/{FILE_NAME}";
            }
            else if(mission.condition.condition.ToExtendedConditionId() == GetConditionId(CONDITION_SPECIFIC_PATH_KEY))
            {
                target = GetRandomPath(computer, random);
                targetPath = $"{target}{FILE_NAME}";
            }

            var text = BuildMissionText(
                mission,
                language,
                ("$PUBLIC_IP", router.GetPublicIP()!.BoldString()),
                ("$LOCAL_IP", computer.GetLocalIP()!.BoldString()),
                ("$TARGET", target.BoldString())
            );
            
            return new PreparedMission
            {
                TargetComputer = computer,
                KarmaType = KarmaSystem.KarmaType.GREY,
                MissionData = new Data()
                {
                    Path = targetPath,
                    Content = FileContent
                },
                Text = text
            };
        }

        private string GetRandomPath(Computer computer, Random random)
        {
            var dir = computer.GetFileSystem().GetCarpetaRaiz();
            var path = "/";
            while (true)
            {
                var folders = dir.GetCarpetas();
                if (folders.Count == 0 || random.NextDouble() < 0.5) return path;
                dir = folders[random.Next(folders.Count)];
                path += $"{dir.nombre}/";
            }
        }

        protected override bool ValidateMission(ActiveMission mission, string message, FileSystem.Archivo attachment)
        {
            var data = GetData(mission);
            var computer = ServerMap.Singleton.GetRemoteComputer(mission.targetComputerID);
            var file = computer.GetFileSystem().GetArchivo(data.Path);
            if (file == null) return false;
            if (!file.IsBinario()) return false;
            if (file.typeFile != FileSystem.Fichero.TypeFile.Generic) return false;
            if (file.GetContenido() != data.Content) return false;
            return true;
        }
    }
}
