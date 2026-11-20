using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Wild Mark: adds physical and magic attack percentage points equal to bonus movement
    /// speed percentage points. The item's 90% movement speed is declared separately in stats.
    /// </summary>
    [Serializable]
    public sealed class WildMarkSpeedPower : Ability, ICloneable
    {
        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone() { return new WildMarkSpeedPower { _defaultIcon = _defaultIcon }; }

        public sealed class Instance : AbilityInstance<WildMarkSpeedPower>
        {
            private Stat.Values bonus;
            private float refreshTimer;
            public Instance(Character owner, WildMarkSpeedPower ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                bonus = new Stat.Values(new[]
                {
                    new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.PhysicalAttackDamage, 0.0),
                    new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MagicAttackDamage, 0.0)
                });
                owner.stat.AttachValues(bonus);
                Refresh();
            }

            public override void OnDetach()
            {
                if (bonus != null) owner.stat.DetachValues(bonus);
                bonus = null;
            }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                refreshTimer -= deltaTime;
                if (refreshTimer > 0) return;
                refreshTimer = 0.2f;
                Refresh();
            }

            private void Refresh()
            {
                if (bonus == null) return;
                // PercentPoint is expressed as a fraction (0.9 => +90%).
                // Clamp debuffs to zero: item description specifies "additional" movement speed.
                double speed = Math.Max(0.0, owner.stat.Get(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed));
                if (Math.Abs(bonus.values[0].value - speed) < 0.00001) return;
                bonus.values[0].value = speed;
                bonus.values[1].value = speed;
                owner.stat.SetNeedUpdate();
            }
        }
    }

    public sealed class WildMarkSpeedPowerComponent : AbilityComponent<WildMarkSpeedPower>
    {
        internal void Configure(WildMarkSpeedPower value) { _ability = value; }
    }
}
