namespace ExtendedMissions.Registries
{
    internal sealed class MissionTypeRegistry : Registry
    {
        public static readonly MissionTypeRegistry Instance = new MissionTypeRegistry();

        public override int RangeStart => 1001;

        public override string Name => "MissionTypeRegistry";
    }
}
