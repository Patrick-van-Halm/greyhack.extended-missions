namespace ExtendedMissions.Registries
{
    internal sealed class MissionConditionRegistry : Registry
    {
        public static readonly MissionConditionRegistry Instance = new MissionConditionRegistry();

        public override int RangeStart => 1001;

        public override string Name => "MissionConditionRegistry";
    }
}
