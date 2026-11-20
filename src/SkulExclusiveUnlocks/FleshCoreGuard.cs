using System;
using System.Collections.Generic;
using Characters;
using Characters.Abilities;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Flesh Core: halves non-null incoming damage while equipped.
    /// Health +100 is supplied by the item's regular Stat.Values.
    /// Once-per-run resurrection is guarded by a static owner identity set.
    /// </summary>
    [Serializable]
    public sealed class FleshCoreGuard : Ability, ICloneable
    {
        private static readonly HashSet<int> RevivedOwners = new HashSet<int>();
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
                owner.health.onDie += TryRevive;
            }

            public override void OnDetach()
            {
                owner.health.onTakeDamage.Remove(new TakeDamageDelegate(ReduceDamage));
                owner.health.onDie -= TryRevive;
            }

            private void TryRevive()
            {
                int id = owner.GetInstanceID();
                if (!RevivedOwners.Add(id)) return;
                owner.health.ResetToMaximumHealth();
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
