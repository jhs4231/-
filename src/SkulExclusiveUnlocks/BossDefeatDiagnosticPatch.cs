using Characters;
using HarmonyLib;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Diagnostic hook for discovering exact Alexander boss Character.Key and
    /// upgraded skull prefab names. Does NOT grant unlocks unless explicit,
    /// externally verified keys are configured by the user.
    /// </summary>
    [HarmonyPatch(typeof(Character), "Awake")]
    internal static class BossDefeatDiagnosticPatch
    {
        [HarmonyPostfix]
        private static void OnCharacterAwake(Character __instance)
        {
            if (__instance == null || __instance.type != Character.Type.Player) return;
            __instance.onKilled += OnPlayerKilledCharacter;
        }

        private static void OnPlayerKilledCharacter(ITarget target, ref Damage damage)
        {
            if (target == null || target.character == null ||
                target.character.type != Character.Type.Boss) return;

            var player = Plugin.Instance;
            if (player == null) return;
            var weapon = player.CurrentWeaponName();
            int key = (int)target.character.key;
            Plugin.WriteLog("Boss kill diagnostics: bossKey=" + key +
                ", bossObject=" + target.character.name + ", playerWeapon=" + weapon);
            player.TryUnlockFromVerifiedBossAndWeapon(key, weapon);
        }
    }
}
