using System;
using Characters;
using Characters.Abilities;
using UnityEngine;

namespace SkulExclusiveUnlocks
{
    /// <summary>Universal Flask: on each valid offensive hit roll once for exactly one random status.</summary>
    [Serializable]
    public sealed class UniversalFlaskProc : Ability, ICloneable
    {
        [SerializeField] public int chancePercent = 15;
        public override IAbilityInstance CreateInstance(Character owner) { return new Instance(owner, this); }
        public object Clone() { return new UniversalFlaskProc { chancePercent = chancePercent, _defaultIcon = _defaultIcon }; }

        public sealed class Instance : AbilityInstance<UniversalFlaskProc>
        {
            public Instance(Character owner, UniversalFlaskProc ability) : base(owner, ability) {}
            public override void OnAttach() { owner.onGiveDamage.Add(0, OnHit); }
            public override void OnDetach() { owner.onGiveDamage.Remove(OnHit); }

            private bool OnHit(ITarget target, ref Damage damage)
            {
                if (target == null || target.character == null || target.character.status == null ||
                    damage.@null || damage.amount <= 0 || damage.motionType == Damage.MotionType.Status)
                    return false;
                if (Southpaw.Random.NextInt(0, 99) >= ability.chancePercent) return false;
                switch (Southpaw.Random.NextInt(0, 4))
                {
                    case 0: target.character.status.ApplyBurn(owner); break;
                    case 1: target.character.status.ApplyFreeze(owner); break;
                    case 2: target.character.status.ApplyPoison(owner); break;
                    case 3: target.character.status.ApplyStun(owner); break;
                    default: target.character.status.ApplyWound(owner); break;
                }
                return false;
            }
        }
    }

    public sealed class UniversalFlaskProcComponent : AbilityComponent<UniversalFlaskProc>
    {
        internal void Configure(UniversalFlaskProc value) { _ability = value; }
    }
}
