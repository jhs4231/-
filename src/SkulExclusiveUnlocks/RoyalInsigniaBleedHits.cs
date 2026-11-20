using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Royal Insignia: when an enemy bleeds, launch up to eight extra physical hits.
    /// Original screenshot does not specify damage per hit: baseDamage=1 is a
    /// deliberately small *temporary* placeholder, NOT a sourced balance value.
    /// </summary>
    [Serializable]
    public sealed class RoyalInsigniaBleedHits : Ability, ICloneable
    {
        [SerializeField] public int extraHitCount = 8;
        [SerializeField] public double baseDamage = 1.0; // UNSPECIFIED IN SOURCE. Replace after verification.

        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone()
        {
            return new RoyalInsigniaBleedHits
            {
                extraHitCount = extraHitCount, baseDamage = baseDamage, _defaultIcon = _defaultIcon
            };
        }

        public sealed class Instance : AbilityInstance<RoyalInsigniaBleedHits>
        {
            private bool applying;
            public Instance(Character owner, RoyalInsigniaBleedHits ability) : base(owner, ability) {}
            public override void OnAttach()
            {
                owner.status.onApplyBleed += OnBleedApplied;
            }
            public override void OnDetach()
            {
                owner.status.onApplyBleed -= OnBleedApplied;
            }

            private void OnBleedApplied(Character attacker, Character target)
            {
                if (applying || target == null || attacker != owner || ability.baseDamage <= 0) return;
                applying = true;
                try
                {
                    int hits = Mathf.Clamp(ability.extraHitCount, 0, 8);
                    for (int i = 0; i < hits; i++)
                    {
                        if (target.health == null || target.health.dead) break;
                        var hit = new Damage(owner, ability.baseDamage, target.transform.position,
                            Damage.Attribute.Physical, Damage.AttackType.Additional,
                            Damage.MotionType.Item, "SkulExclusive-RoyalInsignia");
                        owner.Attack(target, ref hit);
                    }
                }
                finally { applying = false; }
            }
        }
    }

    public sealed class RoyalInsigniaBleedHitsComponent : AbilityComponent<RoyalInsigniaBleedHits>
    {
        internal void Configure(RoyalInsigniaBleedHits value) { _ability = value; }
    }
}
