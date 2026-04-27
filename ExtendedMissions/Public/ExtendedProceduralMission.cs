using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedMissions.Registries;
using ExtendedMissions.Utils;
using MissionConfig;

namespace ExtendedMissions.Missions
{
    /// <summary>
    /// Base class for custom missions that can be offered through the procedural mission system.
    /// </summary>
    /// 
    public abstract class ExtendedProceduralMission<TInstance> : ExtendedMission<TInstance>, IExtendedProceduralMission
        where TInstance : ExtendedProceduralMission<TInstance>, new()
    {
        /// <summary>
        /// Describes an optional support chat log generated alongside procedural mission content.
        /// </summary>
        public sealed class SupportChatLogBuilder
        {
            /// <summary>
            /// Gets or sets the support chat log text.
            /// </summary>
            public string Text { get; set; } = "";
            /// <summary>
            /// Gets or sets the optional support chat log path.
            /// </summary>
            public string? Path { get; set; } = null;
        }

        /// <inheritdoc />
        public override MissionRegistrationTarget RegistrationTarget => MissionRegistrationTarget.Procedural;
        private readonly Dictionary<string, string> translations = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Gets the reserved procedural archetype id.
        /// </summary>
        public int ArcheTypeId { get; private set; }

        /// <summary>
        /// Gets the translation id for the procedural archetype text.
        /// </summary>
        public string ArcheTypeTextId { get; private set; }
        /// <summary>
        /// Gets the translation id for the procedural archetype detail text.
        /// </summary>
        public string ArcheTypeDetailTextId { get; private set; }

        /// <summary>
        /// Gets the procedural archetype text.
        /// </summary>
        protected abstract string ArcheTypeText { get; }
        /// <summary>
        /// Gets the procedural archetype detail text.
        /// </summary>
        protected abstract string ArcheTypeDetailText { get; }

        private readonly ClosureConfig _closure = new ClosureConfig();
        private readonly HeartConfig _heart = new HeartConfig();
        private readonly List<TemplateOption> _starterTemplates = new List<TemplateOption>();
        private readonly List<string> _groups = new List<string>();

        /// <summary>
        /// Initializes a procedural mission definition and reserves its archetype ids.
        /// </summary>
        public ExtendedProceduralMission()
        {
            ArcheTypeId = ProceduralArchetypesRegistry.Instance.Reserve($"{StaticKey}.ArcheType");
            ArcheTypeTextId = RegisterTranslation("archetype.text", ArcheTypeText);
            ArcheTypeDetailTextId = RegisterTranslation("archetype.detail", ArcheTypeDetailText);
        }

        /// <summary>
        /// Creates the procedural archetype configuration registered with the game.
        /// </summary>
        /// <returns>The procedural archetype configuration.</returns>
        /// <exception cref="Exception">Thrown when required groups, starters, heart, or closure configuration is missing or inconsistent.</exception>
        public ArchetypeConfig ProceduralMissionConfig()
        {
            if (_groups == null || _groups.Count == 0 || _starterTemplates == null || _starterTemplates.Count == 0 || _heart == null || _closure == null)
            {
                throw new Exception($"Procedural mission is not initialized correctly");
            }

            foreach(var template in _starterTemplates)
            {
                if (_groups.Contains(template.group)) continue;
                throw new Exception($"StarterTemplate group '{template.group}' is not part of {nameof(_groups)}");
            }

            foreach(var group in _groups)
            {
                if (_starterTemplates.Any(template => template.group == group)) continue;
                throw new Exception($"Group: '{group}' has no starter template");
            }

            return new ArchetypeConfig()
            {
                archetype = (Archetype) ArcheTypeId,
                closure = _closure,
                groups = _groups,
                heart = _heart,
                starters = _starterTemplates
            };
        }

        /// <summary>
        /// Adds a procedural mission group.
        /// </summary>
        /// <param name="name">The group name.</param>
        /// <exception cref="Exception">Thrown when a group with the same name already exists.</exception>
        protected void AddGroup(string name)
        {
            if (_groups.Contains(name)) throw new Exception($"Group with name '{name}' already exists");
            _groups.Add(name);
        }

        /// <summary>
        /// Adds a starter template for an existing procedural mission group.
        /// </summary>
        /// <param name="name">The group name.</param>
        /// <param name="emailText">The starter email text.</param>
        /// <param name="evidences">The evidence functions generated by the starter.</param>
        /// <exception cref="Exception">Thrown when the group has not been added.</exception>
        protected void AddGroupStarter(
            string name,
            string emailText,
            List<EvidenceFunction> evidences)
        {
            if (!_groups.Contains(name)) throw new Exception($"Group with name '{name}' doesn't exist - Add it first");
            var idx = _starterTemplates.Count(template => template.group == name);

            _starterTemplates.Add(CreateTemplateOption(
                $"starter.{name}_{idx}",
                name,
                emailText,
                evidences,
                string.Empty,
                null,
                null,
                false,
                string.Empty,
                null,
                null
            ));
        }

        private VerbConfig GetOrAddVerb(Verb verb)
        {
            var verbConfig = _heart.verbs.FirstOrDefault(v => v.verb == verb);
            if (verbConfig != null) return verbConfig;

            verbConfig = new VerbConfig()
            {
                verb = verb,
                modalities = new List<ModalityConfig>()
            };
            _heart.verbs.Add(verbConfig);
            return verbConfig;
        }

        private ModalityConfig GetOrAddModality(VerbConfig verbConfig, Modality modality)
        {
            var modalityConfig = verbConfig.modalities.FirstOrDefault(m => m.modality == modality);
            if (modalityConfig != null) return modalityConfig;

            modalityConfig = new ModalityConfig()
            {
                modality = modality,
                options = new List<TemplateOption>()
            };
            verbConfig.modalities.Add(modalityConfig);
            return modalityConfig;
        }

        /// <summary>
        /// Adds a file-based heart beat entry to the procedural mission.
        /// </summary>
        /// <param name="verb">The mission verb associated with the entry.</param>
        /// <param name="text">The generated file text.</param>
        /// <param name="evidences">The evidence functions generated by the entry.</param>
        /// <param name="type">The generated file type.</param>
        /// <param name="fileName">The optional fixed file name.</param>
        /// <param name="path">The optional fixed file path.</param>
        /// <param name="supportChatLog">Optional support chat log content.</param>
        protected void AddFileHeartBeatEntry(Verb verb, string text, List<EvidenceFunction> evidences, FileType type = FileType.Text, string? fileName = null, string? path = null, SupportChatLogBuilder? supportChatLog = null)
        {
            var verbConfig = GetOrAddVerb(verb);
            var modalityConfig = GetOrAddModality(verbConfig, Modality.File);
            var idx = modalityConfig.options.Count;
            var fileSpec = new FileOptionSpec();
            var modifiedSpec = false;
            
            if (!string.IsNullOrEmpty(fileName))
            {
                fileSpec.fileName = fileName;
                fileSpec.randomFileName = false;
                modifiedSpec = true;
            }

            if (!string.IsNullOrEmpty(path))
            {
                fileSpec.path = path;
                fileSpec.randomPath = false;
                modifiedSpec = true;
            }

            switch(type)
            {
                case FileType.Binary:
                    fileSpec.isBinary = true;
                    modifiedSpec = true;
                    break;

                case FileType.Image:
                    fileSpec.isImage = true;
                    modifiedSpec = true;
                    break;

                case FileType.Pdf:
                    fileSpec.isPdf = true;
                    modifiedSpec = true;
                    break;

                case FileType.Script:
                    fileSpec.isScript = true;
                    modifiedSpec = true;
                    break;
            }

            var option = CreateTemplateOption(
                $"heart.{verb}.{modalityConfig.modality}_{idx}",
                string.Empty,
                text,
                evidences,
                string.Empty,
                modifiedSpec ? fileSpec : null,
                null,
                supportChatLog != null,
                supportChatLog != null ? supportChatLog.Text : string.Empty,
                supportChatLog != null ? new ChatLogPathSpec()
                {
                    path = supportChatLog.Path != null ? supportChatLog.Path : string.Empty,
                    randomPath = supportChatLog.Path == null
                } : null,
                null
            );
            modalityConfig.options.Add(option);
        }

        /// <summary>
        /// Adds a chat-log heart beat entry to the procedural mission.
        /// </summary>
        /// <param name="verb">The mission verb associated with the entry.</param>
        /// <param name="text">The generated chat log text.</param>
        /// <param name="evidences">The evidence functions generated by the entry.</param>
        /// <param name="path">The optional fixed chat log path.</param>
        /// <param name="supportChatLog">Optional support chat log content.</param>
        protected void AddChatLogHeartBeatEntry(Verb verb, string text, List<EvidenceFunction> evidences, string? path = null, SupportChatLogBuilder? supportChatLog = null)
        {
            var verbConfig = GetOrAddVerb(verb);
            var modalityConfig = GetOrAddModality(verbConfig, Modality.ChatLog);
            var idx = modalityConfig.options.Count;

            var option = CreateTemplateOption(
                $"heart.{verb}.{modalityConfig.modality}_{idx}",
                string.Empty,
                text,
                evidences,
                string.Empty,
                null,
                string.IsNullOrEmpty(path) ? null : new ChatLogPathSpec()
                {
                    path = path,
                    randomPath = false
                },
                supportChatLog != null,
                supportChatLog != null ? supportChatLog.Text : string.Empty,
                supportChatLog != null ? new ChatLogPathSpec()
                {
                    path = supportChatLog.Path != null ? supportChatLog.Path : string.Empty,
                    randomPath = supportChatLog.Path == null
                } : null,
                null
            );
            modalityConfig.options.Add(option);
        }

        /// <summary>
        /// Adds an email heart beat entry to the procedural mission.
        /// </summary>
        /// <param name="verb">The mission verb associated with the entry.</param>
        /// <param name="text">The generated email body text.</param>
        /// <param name="evidences">The evidence functions generated by the entry.</param>
        /// <param name="subjectText">The optional generated email subject text.</param>
        /// <param name="supportChatLog">Optional support chat log content.</param>
        protected void AddMailHeartBeatEntry(Verb verb, string text, List<EvidenceFunction> evidences, string subjectText = "", SupportChatLogBuilder? supportChatLog = null)
        {
            var verbConfig = GetOrAddVerb(verb);
            var modalityConfig = GetOrAddModality(verbConfig, Modality.Email);
            var idx = modalityConfig.options.Count;

            var option = CreateTemplateOption(
                $"heart.{verb}.{modalityConfig.modality}_{idx}",
                string.Empty,
                text,
                evidences,
                subjectText,
                null,
                null,
                supportChatLog != null,
                supportChatLog != null ? supportChatLog.Text : string.Empty,
                supportChatLog != null ? new ChatLogPathSpec()
                {
                    path = supportChatLog.Path != null ? supportChatLog.Path : string.Empty,
                    randomPath = supportChatLog.Path == null
                } : null,
                null
            );
            modalityConfig.options.Add(option);
        }

        private CrossInfoSpec CreateCrossInfoSpec(
            string keyPrefix,
            CrossInfoPayloadType currentType,
            CrossInfoPayloadType nextType,
            CrossInfoNextMode nextMode,
            string nextText,
            List<EvidenceFunction> nextEvidences,
            FileOptionSpec? nextFileSpec,
            ChatLogPathSpec? nextChatLogPathSpec,
            SupportChatLogBuilder? nextSupportChatLog = null)
        {
            return new CrossInfoSpec
            {
                currentType = currentType,
                nextType = nextType,
                nextMode = nextMode,
                nextOption = CreateTemplateOption(
                    $"{keyPrefix}.next",
                    string.Empty,
                    nextText,
                    nextEvidences,
                    string.Empty,
                    nextFileSpec,
                    nextChatLogPathSpec,
                    nextSupportChatLog != null,
                    nextSupportChatLog != null ? nextSupportChatLog.Text : string.Empty,
                    nextSupportChatLog != null ? new ChatLogPathSpec()
                    {
                        path = nextSupportChatLog.Path != null ? nextSupportChatLog.Path : string.Empty,
                        randomPath = nextSupportChatLog.Path == null
                    } : null,
                    null
                )
            };
        }

        /// <summary>
        /// Adds a cross-info heart beat entry that links one generated payload to another.
        /// </summary>
        /// <param name="verb">The mission verb associated with the entry.</param>
        /// <param name="text">The current payload text.</param>
        /// <param name="evidences">The evidence functions generated by the current payload.</param>
        /// <param name="currentType">The current payload type.</param>
        /// <param name="nextType">The linked payload type.</param>
        /// <param name="nextMode">The mode used to generate or select the linked payload.</param>
        /// <param name="nextText">The linked payload text.</param>
        /// <param name="nextEvidences">The evidence functions generated by the linked payload.</param>
        /// <param name="supportChatLog">Optional support chat log content for the current payload.</param>
        /// <param name="nextsupportChatLog">Optional support chat log content for the linked payload.</param>
        /// <param name="nextFileSpec">Optional file specification for the linked payload.</param>
        /// <param name="nextChatLogPathSpec">Optional chat log path specification for the linked payload.</param>
        protected void AddCrossInfoHeartBeatEntry(
            Verb verb,
            string text,
            List<EvidenceFunction> evidences,
            CrossInfoPayloadType currentType,
            CrossInfoPayloadType nextType,
            CrossInfoNextMode nextMode,
            string nextText,
            List<EvidenceFunction> nextEvidences,
            SupportChatLogBuilder? supportChatLog = null,
            SupportChatLogBuilder? nextsupportChatLog = null,
            FileOptionSpec? nextFileSpec = null,
            ChatLogPathSpec? nextChatLogPathSpec = null)
        {
            var verbConfig = GetOrAddVerb(verb);
            var modalityConfig = GetOrAddModality(verbConfig, Modality.CrossInfo);
            var idx = modalityConfig.options.Count;

            var option = CreateTemplateOption(
                $"heart.{verb}.{modalityConfig.modality}_{idx}",
                string.Empty,
                text,
                evidences,
                string.Empty,
                null,
                null,
                supportChatLog != null,
                supportChatLog != null ? supportChatLog.Text : string.Empty,
                supportChatLog != null ? new ChatLogPathSpec()
                {
                    path = supportChatLog.Path != null ? supportChatLog.Path : string.Empty,
                    randomPath = supportChatLog.Path == null
                } : null,
                CreateCrossInfoSpec(
                    $"heart.{verb}.{modalityConfig.modality}_{idx}.cross_info",
                    currentType,
                    nextType,
                    nextMode,
                    nextText,
                    nextEvidences,
                    nextFileSpec,
                    nextChatLogPathSpec,
                    nextsupportChatLog
                )
            );
            modalityConfig.options.Add(option);
        }

        /// <summary>
        /// Adds a closure entry for a procedural mission group.
        /// </summary>
        /// <param name="group">The group name.</param>
        /// <param name="type">The closure entry type.</param>
        /// <param name="text">The closure text.</param>
        /// <param name="evidences">Optional evidence functions generated by the closure.</param>
        /// <param name="fileSpec">Optional generated file specification.</param>
        /// <param name="chatLogPathSpec">Optional generated chat log path specification.</param>
        /// <param name="supportChatLog">Optional support chat log content.</param>
        /// <exception cref="Exception">Thrown when the group is missing or has no starter template.</exception>
        protected void AddClosureEntry(
            string group,
            ClosureEntryType type,
            string text,
            List<EvidenceFunction>? evidences = null,
            FileOptionSpec? fileSpec = null,
            ChatLogPathSpec? chatLogPathSpec = null,
            SupportChatLogBuilder? supportChatLog = null)
        {
            if (!_groups.Contains(group)) throw new Exception($"Group with name '{group}' doesn't exist - Add it first");
            if (!_starterTemplates.Any(t => t.group == group)) throw new Exception($"Group with name '{group}' doesn't have a starter template yet - Add it first");
            var idx = _closure.entries.Count(entry => entry.option.group == group);
            _closure.entries.Add(new ClosureEntry
            {
                type = type,
                option = CreateTemplateOption(
                    $"closure.{group}_{idx}",
                    group,
                    text,
                    evidences ?? new List<EvidenceFunction>(),
                    string.Empty,
                    fileSpec,
                    chatLogPathSpec,
                    supportChatLog != null,
                    supportChatLog != null ? supportChatLog.Text : string.Empty,
                    supportChatLog != null ? new ChatLogPathSpec()
                    {
                        path = supportChatLog.Path != null ? supportChatLog.Path : string.Empty,
                        randomPath = supportChatLog.Path == null
                    } : null,
                    null
                )
            });
        }

        private TemplateOption CreateTemplateOption(
            string keyPrefix,
            string group,
            string text,
            List<EvidenceFunction> evidences,
            string emailTitleText,
            FileOptionSpec? fileSpec,
            ChatLogPathSpec? chatLogPathSpec,
            bool generateSupportChatLog,
            string supportChatLogText,
            ChatLogPathSpec? supportChatLogPathSpec,
            CrossInfoSpec? crossInfoSpec)
        {
            var textId = RegisterTranslation($"{keyPrefix}.text", text);
            var emailTitleTextId = string.IsNullOrEmpty(emailTitleText) ? "" : RegisterTranslation($"{keyPrefix}.email_title", emailTitleText);
            var supportChatLogTextId = string.IsNullOrEmpty(supportChatLogText)
                ? ""
                : RegisterTranslation($"{keyPrefix}.support_chat_log", supportChatLogText);

            return new TemplateOption
            {
                group = group,
                textId = textId,
                evidence = evidences,
                emailTitleTextId = emailTitleTextId,
                fileSpec = fileSpec ?? new FileOptionSpec(),
                chatLogPathSpec = chatLogPathSpec ?? new ChatLogPathSpec(),
                generateSupportChatLog = generateSupportChatLog,
                supportChatLogTextId = supportChatLogTextId,
                supportChatLogPathSpec = supportChatLogPathSpec ?? new ChatLogPathSpec(),
                crossInfoSpec = crossInfoSpec
            };
        }

        /// <summary>
        /// Registers a procedural mission-specific translation value.
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
        /// Registers all translations declared by this procedural mission.
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
