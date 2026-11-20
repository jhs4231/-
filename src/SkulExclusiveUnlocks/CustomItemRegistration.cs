using System;
using System.Collections.Generic;
using Characters.Abilities;
using System.Reflection;
using Characters;
using Characters.Abilities.CharacterStat;
using Characters.Gear.Items;
using Characters.Gear.Synergy.Inscriptions;
using GameResources;
using HarmonyLib;
using Level;
using UnityEngine;
using static Characters.Damage;
using static Characters.CharacterStatus;

namespace SkulExclusiveUnlocks
{
    // Uses the same public Skul extension points used by community custom item mods.
    // Requires Assembly-CSharp publicization for access to game internal fields.
    internal static class CustomItemRegistration
    {
        internal static readonly List<ExclusiveItemReference> Items = BuildItems();
        private static readonly Dictionary<ItemRequest, ExclusiveItemReference> Requests =
            new Dictionary<ItemRequest, ExclusiveItemReference>();
        private static readonly Dictionary<GearRequest, ExclusiveItemReference> GearRequests =
            new Dictionary<GearRequest, ExclusiveItemReference>();
        private static readonly Dictionary<string, string> Translations = new Dictionary<string, string>();

        private static List<ExclusiveItemReference> BuildItems()
        {
            var result = new List<ExclusiveItemReference>();
            Add(result, "SilverFur", "은빛 모피", "피격 시 데미지를 입지 않습니다. (쿨타임: 20초) 이동속도가 40% 증가합니다.",
                Rarity.Rare, Inscription.Key.Soar, Inscription.Key.Misfortune, "leader",
                new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed, 0.40));
            Add(result, "WildMark", "야생의 표식", "추가 이동속도 1%당 물리 및 마법공격력이 1% 증가합니다. 이동속도가 90% 증가합니다.",
                Rarity.Legendary, Inscription.Key.Chase, Inscription.Key.Brawl, "leader",
                new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed, 0.90));
            Add(result, "GiantCleaver", "거인의 식칼", "체력 회복 시 10초간 물리공격력이 75% 증가합니다.",
                Rarity.Rare, Inscription.Key.Revenge, Inscription.Key.Arms, "hunger");
            Add(result, "FleshCore", "살점 핵", "최대 체력 100 증가, 받는 피해 50% 감소, 사망 시 한 번 최대 체력으로 부활.",
                Rarity.Legendary, Inscription.Key.Revenge, Inscription.Key.Relic, "hunger",
                new Stat.Value(Stat.Category.Constant, Stat.Kind.Health, 100));
            Add(result, "UniversalFlask", "만능 플라스크", "모든 공격이 15% 확률로 무작위 상태이상을 부여합니다.",
                Rarity.Rare, Inscription.Key.Brave, Inscription.Key.Wisdom, "science");
            Add(result, "PerfectStone", "완벽한 마석", "원본 이미지에 효과 정보가 없습니다. (효과 미구현)",
                Rarity.Legendary, Inscription.Key.Antique, Inscription.Key.None, "science");
            Add(result, "MenacingMask", "험악한 가면", "교대 쿨다운 속도 20% 증가. 교대 시 적을 6초간 둔화.",
                Rarity.Rare, Inscription.Key.Execution, Inscription.Key.Brave, "violence",
                new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.SwapCooldownSpeed, 0.20));
            Add(result, "DestroyerBlade", "파괴자의 검", "스킬 시전 중 물리공격력과 치명타 확률 지속 증가 (최대 100%, 35%).",
                Rarity.Unique, Inscription.Key.Execution, Inscription.Key.Arms, "violence");
            Add(result, "ThirdEye", "세 번째 눈", "마법공격력 50% 증가. 5초마다 전방의 적에게 마법피해 및 기절.",
                Rarity.Rare, Inscription.Key.Dizziness, Inscription.Key.Misfortune, "released",
                new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MagicAttackDamage, 0.50));
            Add(result, "Chains", "쇠사슬", "스킬 사용 시 마법피해를 주는 사슬이 나오며 스킬 쿨타임에 따라 증가.",
                Rarity.Legendary, Inscription.Key.Wisdom, Inscription.Key.Manatech, "released");
            Add(result, "RubyHeart", "루비 심장", "교대 후 3초간 모든 공격에 상처. 출혈 처치 3명마다 체력 1 회복.",
                Rarity.Unique, Inscription.Key.ExcessiveBleeding, Inscription.Key.Antique, "swordsman");
            Add(result, "RoyalInsignia", "왕가의 휘장", "출혈 부여 시 추가 물리피해가 최대 8회 발동.",
                Rarity.Legendary, Inscription.Key.ExcessiveBleeding, Inscription.Key.Heirloom, "swordsman");
            return result;
        }

        private static void Add(List<ExclusiveItemReference> items, string key, string title, string description,
            Rarity rarity, Inscription.Key first, Inscription.Key second, string group, params Stat.Value[] values)
        {
            var item = new ExclusiveItemReference
            {
                name = key,
                title = title,
                description = description,
                unlockGroup = group,
                rarity = rarity,
                prefabKeyword1 = first,
                prefabKeyword2 = second,
                stats = new Stat.Values(values),
            };
            if (key == "GiantCleaver")
                item.abilities = new Ability[] { new HealTriggeredPhysicalBuff { duration = 10f, physicalAttackBonus = 0.75 } };
            if (key == "UniversalFlask")
            {
                // 15% total, not 15% per status. Each hit chooses one random status.
                // Runtime attack hook still needs verification to avoid multi-hit overprocs.
                item.combatLogicRequiresIntegration = true;
            }
            items.Add(item);
            Translations[item.displayNameKey] = title;
            Translations["item/Custom-" + key + "/description"] = description;
            Translations["item/Custom-" + key + "/lore"] = "스컬 전용 아이템";
        }

        // Only already-unlocked items are included at database initialization.
        // A newly defeated Alexander unlock is available upon next game startup.
        [HarmonyPatch(typeof(GearResource), "Initialize")]
        [HarmonyPrefix]
        private static void Register(ref GearResource __instance)
        {
            var available = Items.FindAll(x => Plugin.HasUnlock(x.unlockGroup)).ToArray();
            var oldCount = __instance._items.Length;
            Array.Resize(ref __instance._items, oldCount + available.Length);
            Array.Copy(available, 0, __instance._items, oldCount, available.Length);
            Plugin.WriteLog("Registered unlocked Skul items: " + available.Length);
        }

        [HarmonyPatch(typeof(ItemReference), "LoadAsync")]
        [HarmonyPostfix]
        private static void TrackItemRequest(ItemReference __instance, ItemRequest __result)
        {
            if (__result != null && __instance is ExclusiveItemReference custom)
                Requests[__result] = custom;
        }

        [HarmonyPatch(typeof(GearReference), "LoadAsync")]
        [HarmonyPostfix]
        private static void TrackGearRequest(GearReference __instance, GearRequest __result)
        {
            if (__result != null && __instance is ExclusiveItemReference custom)
                GearRequests[__result] = custom;
        }

        [HarmonyPatch(typeof(LevelManager), "DropItem", new Type[] { typeof(ItemRequest), typeof(Vector3) })]
        [HarmonyPrefix]
        private static bool ReplaceItem(LevelManager __instance, ItemRequest gearRequest, Vector3 position, ref Item __result)
        {
            if (!Requests.TryGetValue(gearRequest, out var custom)) return true;
            Requests.Remove(gearRequest);
            if (gearRequest.asset == null) return true;
            var prefab = gearRequest.asset.gameObject.GetComponent<Item>();
            if (prefab == null) return true;
            __result = __instance.DropItem(custom.Materialize(prefab), position);
            return false;
        }

        [HarmonyPatch(typeof(LevelManager), "DropGear", new Type[] { typeof(GearRequest), typeof(Vector3) })]
        [HarmonyPrefix]
        private static bool ReplaceGear(LevelManager __instance, GearRequest gearRequest, Vector3 position,
            ref Characters.Gear.Gear __result)
        {
            if (!GearRequests.TryGetValue(gearRequest, out var custom)) return true;
            GearRequests.Remove(gearRequest);
            if (gearRequest.asset == null) return true;
            var prefab = gearRequest.asset.gameObject.GetComponent<Item>();
            if (prefab == null) return true;
            __result = __instance.DropItem(custom.Materialize(prefab), position);
            return false;
        }

        [HarmonyPatch(typeof(GameResources.Localization), "GetLocalizedString", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        private static bool Translate(string key, ref string __result)
        {
            if (!Translations.TryGetValue(key, out var value)) return true;
            __result = value;
            return false;
        }
    }

    [Serializable]
    internal sealed class ExclusiveItemReference : ItemReference
    {
        internal string title;
        internal string description;
        internal string unlockGroup;
        internal bool combatLogicRequiresIntegration;
        internal Stat.Values stats;
        internal Ability[] abilities;
        private Item cached;
        private static GameObject cacheRoot;

        internal ExclusiveItemReference()
        {
            obtainable = true;
            needUnlock = false; // Unlock gating is done by database inclusion, not game's built-in lock.
            path = "Assets/Gear/Items/BasicCarleonSword.prefab";
        }

        public new string name
        {
            get { return base.name; }
            set
            {
                base.name = "Custom-" + value;
                guid = "skul-exclusive://" + value;
                displayNameKey = "item/Custom-" + value + "/name";
            }
        }

        internal Item Materialize(Item template)
        {
            if (cached != null) return cached;
            if (cacheRoot == null)
            {
                cacheRoot = new GameObject("ExclusiveItemTemplates");
                cacheRoot.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(cacheRoot);
            }
            cached = UnityEngine.Object.Instantiate(template, cacheRoot.transform);
            cached.gameObject.name = name;
            cached.keyword1 = prefabKeyword1;
            cached.keyword2 = prefabKeyword2;
            cached._stat = stats;
            cached._rarity = rarity;
            if (abilities != null && abilities.Length > 0)
            {
                // Mirrors the verified item ability attachment flow in VoiceOfTheCommunity.
                // Avoid shared mutable state: cloned item prefab keeps the ability component on its own template.
                var abilityContainer = new GameObject("ExclusiveAbilities");
                abilityContainer.transform.SetParent(cached.transform, false);
                var attacher = cached._abilityAttacher = new AbilityAttacher();
                attacher._container = abilityContainer;
                attacher._components = new AbilityAttacher[abilities.Length];
                for (int i = 0; i < abilities.Length; i++)
                {
                    var node = new GameObject("[" + i + "]", typeof(HealTriggeredPhysicalBuffComponent));
                    node.transform.SetParent(abilityContainer.transform, false);
                    var component = node.GetComponent<HealTriggeredPhysicalBuffComponent>();
                    component.Configure((HealTriggeredPhysicalBuff)abilities[i]);
                    attacher._components[i] = component;
                }
            }
            // Placeholder visual assets until permission-cleared 32px sprites are available.
            UnityEngine.Object.DontDestroyOnLoad(cached);
            return cached;
        }
    }
}
