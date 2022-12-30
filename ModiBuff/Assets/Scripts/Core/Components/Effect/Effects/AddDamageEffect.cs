using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public sealed class AddDamageEffect : ITargetEffect, IStackEffect, IStateEffect, IRevertEffect, IEffect
	{
		public bool IsRevertible { get; }

		private readonly float _damage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;

		private float _extraDamage;
		private float _totalAddedDamage;

		public AddDamageEffect(float damage, bool revertible = false, StackEffectType stackEffect = StackEffectType.None) :
			this(damage, revertible, stackEffect, Targeting.TargetActer)
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

		public void Effect(IUnit target, IUnit acter)
		{
			if (IsRevertible)
				_totalAddedDamage += _damage + _extraDamage;

			Effect(target, acter, _damage + _extraDamage);
		}

		public void RevertEffect(IUnit target, IUnit acter)
		{
			Effect(target, acter, -_totalAddedDamage);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect(IUnit target, IUnit acter, float damage)
		{
			switch (_targeting)
			{
				case Targeting.TargetActer:
					target.AddDamage(damage);
					break;
				case Targeting.ActerTarget:
					acter.AddDamage(damage);
					break;
				case Targeting.TargetTarget:
					target.AddDamage(damage);
					break;
				case Targeting.ActerActer:
					acter.AddDamage(damage);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(targetComponent.Target, targetComponent.Acter);
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