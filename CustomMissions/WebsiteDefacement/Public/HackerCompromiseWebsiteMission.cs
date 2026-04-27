using System.Collections.Generic;
using System.Linq;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;
using ServiceConfig;
using Random = System.Random;

namespace ExtendedMissions.Registries
{
    internal class HackerCompromiseWebsiteMission : ExtendedDirectMission<HackerCompromiseWebsiteMission, HackerCompromiseWebsiteMission.Data>
    {
        private const string ByDomainConditionTag = "BY_DOMAIN";
        private int MissionConditionWebsiteByDomain => GetConditionId(ByDomainConditionTag);
        private const string ByIpConditionTag = "BY_IP";

        internal class Data { }

        protected override string Title => "Compromise a Target Website";
        protected override string Preview => "Compromise a target website by replacing its content with a hacker notice.";
        protected override string Mail => "The client wants to compromise a website.\nOperate on /Public/htdocs/website.html.";
        protected override string Details => "The client wants to compromise a website by replacing its content with a message from the Gecko Crew. The required HTML page is attached to the contract mail.";
        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;
        protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
        {
            [ByDomainConditionTag] = "Target the website identified by domain $TARGET.",
            [ByIpConditionTag] = "Target the website hosted at public IP $TARGET."
        };
        
        public override FileSystem.Archivo? GetMailAttachment(ActiveMission mission)
        {
            var file = new FileSystem.Archivo("website.html", HTML, "Unknown", FileSystem.Fichero.TypeFile.Generic, true, true);
            file.SetBinario(false);
            return file;
        }

        protected override PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            DebugLogger.Log("[HackerCompromiseWebsiteMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var webserverTypes = XmlGlobal.Singleton.tipoServidor.publicServices.Where(s => s.Value.Any(service => service.ID == ServicioID.http));
            var tipoRed = webserverTypes.ElementAt(random.Next(webserverTypes.Count())).Key;
            var router = ServerMap.Singleton.SpawnRouter(tipoRed, mission.GetAccessType(), null, false, "");
            var targetComputer = GetHttpTarget(router, random);
            if (targetComputer == null) return null;

            var useDomainInContract = mission.condition.condition.ToExtendedConditionId() == MissionConditionWebsiteByDomain;
            var targetRef = useDomainInContract ? router.GetDomain() : router.GetPublicIP();

            var text = BuildMissionText(
                mission,
                language,
                ("$TARGET", targetRef.BoldString())
            );
                
            return new PreparedMission
            {
                TargetComputer = targetComputer,
                KarmaType = KarmaSystem.KarmaType.BLACK,
                MissionData = new Data(),
                Text = text
            };
        }

        protected override bool ValidateMission(ActiveMission activeMission, string message, FileSystem.Archivo attachment)
        {
            var computer = ServerMap.Singleton.GetRemoteComputer(activeMission.targetComputerID);
            if (computer == null) return false;
            if (!computer.ExisteServicio(ServicioID.http)) return false;

            var file = computer.GetFileSystem().GetArchivo("/Public/htdocs/website.html");
            if (file == null || file.IsBinario()) return false;

            var content = file.GetContenido();
            return content == HTML;
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

        private const string HTML = @"<!doctype html>
        <html>
        <head>
        <meta charset=""utf-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
        <title>System Breach Notice</title>
        <style type=""text/css"">
            html {
            width: 100%;
            height: 100%;
            margin: 0;
            overflow: hidden;
            background: #050805;
            }

            body {
            width: 100%;
            height: 100%;
            margin: 0;
            overflow: hidden;
            color: #7CFFB2;
            font: 13px ""Courier New"", monospace;
            }

            .scroll-root {
            width: 100%;
            height: 100%;
            overflow-y: auto;
            overflow-x: hidden;
            box-sizing: border-box;
            padding: 20px 0;
            }

            .page {
            width: 860px;
            max-width: 95%;
            margin: 0 auto 18px auto;
            border: 1px solid #1f5f3f;
            background: #020403;
            box-shadow: 0 0 20px rgba(0,255,150,0.15);
            }

            .shell {
            margin: 10px;
            border: 1px solid #2c8f63;
            background: #040807;
            }

            .top {
            padding: 8px 12px;
            border-bottom: 1px solid #1f5f3f;
            background: #06120d;
            color: #63ff9e;
            font-size: 11px;
            letter-spacing: 1px;
            }

            .warning {
            margin: 12px;
            padding: 6px;
            border: 1px solid #2c8f63;
            text-align: center;
            font-size: 11px;
            }

            .hero {
            margin: 12px;
            padding: 14px;
            border: 1px solid #2c8f63;
            text-align: center;
            }

            .hero img {
            display: block;
            margin: 0 auto 10px auto;
            border: 1px solid #2c8f63;
            padding: 4px;
            }

            .headline {
            margin: 0;
            font-size: 20px;
            text-transform: uppercase;
            }

            .subhead {
            margin: 6px 0;
            font-size: 11px;
            color: #55d98f;
            }

            .hero-copy {
            margin: 8px 0 0 0;
            font-size: 12px;
            line-height: 1.4;
            }

            .section-title {
            margin: 0 12px 6px 12px;
            font-size: 11px;
            color: #55d98f;
            }

            .panel {
            margin: 0 12px 12px 12px;
            border: 1px solid #1f5f3f;
            background: #010201;
            padding: 10px;
            }

            .line {
            margin: 0 0 6px 0;
            font-size: 12px;
            }

            .terminal {
            margin: 0 12px 12px 12px;
            border: 1px solid #1f5f3f;
            background: #000000;
            padding: 10px;
            font-size: 11px;
            color: #8affc1;
            white-space: pre-wrap;
            line-height: 1.4;
            }

            .footer {
            margin: 0 12px 12px 12px;
            border-top: 1px solid #1f5f3f;
            padding-top: 8px;
            text-align: center;
            font-size: 10px;
            color: #4ed488;
            }
        </style>
        </head>
        <body>
        <div class=""scroll-root"">
            <div class=""page"">
            <div class=""shell"">
                <div class=""top"">[REMOTE ACCESS TERMINAL] // SESSION ACTIVE</div>

                <div class=""warning"">!!! SYSTEM COMPROMISED !!!</div>

                <div class=""hero"">
                <img src=""gecko.png"" width=""84"" height=""84"">
                <p class=""headline"">Hacked by Gecko Crew</p>
                <p class=""subhead"">-- WE WERE HERE --</p>
                <p class=""hero-copy"">Root access obtained. Surface replaced. Signal delivered. Your defenses failed under minimal resistance.</p>
                </div>

                <p class=""section-title"">BREACH NOTES</p>
                <div class=""panel"">
                <p class=""line"">- Unauthorized access gained via weak entry point.</p>
                <p class=""line"">- Frontend replaced for visibility.</p>
                <p class=""line"">- System integrity cannot be trusted.</p>
                </div>

                <p class=""section-title"">EXPOSED LOG SNIPPET</p>
                <div class=""terminal"">
        [22:14:03] connection accepted :: 192.168.0.42
        [22:14:05] auth bypass :: success
        [22:14:07] privilege escalation :: root
        [22:14:12] reading /config/admin.json
        [22:14:15] dumping credentials... done
        [22:14:18] injecting payload :: index.html
        [22:14:21] overwrite complete
        [22:14:22] session closed
                </div>

                <div class=""footer"">gecko crew // persistence is temporary</div>
            </div>
            </div>
        </div>
        </body>
        </html>";
    }
}
