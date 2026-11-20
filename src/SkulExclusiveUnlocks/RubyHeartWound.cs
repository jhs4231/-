using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Ruby Heart: all eligible attacks apply Wound during the three seconds after swap.
    /// Bleed-kill-based healing is not part of this class until verified bleed attribution is available.
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
            public Instance(Character owner, RubyHeartWound ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                remaining = 0;
                owner.playerComponents.inventory.weapon.onSwap += OnSwap;
                owner.onGiveDamage.Add(0, OnHit);
            }

            public override void OnDetach()
            {
                owner.playerComponents.inventory.weapon.onSwap -= OnSwap;
                owner.onGiveDamage.Remove(OnHit);
            }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                remaining = Mathf.Max(0, remaining - Mathf.Max(0, deltaTime));
            }

            private void OnSwap() { remaining = ability.windowSeconds; }

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
