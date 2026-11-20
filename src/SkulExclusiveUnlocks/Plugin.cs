using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    [BepInPlugin("jhs4231.skul.exclusiveunlocks", "Skul Exclusive Unlock Items (Prototype)", "0.3.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance { get; private set; }
        private UnlockStore store;
        private bool unlockAllForTesting;

        private void Awake()
        {
            Instance = this;
            store = new UnlockStore(Path.Combine(Paths.ConfigPath, "jhs4231.skul.exclusiveunlocks.json"));
            store.Load();
            unlockAllForTesting = Config.Bind("Testing", "UnlockAllItemsForTesting", false,
                "Developer-only: register all exclusive items regardless of Alexander unlock progress.").Value;
            try
            {
                Harmony.CreateAndPatchAll(typeof(CustomItemRegistration));
                Logger.LogWarning("Experimental item registration loaded. Giant Cleaver heal buff added; most combat effects and boss hook are still pending.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Item registration failed. Check game version and dependencies: " + ex);
            }
        }

        // Integration point ONLY. Call from a verified Harmony hook after Alexander is defeated
        // and the actual skull identifier has been verified. This plugin does not call it itself.
        internal void OnVerifiedAlexanderKill(string normalizedSkullId)
        {
            if (string.IsNullOrEmpty(normalizedSkullId)) return;
            string groupId;
            if (!UnlockDefinitions.TryGetGroup(normalizedSkullId, out groupId)) return;
            if (store.Unlock(groupId))
                Logger.LogInfo("Exclusive item group unlocked: " + groupId);
        }

        internal bool IsGroupUnlocked(string groupId) { return store != null && store.IsUnlocked(groupId); }
        internal static bool HasUnlock(string groupId) { return Instance != null && (Instance.unlockAllForTesting || Instance.IsGroupUnlocked(groupId)); }
        internal static void WriteLog(string message) { if (Instance != null) Instance.Logger.LogInfo(message); }
    }

    internal static class UnlockDefinitions
    {
        // These identifiers are project-local placeholders, NOT verified game identifiers.
        private static readonly Dictionary<string, string> GroupByLocalSkullId =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "eternal-werewolf", "leader" },
                { "ghoul", "hunger" },
                { "dark-alchemist", "science" },
                { "destroyer", "violence" },
                { "archlich", "released" },
                { "royal-guard-captain", "swordsman" }
            };

        internal static bool TryGetGroup(string id, out string groupId)
        {
            return GroupByLocalSkullId.TryGetValue(id, out groupId);
        }
    }

    [Serializable]
    internal sealed class UnlockData
    {
        public List<string> groups = new List<string>();
    }

    internal sealed class UnlockStore
    {
        private readonly string path;
        private readonly HashSet<string> unlocked = new HashSet<string>(StringComparer.Ordinal);
        internal UnlockStore(string path) { this.path = path; }

        internal bool IsUnlocked(string groupId) { return unlocked.Contains(groupId); }

        internal void Load()
        {
            if (!File.Exists(path)) return;
            try
            {
                var data = JsonUtility.FromJson<UnlockData>(File.ReadAllText(path));
                if (data != null && data.groups != null)
                    foreach (var id in data.groups) if (!string.IsNullOrEmpty(id)) unlocked.Add(id);
            }
            catch (Exception ex)
            {
                if (Plugin.Instance != null) Plugin.Instance.Logger.LogError("Unlock save load failed: " + ex);
            }
        }

        internal bool Unlock(string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || !unlocked.Add(groupId)) return false;
            Save();
            return true;
        }

        private void Save()
        {
            try
            {
                var data = new UnlockData();
                data.groups.AddRange(unlocked);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string temp = path + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(data, true));
                // Do not destroy prior save if replacement fails.
                if (File.Exists(path)) File.Delete(path);
                File.Move(temp, path);
            }
            catch (Exception ex)
            {
                if (Plugin.Instance != null) Plugin.Instance.Logger.LogError("Unlock save failed: " + ex);
            }
        }
    }
}
