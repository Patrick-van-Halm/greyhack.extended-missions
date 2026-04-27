using System.Collections.Generic;
using System.Linq;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;
using ServiceConfig;
using Random = System.Random;

namespace ExtendedMissions.Registries
{
    internal class TrollingWebsiteMission : ExtendedDirectMission<TrollingWebsiteMission, TrollingWebsiteMission.Data>
    {
        private const string ByDomainConditionTag = "BY_DOMAIN";
        private int MissionConditionWebsiteByDomain => GetConditionId(ByDomainConditionTag);
        private const string ByIpConditionTag = "BY_IP";

        internal class Data
        {
            public int TextID { get; set; } = -1;
        }

        protected override string Title => "Website Trolling";
        protected override string Preview => "Troll a target website by appending the content with a humorous message.";
        protected override string Mail => @"The client wants to troll a website.\nOperate on /Public/htdocs/website.html.\nAppend the following message: ""$MESSAGE""";
        protected override string Details => "The client wants to troll a website by appending its content with a humorous message. The required message is attached to the contract mail.";
        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;
        protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
        {
            [ByDomainConditionTag] = "Target the website identified by domain $TARGET.",
            [ByIpConditionTag] = "Target the website hosted at public IP $TARGET."
        };

        protected override PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            DebugLogger.Log("[PoliceSeizeWebsiteMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var webserverTypes = XmlGlobal.Singleton.tipoServidor.publicServices.Where(s => s.Value.Any(service => service.ID == ServicioID.http));
            var tipoRed = webserverTypes.ElementAt(random.Next(webserverTypes.Count())).Key;
            var router = ServerMap.Singleton.SpawnRouter(tipoRed, mission.GetAccessType(), null, false, "");
            var targetComputer = GetHttpTarget(router, random);
            if (targetComputer == null) return null;

            var useDomainInContract = mission.condition.condition.ToExtendedConditionId() == MissionConditionWebsiteByDomain;
            var targetRef = useDomainInContract ? router.GetDomain() : router.GetPublicIP();
            var messageId = random.Next(Messages.Length);

            var text = BuildMissionText(
                mission,
                language,
                ("$TARGET", targetRef.BoldString()),
                ("$MESSAGE", Messages[messageId].BoldString())
            );

                
            return new PreparedMission
            {
                TargetComputer = targetComputer,
                KarmaType = KarmaSystem.KarmaType.WHITE,
                MissionData = new Data()
                {
                    TextID = messageId
                },
                Text = text
            };
        }

        protected override bool ValidateMission(ActiveMission activeMission, string message, FileSystem.Archivo attachment)
        {
            var data = GetData(activeMission);
            if (data == null) return false;
            var computer = ServerMap.Singleton.GetRemoteComputer(activeMission.targetComputerID);
            if (computer == null) return false;
            if (!computer.ExisteServicio(ServicioID.http)) return false;

            var file = computer.GetFileSystem().GetArchivo("/Public/htdocs/website.html");
            if (file == null || file.IsBinario()) return false;

            var content = file.GetContenido();
            return content.Contains(Messages[data.TextID]);
        }

        private static Computer? GetHttpTarget(Router router, Random random)
        {
            var candidates = new List<Computer>();
            if (router.ExisteServicio(ServicioID.http))
            {
                candidates.Add(router);
            }

            candidates.AddRange(router.GetComputers(true)
                .Where(computer => computer != null && computer.ExisteServicio(ServicioID.http)));
            if (candidates.Count == 0) return null;
            return candidates[random.Next(candidates.Count)];
        }

        private string[] Messages = new[]
        {
            "This website has been trolled by a mysterious hacker. All your base are belong to us.",
            "Congratulations! You've been visited by the troll of the internet. Share this with 10 friends or your website will be forever trolled.",
            "This website is now under new management. Please direct all complaints to the new owner: The Internet Troll.",
            "You've been trolled! Don't worry, it's just a prank bro.",
            "This website has been upgraded with premium troll content. Enjoy the enhanced trolling experience!"
        };
    }
}
