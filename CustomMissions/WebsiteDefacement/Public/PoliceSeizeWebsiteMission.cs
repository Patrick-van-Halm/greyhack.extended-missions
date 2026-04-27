using System.Collections.Generic;
using System.Linq;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;
using ServiceConfig;
using Random = System.Random;

namespace ExtendedMissions.Registries
{
    internal class PoliceSeizeWebsiteMission : ExtendedDirectMission<PoliceSeizeWebsiteMission, PoliceSeizeWebsiteMission.Data>
    {
        private const string ByDomainConditionTag = "BY_DOMAIN";
        private int MissionConditionWebsiteByDomain => GetConditionId(ByDomainConditionTag);
        private const string ByIpConditionTag = "BY_IP";

        internal class Data { }

        protected override string Title => "Website Seizure Notice";
        protected override string Preview => "Seize a target website by replacing its content with a police notice.";
        protected override string Mail => "The client wants to seize a website.\nOperate on /Public/htdocs/website.html.";
        protected override string Details => "The client wants to seize a website by replacing its content with a police notice. The required HTML page is attached to the contract mail.";
        protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Police;
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
            DebugLogger.Log("[PoliceSeizeWebsiteMission] Add Mission to User");
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
                KarmaType = KarmaSystem.KarmaType.WHITE,
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
                <title>Police Seizure Notice</title>
                <style type=""text/css"">
                    html {
                    width: 100%;
                    height: 100%;
                    margin: 0;
                    overflow: hidden;
                    background: linear-gradient(180deg, #1f3550 0%, #223f63 60%, #2a4c75 100%);
                    }

                    body {
                    width: 100%;
                    height: 100%;
                    margin: 0;
                    overflow: hidden;
                    color: #dbe8f4;
                    font: 14px ""Trebuchet MS"", ""Segoe UI"", Arial, sans-serif;
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
                    border: 1px solid #3f5f84;
                    background: #0f1f33;
                    box-shadow: 0 10px 20px rgba(0, 0, 0, 0.35);
                    }

                    .shell {
                    margin: 10px;
                    border: 1px solid #4b6d92;
                    background: #142842;
                    }

                    .top {
                    padding: 9px 12px;
                    border-bottom: 1px solid #3f5f84;
                    background: #1d3758;
                    color: #b9d5f2;
                    font-size: 11px;
                    letter-spacing: 0.8px;
                    }

                    .alert-bar {
                    margin: 12px 12px 0 12px;
                    padding: 8px 10px;
                    border: 1px solid #8e4b56;
                    background: linear-gradient(180deg, #6e2933 0%, #58212a 100%);
                    color: #ffd8dc;
                    text-align: center;
                    font-size: 11px;
                    letter-spacing: 1px;
                    font-weight: bold;
                    }

                    .hero {
                    margin: 12px;
                    padding: 16px;
                    border: 1px solid #4f759b;
                    background: linear-gradient(180deg, #1b3453 0%, #162d49 100%);
                    text-align: center;
                    }

                    .headline {
                    margin: 0;
                    font-size: 22px;
                    color: #dcecff;
                    letter-spacing: 0.7px;
                    }

                    .subhead {
                    margin: 6px 0 0 0;
                    color: #9bc1ea;
                    font-size: 12px;
                    letter-spacing: 0.8px;
                    }

                    .hero-copy {
                    margin: 10px 0 0 0;
                    color: #c0d7ee;
                    font-family: inherit;
                    font-size: 12px;
                    line-height: 1.45;
                    }

                    .section-title {
                    margin: 0 12px 8px 12px;
                    color: #9bc1ea;
                    font-size: 12px;
                    letter-spacing: 0.8px;
                    }

                    .panel {
                    margin: 0 12px 12px 12px;
                    border: 1px solid #496d94;
                    background: #132840;
                    padding: 10px 12px;
                    }

                    .line {
                    margin: 0 0 7px 0;
                    color: #c1d8ee;
                    font-size: 12px;
                    line-height: 1.4;
                    }

                    .line:last-child {
                    margin-bottom: 0;
                    }

                    .grid {
                    margin: 0 12px 12px 12px;
                    text-align: center;
                    }

                    .card {
                    display: inline-block;
                    vertical-align: top;
                    width: 31%;
                    min-width: 180px;
                    margin: 0 4px 8px 4px;
                    border: 1px solid #496d94;
                    border-top-width: 3px;
                    background: #18314d;
                    padding: 10px 8px;
                    box-sizing: border-box;
                    text-align: left;
                    }

                    .card-1 {
                    border-top-color: #7aa0c8;
                    }

                    .card-2 {
                    border-top-color: #d57a84;
                    }

                    .card-3 {
                    border-top-color: #8bb890;
                    }

                    .card-tag {
                    margin: 0 0 6px 0;
                    font-size: 9px;
                    letter-spacing: 0.8px;
                    font-weight: bold;
                    color: #9fc4e8;
                    }

                    .card-name {
                    margin: 0;
                    color: #e0eeff;
                    font-size: 14px;
                    }

                    .card-info {
                    margin: 6px 0 0 0;
                    color: #bdd2e7;
                    font-size: 12px;
                    line-height: 1.35;
                    }

                    .footer {
                    margin: 0 12px 12px 12px;
                    border-top: 1px solid #4f759b;
                    padding-top: 8px;
                    color: #9dbbd8;
                    font-size: 10px;
                    text-align: center;
                    }

                    @media (max-width: 640px) {
                    .card {
                        display: block;
                        width: auto;
                        margin: 0 0 8px 0;
                    }
                    }
                </style>
            </head>
            <body>
                <div class=""scroll-root"">
                    <div class=""page"">
                    <div class=""shell"">
                        <div class=""top"">CITY POLICE NETWORK // ENFORCEMENT ACTION NOTICE</div>

                        <div class=""alert-bar"">RESTRICTED ACCESS // DIGITAL PROPERTY SEIZED</div>

                        <div class=""hero"">
                        <img src=""badge.png"" width=""74"" height=""74"" style=""display:block;margin:0 auto 10px auto;"">
                        <p class=""headline"">This Site Has Been Seized</p>
                        <p class=""subhead"">By Order of the City Police Department</p>
                        <p class=""hero-copy"">Access to this service has been suspended as part of an active police action. The domain, content, and associated digital assets are currently under official control pending investigation, evidence review, and further municipal procedure.</p>
                        </div>

                        <p class=""section-title"">STATUS NOTICE</p>
                        <div class=""panel"">
                        <p class=""line"">- This page is no longer operated by its prior owners or administrators.</p>
                        <p class=""line"">- All connected systems, records, and hosted materials may be subject to examination.</p>
                        <p class=""line"">- Unauthorized attempts to restore, alter, or bypass this notice may result in additional charges.</p>
                        </div>

                        <p class=""section-title"">ENFORCEMENT DETAILS</p>
                        <div class=""grid"">
                        <div class=""card card-1"">
                            <p class=""card-tag"">ACTION</p>
                            <p class=""card-name"">Service Frozen</p>
                            <p class=""card-info"">Public access has been disabled while the department secures relevant digital property and linked infrastructure.</p>
                        </div>
                        <div class=""card card-2"">
                            <p class=""card-tag"">LEGAL STATUS</p>
                            <p class=""card-name"">Under Investigation</p>
                            <p class=""card-info"">Records and materials connected to this page are being retained for official review and possible criminal proceedings.</p>
                        </div>
                        <div class=""card card-3"">
                            <p class=""card-tag"">CONTACT</p>
                            <p class=""card-name"">Police Records Desk</p>
                            <p class=""card-info"">Authorized parties may contact the department through standard municipal channels for verified case-related inquiries.</p>
                        </div>
                        </div>

                        <div class=""footer"">Official municipal portal // Verified law enforcement notice</div>
                    </div>
                    </div>
                </div>
            </body>
        </html>";
    }
}
