using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ExtendedMissions.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExtendedMissions.Registries
{
    internal abstract class Registry
    {
        private Dictionary<string, int> entries = new Dictionary<string, int>();
        private readonly string registryFilePath;
        private readonly string invalidRegistryFilePath;

        public abstract int RangeStart { get; }
        public abstract string Name { get; }

        public Registry()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("Registry name is required.");
            }

            registryFilePath = Path.Combine(Paths.ConfigPath, $"{Plugin.PluginName.Replace(" ", "_")}.{Name}.json");
            invalidRegistryFilePath = Path.Combine(Paths.ConfigPath,  $"{Plugin.PluginName.Replace(" ", "_")}.{Name}.invalid.json");
        }

        public int Reserve(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key is required.");
            if (RangeStart < 0) throw new ArgumentOutOfRangeException(nameof(RangeStart));
            EnsureRegistryLoaded();

            if (entries!.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var id = FindNextFreeId();
            entries.Add(key, id);
            Save();
            return id;
        }

        private int FindNextFreeId()
        {
            for (var candidate = RangeStart; candidate <= int.MaxValue; candidate++)
            {
                if (!entries!.ContainsValue(candidate))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                $"[{Name}] Registration full");
        }

        private void EnsureRegistryLoaded()
        {
            if (entries != null) return;

            if (!File.Exists(registryFilePath))
            {
                entries = new Dictionary<string, int>();
                return;
            }

            try
            {
                var content = File.ReadAllText(registryFilePath);
                var root = JsonConvert.DeserializeObject<JObject>(content);
                entries = root?["entries"]?.ToObject<Dictionary<string, int>>() ?? new Dictionary<string, int>();
            }
            catch (Exception exception)
            {
                PreserveInvalidRegistryFile();
                DebugLogger.Log($"[{Name}] Failed to load registry '{registryFilePath}': {exception}");
                throw new InvalidOperationException(
                    $"[{Name}]: '{registryFilePath}' is invalid. A copy was preserved at '{invalidRegistryFilePath}'.",
                    exception);
            }
        }

        private void PreserveInvalidRegistryFile()
        {
            try
            {
                File.Copy(registryFilePath, invalidRegistryFilePath, true);
            }
            catch (Exception exception)
            {
                DebugLogger.Log($"[{Name}] Failed to preserve invalid registry copy: {exception}");
            }
        }

        private void Save()
        {
            var directory = Path.GetDirectoryName(registryFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var state = new Dictionary<string, object>
            {
                ["entries"] = entries ?? new Dictionary<string, int>()
            };

            File.WriteAllText(registryFilePath, JsonConvert.SerializeObject(state, Formatting.Indented));
        }
    }
}