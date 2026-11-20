using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Ruby Heart: all eligible attacks apply Wound during the three seconds after swap.
    /// Bleed kills heal one HP after each three kills, using the game bleed damage key.
    /// </summary>
    [Serializable]
    public sealed class RubyHeartWound : Ability, ICloneable
    {
        [SerializeField] public float windowSeconds = 3f;
        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone() { return new RubyHeartWound { windowSeconds = windowSeconds, _defaultIcon = _defaultIcon }; }

        public sealed class Instance : AbilityInstance<RubyHeartWound>
        {
            private float remaining;
            private int bleedKills;
            public Instance(Character owner, RubyHeartWound ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                remaining = 0;
                owner.playerComponents.inventory.weapon.onSwap += OnSwap;
                owner.onGiveDamage.Add(0, OnHit);
                owner.onKilled += OnKilledEnemy;
            }

            public override void OnDetach()
            {
                owner.playerComponents.inventory.weapon.onSwap -= OnSwap;
                owner.onGiveDamage.Remove(OnHit);
                owner.onKilled -= OnKilledEnemy;
            }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                remaining = Mathf.Max(0, remaining - Mathf.Max(0, deltaTime));
            }

            private void OnSwap() { remaining = ability.windowSeconds; }

            private void OnKilledEnemy(ITarget target, ref Damage damage)
            {
                if (target == null || target.character == null ||
                    target.character.type == Character.Type.Dummy) return;
                if (damage.key != CharacterStatus.AttackKeyBleed) return;
                bleedKills++;
                if (bleedKills < 3) return;
                bleedKills = 0;
                owner.health.Heal(1.0);
            }

            private bool OnHit(ITarget target, ref Damage damage)
            {
                if (remaining <= 0 || target == null || target.character == null ||
                    target.character.status == null || damage.@null || damage.amount <= 0 ||
                    damage.motionType == Damage.MotionType.Status) return false;
                target.character.status.ApplyWound(owner);
                return false;
            }
        }
    }

    public sealed class RubyHeartWoundComponent : AbilityComponent<RubyHeartWound>
    {
        internal void Configure(RubyHeartWound value) { _ability = value; }
    }
}
