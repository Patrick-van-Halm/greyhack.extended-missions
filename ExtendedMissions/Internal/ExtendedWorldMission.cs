using System;
using System.Collections.Generic;
using ExtendedMissions.Registries;
using ExtendedMissions.Utils;
using MissionConfig;
using ServiceConfig;

namespace ExtendedMissions.Missions
{
    /// <summary>
    /// Describes the world mission board a custom world mission should register under.
    /// </summary>
    public enum WorldMissionTarget
    {
        /// <summary>Register the mission as a black hat mission.</summary>
        Black = MissionRegistrationTarget.BlackHat,
        /// <summary>Register the mission as a white hat mission.</summary>
        White = MissionRegistrationTarget.WhiteHat,
        /// <summary>Register the mission as a grey hat mission.</summary>
        Grey = MissionRegistrationTarget.GreyHat,
        /// <summary>Register the mission as a hidden mission.</summary>
        Hidden = MissionRegistrationTarget.Hidden,
    }

    /// <summary>
    /// Base class for custom missions generated into the world mission system.
    /// </summary>
    /// <typeparam name="TInstance">The concrete singleton mission type.</typeparam>
    public abstract class ExtendedWorldMission<TInstance> : ExtendedMission<TInstance>, IExtendedWorldMission
        where TInstance : ExtendedWorldMission<TInstance>, new()
    {
        /// <summary>
        /// Gets optional starter text shown when the world mission starts.
        /// </summary>
        protected virtual string StarterText => string.Empty;
        /// <summary>
        /// Gets the reward tier used by the world mission.
        /// </summary>
        protected abstract int RewardTier { get; }

        private readonly List<ComputerMission> computerMissions = new List<ComputerMission>();
        private readonly List<PreGenNpc> generatedNpcData = new List<PreGenNpc>();
        private readonly Dictionary<string, string> translations = new Dictionary<string, string>();

        /// <summary>
        /// Creates the world mission configuration registered with the game.
        /// </summary>
        /// <returns>The world mission configuration.</returns>
        public Mission? MissionConfig()
        {
            var starterTextID = string.IsNullOrEmpty(StarterText) ? "" : RegisterTranslation("starter", StarterText);
            var mission = new Mission
            {
                missionType = Instance.RegistrationTarget switch
                {
                    MissionRegistrationTarget.BlackHat => TypeMission.BLACK,
                    MissionRegistrationTarget.WhiteHat => TypeMission.WHITE,
                    MissionRegistrationTarget.GreyHat => TypeMission.GREY,
                    MissionRegistrationTarget.Hidden => TypeMission.HIDDEN,
                    MissionRegistrationTarget.Procedural => TypeMission.PROCEDURAL,
                    _ => TypeMission.GREY,
                },
                computers = new List<ComputerMission>(computerMissions),
                preGenNpcs = new List<PreGenNpc>(generatedNpcData),
                starterTextID = starterTextID,
                rewardTier = RewardTier
            };

            return mission;
        }

        /// <summary>
        /// Generates a computer for the world mission
        /// </summary>
        /// <param name="networkIndex">The index of the remote network. Having 2 with the same index will add the computers to the same network.</param>
        /// <param name="cloneNpcIndex">The specific npc index (pre generated npcs) to use for this computer, null will pick a random non guilty one. The guilty npc is only picked for the last computer in the list.</param>
        /// <param name="networkType">The specific network type to be generated, if not provided it will be randomized.</param>
        /// <returns>The generated computer mission index.</returns>
        protected int AddComputerMission(
            int networkIndex = 0,
            int? cloneNpcIndex = null,
            ServerMap.TipoRed? networkType = null)
        {
            var computerMission = new ComputerMission
            {
                computerIndexNet = networkIndex,
                cloneNpcPcIdx = cloneNpcIndex ?? -1,
            };

            if (networkType.HasValue)
            {
                computerMission.tipoRed = networkType.Value;
                computerMission.isRandomTipoRed = false;
            }

            computerMissions.Add(computerMission);
            return computerMissions.Count - 1;
        }

        /// <summary>
        /// Adds a generated chat log mission item to a generated computer.
        /// </summary>
        /// <param name="computerIndex">The generated computer index.</param>
        /// <param name="text">The chat log text.</param>
        /// <param name="isEvidence">Whether the item counts as evidence.</param>
        protected void AddChatLogMissionItem(int computerIndex, string text, bool isEvidence = false)
        {
            if (computerIndex < 0 || computerIndex >= computerMissions.Count) throw new IndexOutOfRangeException($"{nameof(computerIndex)} is outside of the computer count");
            var computerMission = computerMissions[computerIndex].GetMission();

            var textId = RegisterTranslation($"mission_item.{computerIndex}_{computerMission.Count}.text", text);
            computerMission.Add(new MissionItem(TypeItem.ChatLog)
            {
                ID = textId,
                isEvidence = isEvidence
            });
        }

        /// <summary>
        /// Adds a generated text file mission item to a generated computer.
        /// </summary>
        /// <param name="computerIndex">The generated computer index.</param>
        /// <param name="text">The text file content.</param>
        /// <param name="isEvidence">Whether the item counts as evidence.</param>
        protected void AddTextFileMissionItem(int computerIndex, string text, bool isEvidence = false)
        {
            if (computerIndex < 0 || computerIndex >= computerMissions.Count) throw new IndexOutOfRangeException($"{nameof(computerIndex)} is outside of the computer count");
            var computerMission = computerMissions[computerIndex].GetMission();

            var textId = RegisterTranslation($"mission_item.{computerIndex}_{computerMission.Count}.text", text);
            computerMission.Add(new MissionItem(TypeItem.TextFile)
            {
                ID = textId,
                isEvidence = isEvidence
            });
        }

        /// <summary>
        /// Adds a firewall rule mission item to a generated computer.
        /// </summary>
        /// <param name="computerIndex">The generated computer index.</param>
        /// <param name="port">The optional port to target, or any port when omitted.</param>
        /// <param name="action">The firewall action to generate.</param>
        protected void AddFirewallMissionItem(int computerIndex, int? port = null, HwFirewallRule.Action action = HwFirewallRule.Action.DENY)
        {
            if (computerIndex < 0 || computerIndex >= computerMissions.Count) throw new IndexOutOfRangeException($"{nameof(computerIndex)} is outside of the computer count");
            var computerMission = computerMissions[computerIndex].GetMission();
            computerMission.Add(new FirewallItem()
            {
                port = port ?? -1,
                statusPort = port == null ? HwFirewallRule.Rule.Status.ANY : HwFirewallRule.Rule.Status.SINGLE,
                action = action,
                statusDestIP = HwFirewallRule.Rule.Status.ANY,
                statusSourceIP = HwFirewallRule.Rule.Status.ANY,
            });
        }

        /// <summary>
        /// Adds a system log mission item to a generated computer.
        /// </summary>
        /// <param name="computerIndex">The generated computer index that receives the log.</param>
        /// <param name="targetComputerIndex">The generated target computer index referenced by the log.</param>
        /// <param name="port">The port referenced by the log.</param>
        /// <param name="action">The log action to generate.</param>
        /// <param name="wasSuccessful">Whether the logged connection was successful.</param>
        protected void AddSystemLogMissionItem(int computerIndex, int targetComputerIndex, int port, LogSystem.StatusLog action, bool wasSuccessful = false)
        {
            if (computerIndex < 0 || computerIndex >= computerMissions.Count) throw new IndexOutOfRangeException($"{nameof(computerIndex)} is outside of the computer count");
            var computerMission = computerMissions[computerIndex].GetMission();
            computerMission.Add(new SystemLogItem()
            {
                action = action,
                port = port,
                toComputerIdx = targetComputerIndex,
                isReceivingConn = wasSuccessful
            });
        }

        /// <summary>
        /// Adds a mail conversation mission item to a generated computer.
        /// </summary>
        /// <param name="computerIndex">The generated computer index.</param>
        /// <param name="text">The mail body text.</param>
        /// <param name="subjectText">The mail subject text.</param>
        /// <param name="targetNpcIndex">The optional generated NPC index that should receive the mail.</param>
        protected void AddMailMissionItem(int computerIndex, string text, string subjectText, int? targetNpcIndex = null)
        {
            if (computerIndex < 0 || computerIndex >= computerMissions.Count) throw new IndexOutOfRangeException($"{nameof(computerIndex)} is outside of the computer count");
            var computerMission = computerMissions[computerIndex].GetMission();
            var textId = RegisterTranslation($"mission_item.{computerIndex}_{computerMission.Count}.text", text);
            var subjectId = RegisterTranslation($"mission_item.{computerIndex}_{computerMission.Count}.subject", subjectText);
            computerMission.Add(new MailConversItem()
            {
                ID = textId,
                titleID = subjectId,
                useNetworkNpcSender = targetNpcIndex == null,
                forceNpcReceive = true,
                indexNpc = targetNpcIndex ?? -1
            });
        }

        /// <summary>
        /// Adds a service mission item to a generated computer.
        /// </summary>
        /// <param name="computerIndex">The generated computer index.</param>
        /// <param name="service">The service to generate.</param>
        /// <param name="allowMultiple">Whether duplicate services are allowed.</param>
        protected void AddServiceMissionItem(int computerIndex, ServicioID service, bool allowMultiple = false)
        {
            if (computerIndex < 0 || computerIndex >= computerMissions.Count) throw new IndexOutOfRangeException($"{nameof(computerIndex)} is outside of the computer count");
            var computerMission = computerMissions[computerIndex].GetMission();
            computerMission.Add(new ServiceItem()
            {
                servicio = service,
                allowDuplicates = allowMultiple
            });
        }

        /// <summary>
        /// Adds an NPC definition that generated computers can reference.
        /// </summary>
        /// <param name="isVictim">Whether the NPC is the mission victim.</param>
        /// <param name="isGuilty">Whether the NPC is the guilty actor.</param>
        /// <param name="forcedPass">An optional forced password for the NPC.</param>
        /// <returns>The generated NPC index.</returns>
        protected int AddGeneratedNpc(
            bool isVictim = false,
            bool isGuilty = false,
            string? forcedPass = null)
        {
            generatedNpcData.Add(new PreGenNpc
            {
                isVictim = isVictim,
                isGuilty = isGuilty,
                isBlackmailed = false,
                isVictimNotified = false,
                isForcedPass = forcedPass != null,
                forcedPass = forcedPass,
            });

            return generatedNpcData.Count - 1;
        }

        /// <summary>
        /// Registers a world mission-specific translation value.
        /// </summary>
        /// <param name="key">The mission-local translation key.</param>
        /// <param name="value">The translated text.</param>
        /// <returns>The fully qualified translation key.</returns>
        protected string RegisterTranslation(string key, string value)
        {
            var translationKey = GetTranslationKey(key);
            translations[translationKey] = value;
            return translationKey;
        }

        /// <summary>
        /// Builds the fully qualified translation key for a mission-local key.
        /// </summary>
        /// <param name="key">The mission-local translation key.</param>
        /// <returns>The fully qualified translation key.</returns>
        protected string GetTranslationKey(string key) => $"{StaticKey}.translations.{key}";

        /// <summary>
        /// Registers all translations declared by this world mission.
        /// </summary>
        /// <param name="targetTranslations">The translation dictionary to populate.</param>
        public void RegisterTranslations(IDictionary<string, string> targetTranslations)
        {
            DebugLogger.Log($"> Initializing {GetType().Name} Procedural Mission Translations");
            foreach (var translation in translations)
            {
                targetTranslations[translation.Key] = translation.Value;
            }
        }
    }
}
