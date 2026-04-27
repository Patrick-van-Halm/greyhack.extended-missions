using System.Linq;
using System.Reflection;
using ExtendedMissions.Missions;
using ExtendedMissions.Registries;

/// <summary>
/// Base type for all extended mission definitions.
/// </summary>
/// <typeparam name="TInstance">The concrete singleton mission type.</typeparam>
public abstract class ExtendedMission<TInstance> : IExtendedMission
    where TInstance : ExtendedMission<TInstance>, new()
{
    /// <summary>
    /// Gets the reserved extended mission type id for this mission.
    /// </summary>
    public int MissionTypeId { get; }

    /// <summary>
    /// Gets the game system this mission registers with.
    /// </summary>
    public abstract MissionRegistrationTarget RegistrationTarget { get; }

    /// <summary>
    /// Gets the singleton instance used by the registry and runtime patches.
    /// </summary>
    public static TInstance Instance { get; } = new TInstance();

    /// <summary>
    /// Gets the stable key used for mission ids and translation keys.
    /// </summary>
    protected static string StaticKey
    {
        get
        {
            var pluginType = typeof(TInstance).Assembly
            .GetTypes()
            .FirstOrDefault(type => type.GetCustomAttributes()
                .Any(attribute => attribute.GetType().FullName == "BepInEx.BepInPlugin"));
            var plugin = pluginType?.GetCustomAttributes()
                .FirstOrDefault(attribute => attribute.GetType().FullName == "BepInEx.BepInPlugin");
            var pluginGuid = plugin?.GetType().GetProperty("GUID")?.GetValue(plugin) as string;
            if (string.IsNullOrWhiteSpace(pluginGuid))
            {
                return typeof(TInstance).FullName ?? typeof(TInstance).Name;
            }

            return $"{pluginGuid}.{typeof(TInstance).Name}";
        }
    }

    /// <summary>
    /// Initializes a new mission definition and reserves its mission type id.
    /// </summary>
    public ExtendedMission()
    {
        MissionTypeId = MissionTypeRegistry.Instance.Reserve(StaticKey);
    }
}
