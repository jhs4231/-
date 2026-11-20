using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>Silver Fur: negate one eligible hit every 20 seconds.</summary>
    [Serializable]
    public sealed class SilverFurShield : Ability, ICloneable
    {
        [SerializeField] public float cooldown = 20f;
        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone() { return new SilverFurShield { cooldown = cooldown, _defaultIcon = _defaultIcon }; }

        public sealed class Instance : AbilityInstance<SilverFurShield>
        {
            private float remaining;
            public Instance(Character owner, SilverFurShield ability) : base(owner, ability) {}
            public override void OnAttach()
            {
                remaining = 0;
                owner.health.onTakeDamage.Add(1000, new TakeDamageDelegate(PreventHit));
            }
            public override void OnDetach()
            {
                owner.health.onTakeDamage.Remove(new TakeDamageDelegate(PreventHit));
            }
            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                remaining = Mathf.Max(0, remaining - Mathf.Max(0, deltaTime));
            }
            private bool PreventHit(ref Damage damage)
            {
                if (remaining > 0 || owner.invulnerable.value || damage.@null ||
                    damage.amount < 1 || damage.attackType == Damage.AttackType.None) return false;
                damage.@null = true;
                remaining = ability.cooldown;
                return false;
            }
        }
    }

    public sealed class SilverFurShieldComponent : AbilityComponent<SilverFurShield>
    {
        internal void Configure(SilverFurShield value) { _ability = value; }
    }
}
