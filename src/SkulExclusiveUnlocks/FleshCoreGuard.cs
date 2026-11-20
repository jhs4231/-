using System;
using Characters;
using Characters.Abilities;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Flesh Core: halves non-null incoming damage while equipped.
    /// Health +100 is supplied by the item's regular Stat.Values.
    /// The once-per-run revival requires a separate verified death interception hook.
    /// </summary>
    [Serializable]
    public sealed class FleshCoreGuard : Ability, ICloneable
    {
        public override IAbilityInstance CreateInstance(Character owner)
        {
            return new Instance(owner, this);
        }

        public object Clone()
        {
            return new FleshCoreGuard { _defaultIcon = _defaultIcon };
        }

        public sealed class Instance : AbilityInstance<FleshCoreGuard>
        {
            public Instance(Character owner, FleshCoreGuard ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                owner.health.onTakeDamage.Add(-1000, new TakeDamageDelegate(ReduceDamage));
            }

            public override void OnDetach()
            {
                owner.health.onTakeDamage.Remove(new TakeDamageDelegate(ReduceDamage));
            }

            private bool ReduceDamage(ref Damage damage)
            {
                if (damage.@null || damage.amount <= 0) return false;
                if (owner.invulnerable.value) return false;
                damage.amount *= 0.5;
                return false;
            }
        }
    }

    public sealed class FleshCoreGuardComponent : AbilityComponent<FleshCoreGuard>
    {
        internal void Configure(FleshCoreGuard ability) { _ability = ability; }
    }
}
