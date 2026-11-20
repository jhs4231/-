using System;
using Characters;
using Characters.Abilities;
using Characters.Actions;
using Characters.Cooldowns;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>
    /// Chains: on skill cast, add magical chained hits scaled by base skill cooldown.
    /// Source does not specify damage formula, chain count formula or visuals;
    /// values here are provisional gameplay settings, not a source reproduction.
    /// No graphical chain prefab is created here.
    /// </summary>
    [Serializable]
    public sealed class ChainsOnSkill : Ability, ICloneable
    {
        [SerializeField] public float secondsPerChain = 5f; // TUNABLE placeholder.
        [SerializeField] public int maximumChains = 8; // TUNABLE placeholder.
        [SerializeField] public double baseDamagePerChain = 1.0; // TUNABLE placeholder.
        [SerializeField] public float range = 12f; // TUNABLE placeholder.

        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone()
        {
            return new ChainsOnSkill
            {
                secondsPerChain = secondsPerChain,
                maximumChains = maximumChains,
                baseDamagePerChain = baseDamagePerChain,
                range = range,
                _defaultIcon = _defaultIcon
            };
        }

        public sealed class Instance : AbilityInstance<ChainsOnSkill>
        {
            public Instance(Character owner, ChainsOnSkill ability) : base(owner, ability) {}
            public override void OnAttach() { owner.onStartAction += OnStartAction; }
            public override void OnDetach() { owner.onStartAction -= OnStartAction; }

            private void OnStartAction(Characters.Actions.Action action)
            {
                if (action == null || action.type != Characters.Actions.Action.Type.Skill ||
                    ability.baseDamagePerChain <= 0) return;

                float cooldownSeconds = 0;
                if (action.cooldown != null && action.cooldown.type == CooldownSerializer.Type.Time &&
                    action.cooldown.time != null)
                    cooldownSeconds = action.cooldown.time.cooldownTime;

                int chainCount = Mathf.Clamp(
                    1 + Mathf.FloorToInt(cooldownSeconds / Mathf.Max(0.1f, ability.secondsPerChain)),
                    1, Mathf.Max(1, ability.maximumChains));
                // Any target in range may receive multiple hits. Each hit is tagged as item damage.
                var targets = UnityEngine.Object.FindObjectsOfType<Character>();
                for (int i = 0; i < chainCount; i++)
                {
                    Character best = null;
                    float distanceSquared = ability.range * ability.range;
                    foreach (var target in targets)
                    {
                        if (target == null || target == owner || target.health == null || target.health.dead ||
                            !Enemy(target.type)) continue;
                        float distance = (target.transform.position - owner.transform.position).sqrMagnitude;
                        if (distance >= distanceSquared) continue;
                        distanceSquared = distance;
                        best = target;
                    }
                    if (best == null) break;
                    var hit = new Damage(owner, ability.baseDamagePerChain * (1 + cooldownSeconds / 10.0),
                        best.transform.position, Damage.Attribute.Magic, Damage.AttackType.Additional,
                        Damage.MotionType.Item, "SkulExclusive-Chains");
                    owner.Attack(best, ref hit);
                }
            }

            private static bool Enemy(Character.Type type)
            {
                return type == Character.Type.TrashMob || type == Character.Type.Named ||
                       type == Character.Type.Adventurer || type == Character.Type.Boss;
            }
        }
    }

    public sealed class ChainsOnSkillComponent : AbilityComponent<ChainsOnSkill>
    {
        internal void Configure(ChainsOnSkill ability) { _ability = ability; }
    }
}
