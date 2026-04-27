using MissionConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExtendedMissions.Missions
{
    /// <summary>
    /// Contract for custom direct missions accepted from a mission board.
    /// </summary>
    internal interface IExtendedDirectMission : IExtendedMission, ITranslatableMission
    {
        /// <summary>
        /// Optional attachment included in the contract email when the mission is accepted.
        /// </summary>
        FileSystem.Archivo? GetMailAttachment(ActiveMission mission);

        /// <summary>
        /// Creates and stores runtime mission state, returning the contract body sent to the player.
        /// </summary>
        string? AddMission(DirectMission mission, string language, PlayerMissions playerMissions);

        /// <summary>
        /// Validates mission completion from the player's reply and optional attachment.
        /// </summary>
        string? CheckMission(
            ActiveMission mission,
            string message,
            FileSystem.Archivo attachment,
            string language,
            ref bool missionOk);

        /// <summary>
        /// Deserializes custom mission state from JSON when the game loads saved missions.
        /// </summary>
        object? ReadMissionFromJson(int missionTypeId, JObject jObject, JsonSerializer serializer);

        /// <summary>
        /// Returns the direct mission preview registered into the game's mission generator.
        /// </summary>
        DirectMissionPreview MissionConfig();
    }
}
