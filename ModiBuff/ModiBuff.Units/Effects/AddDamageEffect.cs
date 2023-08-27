using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class AddDamageEffect : ITargetEffect, IStackEffect, IStateEffect, IRevertEffect, IEffect
	{
		public bool IsRevertible { get; }

		private readonly float _damage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;

		private float _extraDamage;
		private float _totalAddedDamage;

		public AddDamageEffect(float damage, bool revertible = false, StackEffectType stackEffect = StackEffectType.Effect) :
			this(damage, revertible, stackEffect, Targeting.TargetSource)
		{
		}

		private AddDamageEffect(float damage, bool revertible, StackEffectType stackEffect, Targeting targeting)
		{
			_damage = damage;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
			_targeting = targeting;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void Effect(IUnit target, IUnit source)
		{
			if (IsRevertible)
				_totalAddedDamage += _damage + _extraDamage;

			Effect(target, source, _damage + _extraDamage);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			Effect(target, source, -_totalAddedDamage);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect(IUnit target, IUnit source, float damage)
		{
			_targeting.UpdateTarget(ref target, source);
			((IAddDamage<float>)target).AddDamage(damage);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraDamage = 0;
			_totalAddedDamage = 0;
		}

		public IStateEffect ShallowClone() => new AddDamageEffect(_damage, IsRevertible, _stackEffect, _targeting);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}