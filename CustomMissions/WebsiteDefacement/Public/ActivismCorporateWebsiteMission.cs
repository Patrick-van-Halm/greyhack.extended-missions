using System.Collections.Generic;
using System.Linq;
using ExtendedMissions.Missions;
using ExtendedMissions.Utils;
using MissionConfig;
using ServiceConfig;
using Random = System.Random;

namespace ExtendedMissions.Registries
{
    internal class ActivismCorporateWebsiteMission : ExtendedDirectMission<ActivismCorporateWebsiteMission, ActivismCorporateWebsiteMission.Data>
    {
        private const string ByDomainConditionTag = "BY_DOMAIN";
        private int MissionConditionWebsiteByDomain => GetConditionId(ByDomainConditionTag);
        private const string ByIpConditionTag = "BY_IP";

        internal class Data { }

        protected override string Title => "Corporate Website Exposure";
        protected override string Preview => "Expose a target website by replacing its content with an activism message.";
        protected override string Mail => "The client wants to expose a website.\nOperate on /Public/htdocs/website.html.";
        protected override string Details => "The client wants to expose a website by replacing its content with an activism message. The required HTML page is attached to the contract mail.";
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
            DebugLogger.Log("[ActivismCorporateWebsiteMission] Add Mission to User");
            var random = RandomUtils.CreateRandom();
            var webserverTypes = XmlGlobal.Singleton.tipoServidor.publicServices.Where(s => s.Value.Any(service => service.ID == ServicioID.http) && new ServerMap.TipoRed[] {ServerMap.TipoRed.Supermercados, ServerMap.TipoRed.FastFood, ServerMap.TipoRed.MobileShop, ServerMap.TipoRed.Bancos, ServerMap.TipoRed.TiendaInformatica, ServerMap.TipoRed.HardwareManufacturer}.Contains(s.Key));
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
                KarmaType = KarmaSystem.KarmaType.GREY,
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
        <title>Corporate Exposure Notice</title>
        <style>
            html { width: 100%; height: 100%; margin: 0; background: linear-gradient(180deg, #160c0c 0%, #231111 55%, #1b1510 100%); }
            body { margin: 0; color: #ffe7e0; font: 14px ""Trebuchet MS"", ""Segoe UI"", Arial, sans-serif; }
            .wrap { max-width: 860px; margin: 40px auto; border: 1px solid #7c3d3d; background: #140b0b; box-shadow: 0 10px 25px rgba(0,0,0,0.45); }
            .inner { margin: 10px; border: 1px solid #9a5252; background: #1d1111; }
            .header { padding: 10px; background: linear-gradient(90deg, #3a1717 0%, #4a1f1f 100%); border-bottom: 1px solid #7c3d3d; font-size: 11px; letter-spacing: 1px; color: #ffb2a5; }
            .alert { margin: 12px 12px 0 12px; padding: 8px 10px; border: 1px solid #b35c52; background: #2b1414; color: #ffd1c8; text-align: center; font-size: 11px; letter-spacing: 1px; font-weight: bold; }
            .hero { padding: 20px; text-align: center; border-bottom: 1px solid #7c3d3d; background: linear-gradient(180deg, #241313 0%, #1b1010 100%); }
            .hero h1 { margin: 0; font-size: 26px; color: #ffffff; letter-spacing: 0.6px; text-transform: uppercase; }
            .hero p { margin: 10px 0 0 0; font-size: 13px; color: #ffd6ce; line-height: 1.45; }
            .section { padding: 16px; border-bottom: 1px solid #7c3d3d; }
            .section-title { font-size: 12px; color: #ff9b8c; margin-bottom: 8px; letter-spacing: 0.8px; }
            .line { font-size: 13px; margin-bottom: 6px; color: #ffe7e0; line-height: 1.45; }
            .quote-box { margin: 0 16px 16px 16px; padding: 12px; border: 1px solid #9a5252; background: #180d0d; color: #ffd7ce; font-size: 13px; line-height: 1.5; text-align: center; }
            .footer { padding: 10px; text-align: center; font-size: 11px; color: #d99a90; }
        </style>
        </head>
        <body>
        <div class=""wrap"">
            <div class=""inner"">
            <div class=""header"">PUBLIC ACCESS NODE // CORPORATE ACCOUNTABILITY MESSAGE</div>

            <div class=""alert"">THIS PAGE HAS BEEN REPURPOSED TO CHALLENGE CORPORATE POWER</div>

            <div class=""hero"">
                <h1>No More Corporate Silence</h1>
                <p>This message applies to every storefront, bank terminal, restaurant chain, and supermarket aisle. Wherever profit overrides people, accountability must follow.</p>
            </div>

            <div class=""section"">
                <div class=""section-title"">ACROSS ALL INDUSTRIES</div>
                <div class=""line"">- Shops: pricing pressure, supplier squeeze, and worker underpayment hidden behind friendly branding.</div>
                <div class=""line"">- Banks: fees, leverage, and risk shifted onto customers while gains remain private.</div>
                <div class=""line"">- Restaurants: long hours, low wages, and unstable work normalized as ""industry standard"".</div>
                <div class=""line"">- Supermarkets: cost-cutting that impacts workers and suppliers while shelves stay full.</div>
            </div>

            <div class=""section"">
                <div class=""section-title"">THE PATTERN</div>
                <div class=""line"">- Polished interfaces, loyalty programs, and marketing campaigns conceal structural imbalance.</div>
                <div class=""line"">- Decisions are centralized, consequences are distributed to workers and customers.</div>
                <div class=""line"">- When profit rises, responsibility is deferred; when losses occur, the public absorbs them.</div>
            </div>

            <div class=""quote-box"">
                They call it business. We call it extraction.<br>
                They call it efficiency. We call it disposable lives.<br>
                They call it growth. We call it power without accountability.
            </div>

            <div class=""section"">
                <div class=""section-title"">WHAT PEOPLE CAN DO</div>
                <div class=""line"">- Ask questions, compare sources, and refuse blind trust in branding.</div>
                <div class=""line"">- Support fair labor, transparent pricing, and ethical operations.</div>
                <div class=""line"">- Share information and hold institutions accountable through visibility.</div>
            </div>

            <div class=""footer"">people over profit // awareness across all sectors</div>
            </div>
        </div>
        </body>
        </html>";
    }
}
