using System;
using System.Collections.Generic;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Menacing Mask: on player swap, apply temporary movement speed reduction to alive enemies.
    /// The original idea specifies SIX seconds but NO slow magnitude.
    /// slowPercent=0.30 is a temporary tuning placeholder, not source-verified.
    /// </summary>
    [Serializable]
    public sealed class MenacingMaskSlow : Ability, ICloneable
    {
        [SerializeField] public float duration = 6f;
        [SerializeField] public double slowPercent = 0.30; // PLACEHOLDER, unverified by source.

        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone()
        {
            return new MenacingMaskSlow
            {
                duration = duration, slowPercent = slowPercent, _defaultIcon = _defaultIcon
            };
        }

        private sealed class SlowEntry
        {
            public Character target;
            public Stat.Values values;
            public float remaining;
        }

        public sealed class Instance : AbilityInstance<MenacingMaskSlow>
        {
            private readonly List<SlowEntry> affected = new List<SlowEntry>();
            public Instance(Character owner, MenacingMaskSlow ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                owner.playerComponents.inventory.weapon.onSwap += OnSwap;
            }

            public override void OnDetach()
            {
                owner.playerComponents.inventory.weapon.onSwap -= OnSwap;
                for (int i = affected.Count - 1; i >= 0; i--) Clear(affected[i]);
                affected.Clear();
            }

            private void OnSwap()
            {
                if (ability.slowPercent <= 0) return;
                // Sweeps only on swap; avoids per-frame global character searches.
                var characters = UnityEngine.Object.FindObjectsOfType<Character>();
                foreach (var target in characters)
                {
                    if (target == null || target == owner || target.stat == null ||
                        target.health == null || target.health.dead || !IsEnemy(target.type))
                        continue;
                    SlowEntry old = affected.Find(x => x.target == target);
                    if (old != null)
                    {
                        old.remaining = ability.duration;
                        continue;
                    }
                    var values = new Stat.Values(new[]
                    {
                        new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed,
                            -Math.Max(0, ability.slowPercent))
                    });
                    target.stat.AttachValues(values);
                    target.stat.SetNeedUpdate();
                    affected.Add(new SlowEntry { target = target, values = values, remaining = ability.duration });
                }
            }

            private static bool IsEnemy(Character.Type type)
            {
                return type == Character.Type.TrashMob || type == Character.Type.Named ||
                       type == Character.Type.Adventurer || type == Character.Type.Boss ||
                       type == Character.Type.Summoned;
            }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                for (int i = affected.Count - 1; i >= 0; i--)
                {
                    var entry = affected[i];
                    entry.remaining -= Math.Max(0, deltaTime);
                    if (entry.remaining > 0 && entry.target != null && entry.target.health != null &&
                        !entry.target.health.dead) continue;
                    Clear(entry);
                    affected.RemoveAt(i);
                }
            }

            private static void Clear(SlowEntry entry)
            {
                if (entry.target == null || entry.target.stat == null) return;
                entry.target.stat.DetachValues(entry.values);
                entry.target.stat.SetNeedUpdate();
            }
        }
    }

    public sealed class MenacingMaskSlowComponent : AbilityComponent<MenacingMaskSlow>
    {
        internal void Configure(MenacingMaskSlow value) { _ability = value; }
    }
}
