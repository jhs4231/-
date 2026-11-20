using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Giant Cleaver: healing grants +75% physical attack for 10 seconds.
    /// Relies on the explicit Health.onHealed delegate and Character.stat attachment.
    /// Does not stack or refresh from damage. Overheal without actual health gained does not activate.
    /// </summary>
    [Serializable]
    public sealed class HealTriggeredPhysicalBuff : Ability, ICloneable
    {
        [SerializeField] public float duration = 10f;
        [SerializeField] public double physicalAttackBonus = 0.75;

        public override IAbilityInstance CreateInstance(Character owner)
        {
            return new Instance(owner, this);
        }

        public object Clone()
        {
            return new HealTriggeredPhysicalBuff
            {
                duration = duration,
                physicalAttackBonus = physicalAttackBonus,
                _defaultIcon = _defaultIcon
            };
        }

        public sealed class Instance : AbilityInstance<HealTriggeredPhysicalBuff>
        {
            private Stat.Values stats;
            private float remaining;

            public Instance(Character owner, HealTriggeredPhysicalBuff ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                stats = new Stat.Values(new[]
                {
                    new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.PhysicalAttackDamage, 0.0)
                });
                owner.stat.AttachValues(stats);
                owner.health.onHealed += OnHealed;
                UpdateBonus();
            }

            public override void OnDetach()
            {
                owner.health.onHealed -= OnHealed;
                if (stats != null) owner.stat.DetachValues(stats);
                stats = null;
                remaining = 0;
            }

            private void OnHealed(double healed, double overhealed)
            {
                if (healed <= 0) return;
                remaining = ability.duration;
                UpdateBonus();
            }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                if (remaining <= 0) return;
                remaining = Math.Max(0, remaining - Math.Max(0, deltaTime));
                if (remaining <= 0) UpdateBonus();
            }

            private void UpdateBonus()
            {
                if (stats == null) return;
                double desired = remaining > 0 ? ability.physicalAttackBonus : 0;
                if (Math.Abs(stats.values[0].value - desired) < 0.0000001) return;
                stats.values[0].value = desired;
                owner.stat.SetNeedUpdate();
            }
        }
    }

    // The name follows the community project's reflection convention:
    // Ability type + 'Component'. AbilityComponent<T> will instantiate the ability.
    public sealed class HealTriggeredPhysicalBuffComponent : AbilityComponent<HealTriggeredPhysicalBuff>
    {
        internal void Configure(HealTriggeredPhysicalBuff buff) { _ability = buff; }
    }
}
