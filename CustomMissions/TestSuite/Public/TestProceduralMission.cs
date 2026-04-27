using System.Collections.Generic;
using MissionConfig;

namespace ExtendedMissions.Missions.Test
{
    internal class TestProceduralMission : ExtendedProceduralMission<TestProceduralMission>
    {
        protected override string ArcheTypeText => "EXAMPLE TEST";
        protected override string ArcheTypeDetailText => "This is an example procedural mission";

        public TestProceduralMission()
        {
            AddGroup("test"); // The group of used for starter
            AddGroupStarter("test", "EXAMPLE STARTER $IP_REMOTE, $IP_REMLOCAL, $NPC_NAME_GLOBAL, $NEXT_NPC_PASSWORD", new List<EvidenceFunction>()
            {
                new EvidenceFunction
                {
                    type = EvidenceFunctionType.NextNodeRemote
                },
                new EvidenceFunction
                {
                    type = EvidenceFunctionType.NpcNameGlobal,
                    forceAccountOnNextNode = true,
                },
                new EvidenceFunction
                {
                    type = EvidenceFunctionType.NextNpcPassword,
                },
                new EvidenceFunction
                {
                    type = EvidenceFunctionType.NextServiceRemote,
                    service = ServiceConfig.ServicioID.ssh,
                }
            });
            AddFileHeartBeatEntry(Verb.Recon, "EXAMPLE HEART: $IP_REMOTE, $IP_REMLOCAL, $NPC_NAME_GLOBAL, $NEXT_NPC_PASSWORD.",  new List<EvidenceFunction>
            {
                new EvidenceFunction { type = EvidenceFunctionType.NextNodeRemote },
                new EvidenceFunction { type = EvidenceFunctionType.NpcNameGlobal },
                new EvidenceFunction
                {
                    type = EvidenceFunctionType.NextServiceRemote,
                    service = ServiceConfig.ServicioID.ssh
                },
                new EvidenceFunction
                {
                    type = EvidenceFunctionType.NextNpcPassword,
                },
            });
            AddClosureEntry("test", ClosureEntryType.File, "EXAMPLE CLOSURE: Internal note: $NPC_NAME_GLOBAL authorized the malware deployment.",  new List<EvidenceFunction>
            {
                new EvidenceFunction { type = EvidenceFunctionType.NpcNameGlobal }
            });
        }
    }
}
