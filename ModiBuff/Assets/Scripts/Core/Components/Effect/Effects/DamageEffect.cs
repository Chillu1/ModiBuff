using System;
using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class DamageEffect : IStackEffect, IStateReset, IEffect
	{
		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.None)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			//Debug.Log($"Base damage: {_baseDamage}. Extra damage: {_extraDamage}");
			target.TakeDamage(_baseDamage + _extraDamage, acter);
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

		public IStackEffect ShallowClone() => new DamageEffect(_baseDamage, _stackEffect);
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