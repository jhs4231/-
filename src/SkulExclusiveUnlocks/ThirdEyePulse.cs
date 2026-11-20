using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Third Eye: every five seconds attack a single nearest enemy in front,
    /// dealing magic damage and attempting to stun.
    /// Source specifies 5 second interval but does NOT specify attack magnitude,
    /// search radius or stun duration. 1 base damage and 12-unit range are placeholders.
    /// </summary>
    [Serializable]
    public sealed class ThirdEyePulse : Ability, ICloneable
    {
        [SerializeField] public float interval = 5f;
        [SerializeField] public float maxRange = 12f; // UNSPECIFIED: temporary.
        [SerializeField] public double baseDamage = 1.0; // UNSPECIFIED: temporary.

        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone()
        {
            return new ThirdEyePulse
            {
                interval = interval, maxRange = maxRange,
                baseDamage = baseDamage, _defaultIcon = _defaultIcon
            };
        }

        public sealed class Instance : AbilityInstance<ThirdEyePulse>
        {
            private float remaining;
            public Instance(Character owner, ThirdEyePulse ability) : base(owner, ability) {}
            public override void OnAttach() { remaining = Math.Max(0.01f, ability.interval); }

            public override void UpdateTime(float deltaTime)
            {
                base.UpdateTime(deltaTime);
                remaining -= Math.Max(0, deltaTime);
                if (remaining > 0) return;
                remaining = Math.Max(0.01f, ability.interval);
                Character target = FindFrontTarget();
                if (target == null || ability.baseDamage <= 0) return;
                var damage = new Damage(owner, ability.baseDamage, target.transform.position,
                    Damage.Attribute.Magic, Damage.AttackType.Additional, Damage.MotionType.Item,
                    "SkulExclusive-ThirdEye");
                owner.Attack(target, ref damage);
                target.status.ApplyStun(owner);
            }

            private Character FindFrontTarget()
            {
                Character closest = null;
                float closestDistanceSq = ability.maxRange * ability.maxRange;
                bool faceRight = owner.lookingDirection == Character.LookingDirection.Right;
                foreach (var target in UnityEngine.Object.FindObjectsOfType<Character>())
                {
                    if (target == null || target == owner || target.health == null ||
                        target.health.dead || target.status == null || !Enemy(target.type)) continue;
                    Vector3 delta = target.transform.position - owner.transform.position;
                    if (faceRight ? delta.x <= 0 : delta.x >= 0) continue;
                    if (delta.sqrMagnitude >= closestDistanceSq) continue;
                    closest = target;
                    closestDistanceSq = delta.sqrMagnitude;
                }
                return closest;
            }

            private static bool Enemy(Character.Type type)
            {
                return type == Character.Type.TrashMob || type == Character.Type.Named ||
                       type == Character.Type.Adventurer || type == Character.Type.Boss;
            }
        }
    }

    public sealed class ThirdEyePulseComponent : AbilityComponent<ThirdEyePulse>
    {
        internal void Configure(ThirdEyePulse value) { _ability = value; }
    }
}
