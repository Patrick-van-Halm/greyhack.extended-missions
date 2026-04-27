namespace ExtendedMissions.Registries
{
    internal sealed class ProceduralArchetypesRegistry : Registry
    {
        public static readonly ProceduralArchetypesRegistry Instance = new ProceduralArchetypesRegistry();

        public override int RangeStart => 1001;

        public override string Name => "ProceduralArcheTypes";
    }
}
