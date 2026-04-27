using MissionConfig;

namespace ExtendedMissions.Utils
{
    /// <summary>
    /// Conversion helpers between vanilla enum values and the integer ids used by extended missions.
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>
        /// Converts a vanilla condition enum value into its raw integer id.
        /// </summary>
        /// <param name="condition">The vanilla condition enum value.</param>
        /// <returns>The raw integer condition id.</returns>
        public static int ToExtendedConditionId(this ConditionTypeDirect condition) => (int)condition;

        /// <summary>
        /// Converts an integer condition id back into the vanilla condition enum.
        /// </summary>
        /// <param name="conditionId">The raw integer condition id.</param>
        /// <returns>The vanilla condition enum value.</returns>
        public static ConditionTypeDirect ToBaseCondition(this int conditionId) => (ConditionTypeDirect)conditionId;

        /// <summary>
        /// Converts an integer mission type id into the vanilla mission type enum.
        /// </summary>
        /// <param name="missionTypeId">The raw integer mission type id.</param>
        /// <returns>The vanilla mission type enum value.</returns>
        public static TypeMissionDirect ToBaseType(this int missionTypeId) => (TypeMissionDirect)missionTypeId;

        /// <summary>
        /// Converts a vanilla mission type enum into its raw integer id.
        /// </summary>
        /// <param name="missionType">The vanilla mission type enum value.</param>
        /// <returns>The raw integer mission type id.</returns>
        public static int ToMissionTypeId(this TypeMissionDirect missionType) => (int)missionType;
    }
}
