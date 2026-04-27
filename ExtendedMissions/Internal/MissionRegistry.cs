using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtendedMissions.Missions;

namespace ExtendedMissions.Registries
{
    /// <summary>
    /// Describes the registration buckets used by the mod when exposing missions to the game.
    /// </summary>
    public enum MissionRegistrationTarget
    {
        Hackshop,
        Procedural,
        Police,
        GreyHat,
        BlackHat,
        WhiteHat,
        Hidden
    }

    /// <summary>
    /// Central registry for all public mission types exposed by the mod.
    /// </summary>
    internal static class MissionRegistry
    {
        private static readonly Dictionary<int, IExtendedMission> MissionsByType = new Dictionary<int, IExtendedMission>();
        private static readonly HashSet<Type> DiscoveredMissionTypes = new HashSet<Type>();

        /// <summary>
        /// Gets all registered missions.
        /// </summary>
        internal static IReadOnlyCollection<IExtendedMission> All
        {
            get
            {
                EnsureDiscovered();
                return MissionsByType.Values.ToArray();
            }
        }

        /// <summary>
        /// Scans loaded assemblies for mission implementations that have not yet been registered.
        /// </summary>
        internal static void EnsureDiscovered()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in GetLoadableMissionTypes(assembly))
                {
                    if (!DiscoveredMissionTypes.Add(type))
                    {
                        continue;
                    }

                    var mission = ResolveMissionInstance(type);
                    if (mission == null)
                    {
                        continue;
                    }

                    Register(mission);
                }
            }
        }

        /// <summary>
        /// Returns all registered missions assignable to <typeparamref name="TMission" />.
        /// </summary>
        internal static IReadOnlyCollection<TMission> GetAll<TMission>() where TMission : class, IExtendedMission
        {
            EnsureDiscovered();
            return MissionsByType.Values
                .OfType<TMission>()
                .ToArray();
        }

        /// <summary>
        /// Returns the registered missions for the requested target assignable to <typeparamref name="TMission" />.
        /// </summary>
        internal static IReadOnlyCollection<TMission> GetByTarget<TMission>(MissionRegistrationTarget target)
            where TMission : class, IExtendedMission
        {
            EnsureDiscovered();
            if (target == MissionRegistrationTarget.Procedural)
            {
                return MissionsByType.Values
                    .OfType<IExtendedProceduralMission>()
                    .OfType<TMission>()
                    .ToArray();
            }

            return MissionsByType.Values
                .OfType<TMission>()
                .Where(mission => mission.RegistrationTarget == target)
                .ToArray();
        }

        /// <summary>
        /// Registers a mission implementation in the main mission registry.
        /// </summary>
        internal static void Register(IExtendedMission mission)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));

            if (MissionsByType.TryGetValue(mission.MissionTypeId, out var existing))
            {
                if (ReferenceEquals(existing, mission))
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"Mission type id '{mission.MissionTypeId}' is already registered by '{existing.GetType().FullName}'.");
            }

            MissionsByType.Add(mission.MissionTypeId, mission);
        }

        /// <summary>
        /// Tries to resolve a mission registration by mission type id and cast it to <typeparamref name="TMission" />.
        /// </summary>
        internal static bool TryGet<TMission>(int missionTypeId, out TMission? mission)
            where TMission : class, IExtendedMission
        {
            EnsureDiscovered();
            if (MissionsByType.TryGetValue(missionTypeId, out var registeredMission) &&
                registeredMission is TMission typedMission)
            {
                mission = typedMission;
                return true;
            }

            mission = null;
            return false;
        }

        internal static IEnumerable<Type> GetLoadableMissionTypes(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type != null).Cast<Type>().ToArray();
            }

            return types.Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                !type.ContainsGenericParameters &&
                typeof(IExtendedMission).IsAssignableFrom(type));
        }

        private static IExtendedMission? ResolveMissionInstance(Type type)
        {
            var instanceProperty = type.GetProperty(
                "Instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (instanceProperty != null &&
                typeof(IExtendedMission).IsAssignableFrom(instanceProperty.PropertyType))
            {
                return instanceProperty.GetValue(null) as IExtendedMission;
            }

            return Activator.CreateInstance(type) as IExtendedMission;
        }
    }
}
