using System;
using Characters;
using Characters.Abilities;
using Characters.Actions;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Destroyer Blade: ramp stats only while a Skill action is running.
    /// Ramp time is a design placeholder because source image provides caps, not a rate.
    /// </summary>
    [Serializable]
    public sealed class DestroyerBladeCharge : Ability, ICloneable
    {
        [SerializeField] public float secondsToMaximum = 3.0f; // TUNABLE PLACEHOLDER, not source specified.
        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone() { return new DestroyerBladeCharge { secondsToMaximum = secondsToMaximum, _defaultIcon = _defaultIcon }; }

        public sealed class Instance : AbilityInstance<DestroyerBladeCharge>
        {
            private Stat.Values stats;
            private bool charging;
            private float elapsed;

            public Instance(Character owner, DestroyerBladeCharge ability) : base(owner, ability) {}

            public override void OnAttach()
            {
                stats = new Stat.Values(new[]
                {
                    new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.PhysicalAttackDamage, 0.0),
                    new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.CriticalChance, 0.0)
                });
                owner.stat.AttachValues(stats);
                owner.onStartAction += OnStartAction;
                owner.onEndAction += OnEndAction;
                owner.onCancelAction += OnEndAction;
            }

            public override void OnDetach()
            {
                owner.onStartAction -= OnStartAction;
                owner.onEndAction -= OnEndAction;
                owner.onCancelAction -= OnEndAction;
                if (stats != null) owner.stat.DetachValues(stats);
                stats = null;
                charging = false;
            }

            private void OnStartAction(Characters.Actions.Action action)
            {
                if (action == null || action.type != Characters.Actions.Action.Type.Skill) return;
                charging = true;
                elapsed = 0;
                UpdateStats();
            }

            private void OnEndAction(Characters.Actions.Action action)
            {
                if (action == null || action.type != Characters.Actions.Action.Type.Skill) return;
                charging = false;
                elapsed = 0;
                UpdateStats();
            }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                if (!charging) return;
                elapsed = Mathf.Min(ability.secondsToMaximum, elapsed + Mathf.Max(0, deltaTime));
                UpdateStats();
            }

            private void UpdateStats()
            {
                if (stats == null) return;
                float fraction = charging ? Mathf.Clamp01(elapsed / Mathf.Max(0.01f, ability.secondsToMaximum)) : 0f;
                stats.values[0].value = fraction;         // +100% at full ramp
                stats.values[1].value = fraction * 0.35;  // +35 percentage points
                owner.stat.SetNeedUpdate();
            }
        }
    }

    public sealed class DestroyerBladeChargeComponent : AbilityComponent<DestroyerBladeCharge>
    {
        internal void Configure(DestroyerBladeCharge value) { _ability = value; }
    }
}
