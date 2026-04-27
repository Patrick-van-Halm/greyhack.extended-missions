using System;
using System.Collections.Generic;
using ExtendedMissions.Registries;
using ExtendedMissions.Texts;
using ExtendedMissions.Utils;
using MissionConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Util;

namespace ExtendedMissions.Missions
{
    /// <summary>
    /// Describes which direct mission board a custom direct mission should register under.
    /// </summary>
    public enum DirectMissionTarget
    {
        /// <summary>Register the mission on the hack shop board.</summary>
        Hackshop = MissionRegistrationTarget.Hackshop,
        /// <summary>Register the mission on the police board.</summary>
        Police = MissionRegistrationTarget.Police
    }

    /// <summary>
    /// Base class for custom direct missions accepted from a mission board.
    /// </summary>
    public abstract class ExtendedDirectMission<TInstance, TData> : ExtendedMission<TInstance>, IExtendedDirectMission
        where TInstance : ExtendedDirectMission<TInstance, TData>, new()
    {
        /// <summary>
        /// Stored active mission data for a direct mission.
        /// </summary>
        protected sealed class StoredMissionData : ActiveMission
        {
            /// <summary>
            /// Gets or sets the custom mission data.
            /// </summary>
            [JsonProperty]
            public TData Data { get; set; } = default!;

            [JsonConstructor]
            private StoredMissionData() { }

            /// <summary>
            /// Initializes stored direct mission data.
            /// </summary>
            /// <param name="missionTypeId">The extended mission type id.</param>
            /// <param name="targetComputerId">The target computer id.</param>
            /// <param name="reputation">The mission reputation reward.</param>
            /// <param name="karmaType">The mission karma type.</param>
            /// <param name="data">The custom mission data.</param>
            public StoredMissionData(
                int missionTypeId,
                string targetComputerId,
                int reputation,
                KarmaSystem.KarmaType karmaType,
                TData data)
                : base(missionTypeId.ToBaseType(), targetComputerId, reputation, karmaType)
            {
                Data = data;
            }
        }

        /// <summary>
        /// Prepared mission data returned before a direct mission is accepted.
        /// </summary>
        protected class PreparedMission
        {
            /// <summary>
            /// Gets or sets the target computer for the accepted mission.
            /// </summary>
            public Computer TargetComputer { get; set; } = null!;
            /// <summary>
            /// Gets or sets the karma type awarded by the mission.
            /// </summary>
            public KarmaSystem.KarmaType KarmaType { get; set; } = KarmaSystem.KarmaType.NONE;
            /// <summary>
            /// Gets or sets the custom mission data to persist.
            /// </summary>
            public TData MissionData { get; set; } = default!;
            /// <summary>
            /// Gets or sets the text returned to the player when the mission starts.
            /// </summary>
            public string Text { get; set; } = string.Empty;
        }

        /// <inheritdoc />
        public override MissionRegistrationTarget RegistrationTarget => (MissionRegistrationTarget)DirectMissionBoard;

        /// <summary>
        /// Gets the direct mission board this mission registers with.
        /// </summary>
        protected abstract DirectMissionTarget DirectMissionBoard { get; }
        /// <summary>
        /// Gets the title translation id shown in the board preview.
        /// </summary>
        protected abstract string Title { get; }
        /// <summary>
        /// Gets the preview translation id shown in the board listing.
        /// </summary>
        protected abstract string Preview { get; }
        /// <summary>
        /// Gets the mail content translation id sent when the mission is accepted.
        /// </summary>
        protected abstract string Mail { get; }
        /// <summary>
        /// Gets the preview details translation id shown in the board listing.
        /// </summary>
        protected virtual string Details => MissionTexts.None(string.Empty);
        /// <summary>
        /// Gets the minimum reputation required for this mission.
        /// </summary>
        protected virtual int MinReputation => 0;
        /// <summary>
        /// Gets the maximum reputation allowed for this mission.
        /// </summary>
        protected virtual int MaxReputation => 2;
        /// <summary>
        /// Gets optional mission condition tags and translation text.
        /// </summary>
        protected virtual Dictionary<string, string>? Conditions => null;

        private readonly Dictionary<string, string> translations = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, MissionCondition> conditionsByTag = new Dictionary<string, MissionCondition>(StringComparer.Ordinal);
        private readonly DirectMissionPreview missionPreview;

        /// <summary>
        /// Initializes a direct mission definition and builds its preview configuration.
        /// </summary>
        protected ExtendedDirectMission()
        {
            var conditions = new List<MissionCondition>();

            if(Conditions != null)
            {
                foreach (var condition in Conditions)
                {
                    var conditionId = ReserveCondition(condition.Key);
                    var mcondition = new MissionCondition
                    {
                        condition = conditionId.ToBaseCondition(),
                        textID = RegisterTranslation(condition.Key, condition.Value),
                    };
                    conditions.Add(mcondition);
                    conditionsByTag[condition.Key] = mcondition;
                }
            }

            missionPreview = new DirectMissionPreview
            {
                titleID = Title,
                previewID = Preview,
                previewDetailsID = Details,
                missionType = MissionTypeId.ToBaseType(),
                contentID = Mail,
                minRep = MinReputation,
                maxRep = MaxReputation,
                conditions = conditions
            };
        }

        /// <summary>
        /// Gets an optional attachment to include in the acceptance mail.
        /// </summary>
        /// <param name="mission">The accepted active mission.</param>
        /// <returns>The attachment to include, or <see langword="null"/> when no attachment is needed.</returns>
        public virtual FileSystem.Archivo? GetMailAttachment(ActiveMission mission) => null;
        
        /// <summary>
        /// Prepares target data and response text when a player accepts a mission.
        /// </summary>
        /// <param name="mission">The direct mission preview being accepted.</param>
        /// <param name="language">The player language code.</param>
        /// <param name="playerMissions">The player's mission state.</param>
        /// <returns>The prepared mission data, or <see langword="null"/> to decline handling.</returns>
        protected abstract PreparedMission? PrepareMission(DirectMission mission, string language, PlayerMissions playerMissions);

        /// <summary>
        /// Accepts a direct mission and stores its active mission data when this mission type matches.
        /// </summary>
        /// <param name="mission">The direct mission preview being accepted.</param>
        /// <param name="language">The player language code.</param>
        /// <param name="playerMissions">The player's mission state.</param>
        /// <returns>The mission start text, or <see langword="null"/> when this mission type does not handle the preview.</returns>
        public string? AddMission(DirectMission mission, string language, PlayerMissions playerMissions)
        {
            if (mission.missionType.ToMissionTypeId() != MissionTypeId) return null;
            var preparedMission = PrepareMission(mission, language, playerMissions);
            if (preparedMission == null) return null;
            playerMissions.missions.Add(mission.ID, Store(preparedMission.TargetComputer.GetID(), mission.rep, preparedMission.KarmaType, preparedMission.MissionData));
            return preparedMission.Text;
        }

        /// <summary>
        /// Validates whether the player's submitted mail completes the active mission.
        /// </summary>
        /// <param name="mission">The active mission being checked.</param>
        /// <param name="message">The submitted mail message.</param>
        /// <param name="attachment">The submitted mail attachment.</param>
        /// <returns><see langword="true"/> when the mission is complete; otherwise, <see langword="false"/>.</returns>
        protected abstract bool ValidateMission(ActiveMission mission, string message, FileSystem.Archivo attachment);

        /// <summary>
        /// Checks a submitted mission completion mail and returns the localized result text.
        /// </summary>
        /// <param name="mission">The active mission being checked.</param>
        /// <param name="message">The submitted mail message.</param>
        /// <param name="attachment">The submitted mail attachment.</param>
        /// <param name="language">The player language code.</param>
        /// <param name="missionOk">Set to whether the mission was completed.</param>
        /// <returns>The localized result text, or <see langword="null"/> when this mission type does not handle the mission.</returns>
        public string? CheckMission(
            ActiveMission mission,
            string message,
            FileSystem.Archivo attachment,
            string language,
            ref bool missionOk)
        {
            if (mission.typeMission.ToMissionTypeId() != MissionTypeId) return null;
            missionOk = ValidateMission(mission, message, attachment);;
            if(!missionOk && IsSoftLocked(mission)) return MissionTexts.MissionFailed(language);
            return missionOk ? MissionTexts.MissionOk(language) : MissionTexts.MissionNotCompleted(language);
        }

        /// <summary>
        /// Determines whether an incomplete mission can no longer be completed.
        /// </summary>
        /// <param name="mission">The active mission being checked.</param>
        /// <returns><see langword="true"/> when the mission should fail permanently; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsSoftLocked(ActiveMission mission) => false;

        /// <summary>
        /// Deserializes stored active mission data for this mission type.
        /// </summary>
        /// <param name="missionTypeId">The serialized mission type id.</param>
        /// <param name="jObject">The serialized mission JSON object.</param>
        /// <param name="serializer">The JSON serializer to use.</param>
        /// <returns>The deserialized active mission, or <see langword="null"/> when this mission type does not handle the id.</returns>
        public virtual object? ReadMissionFromJson(int missionTypeId, JObject jObject, JsonSerializer serializer)
        {
            if (missionTypeId != MissionTypeId)
            {
                return null;
            }

            return jObject.ToObject<StoredMissionData>(serializer);
        }

        /// <summary>
        /// Creates the direct mission preview configuration registered with the game.
        /// </summary>
        /// <returns>A copy of the direct mission preview configuration.</returns>
        public DirectMissionPreview MissionConfig()
        {
            return new DirectMissionPreview
            {
                titleID = missionPreview.titleID,
                previewID = missionPreview.previewID,
                previewDetailsID = missionPreview.previewDetailsID,
                missionType = missionPreview.missionType,
                contentID = missionPreview.contentID,
                minRep = missionPreview.minRep,
                maxRep = missionPreview.maxRep,
                conditions = missionPreview.conditions
            };
        }

        /// <summary>
        /// Registers all translations declared by this mission.
        /// </summary>
        /// <param name="targetTranslations">The translation dictionary to populate.</param>
        public void RegisterTranslations(IDictionary<string, string> targetTranslations)
        {
            foreach (var translation in translations)
            {
                targetTranslations[translation.Key] = translation.Value;
            }
        }

        /// <summary>
        /// Creates stored active mission data for this mission.
        /// </summary>
        /// <param name="targetComputerId">The target computer id.</param>
        /// <param name="reputation">The mission reputation reward.</param>
        /// <param name="karmaType">The mission karma type.</param>
        /// <param name="data">The custom mission data.</param>
        /// <returns>The stored active mission.</returns>
        protected ActiveMission Store(string targetComputerId, int reputation, KarmaSystem.KarmaType karmaType, TData data)
        {
            return new StoredMissionData(MissionTypeId, targetComputerId, reputation, karmaType, data);
        }

        /// <summary>
        /// Gets custom mission data from a stored active mission.
        /// </summary>
        /// <param name="mission">The stored active mission.</param>
        /// <returns>The custom mission data.</returns>
        protected TData GetData(ActiveMission mission)
        {
            return ((StoredMissionData)mission).Data;
        }

        /// <summary>
        /// Spawns a router suitable for the supplied direct mission.
        /// </summary>
        /// <param name="mission">The direct mission whose access type should be used.</param>
        /// <param name="random">The random number generator used to choose a network type.</param>
        /// <returns>The spawned router.</returns>
        public static Router SpawnMissionRouter(DirectMission mission, Random random)
        {
            var networkTypes = OS.GetListTipoRedComun();
            var networkType = networkTypes[random.Next(networkTypes.Count)];
            return ServerMap.Singleton.SpawnRouter(networkType, mission.GetAccessType(), null, false, "");
        }

        /// <summary>
        /// Builds mission mail text from the configured content and condition translations.
        /// </summary>
        /// <param name="mission">The direct mission preview.</param>
        /// <param name="language">The player language code.</param>
        /// <param name="replacements">Token replacements to apply to the generated text.</param>
        /// <returns>The generated mission text.</returns>
        protected string BuildMissionText(
            DirectMission mission,
            string language,
            params (string Token, string Value)[] replacements)
        {
            var sb = new System.Text.StringBuilder();
            sb = sb.AppendLine(TranslationManager.GetText(mission.contentID, language));
            sb = sb.AppendLine(TranslationManager.GetText(mission.condition.textID, language));

            foreach (var replacement in replacements)
            {
                sb = sb.Replace(replacement.Token, replacement.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Registers a mission-specific translation value.
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
        /// Gets the reserved condition id for a condition tag.
        /// </summary>
        /// <param name="tag">The condition tag.</param>
        /// <returns>The condition id, or -1 when the tag is not registered.</returns>
        protected int GetConditionId(string tag)
        {
            return conditionsByTag.TryGetValue(tag, out var condition) ? (int)condition.condition : -1;
        }

        /// <summary>
        /// Reserves a condition id for a mission-local tag.
        /// </summary>
        /// <param name="tag">The condition tag.</param>
        /// <returns>The reserved condition id.</returns>
        protected static int ReserveCondition(string tag)
        {
            return MissionConditionRegistry.Instance.Reserve($"{StaticKey}.condition.{tag}");
        }

        /// <summary>
        /// Builds the fully qualified translation key for a mission-local key.
        /// </summary>
        /// <param name="key">The mission-local translation key.</param>
        /// <returns>The fully qualified translation key.</returns>
        protected string GetTranslationKey(string key) => $"{StaticKey}.translations.{key}";
    }
}
