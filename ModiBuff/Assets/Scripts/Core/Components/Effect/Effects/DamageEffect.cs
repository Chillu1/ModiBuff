using System;
using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class DamageEffect : ITargetEffect, IEventTrigger, IStackEffect, IStateEffect, IEffect
	{
		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private bool _isEventBased;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect) :
			this(damage, stackEffect, Targeting.TargetActer, false)
		{
		}

		private DamageEffect(float damage, StackEffectType stackEffect, Targeting targeting, bool isEventBased)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_targeting = targeting;
			_isEventBased = isEventBased;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;
		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit acter)
		{
			//Debug.Log($"Base damage: {_baseDamage}. Extra damage: {_extraDamage}");
			Effect(target, acter, _baseDamage + _extraDamage);
		}

		private void Effect(IUnit target, IUnit acter, float damage)
		{
			switch (_targeting)
			{
				case Targeting.TargetActer:
					target.TakeDamage(damage, acter, !_isEventBased);
					break;
				case Targeting.ActerTarget:
					acter.TakeDamage(damage, target, !_isEventBased);
					break;
				case Targeting.TargetTarget:
					target.TakeDamage(damage, target, !_isEventBased);
					break;
				case Targeting.ActerActer:
					acter.TakeDamage(damage, acter, !_isEventBased);
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
			//Debug.Log($"Base damage: {_baseDamage}. Extra damage: {_extraDamage}. StackEffect: {_stackEffect}");
		}

		public void ResetState() => _extraDamage = 0;

		public IStateEffect ShallowClone() => new DamageEffect(_baseDamage, _stackEffect, _targeting, _isEventBased);
		object IShallowClone.ShallowClone() => ShallowClone();
	}

	[Flags]
	public enum StackEffectType
	{
		None = 0,
		Effect = 1,
		Add = 2, //Add to all damages?
		AddStacksBased = 4,
		//Multiply = 8, //Multiply all damages?
		//MultiplyStacksBased = 16,
		//SetMultiplierStacksBased = 32, //Multiply all damages?
	}
}