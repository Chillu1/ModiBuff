using System;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	// public sealed class DamageData
	// {
	// 	public float Damage => _baseDamage + _extraDamage;
	// 	private readonly float _baseDamage;
	// 	private float _extraDamage;
	//
	// 	public DamageData(float damage) => _baseDamage = damage;
	//
	// 	public void Add(float damage) => _extraDamage += damage;
	// 	public void Remove(float damage) => _extraDamage -= damage;
	// }

	public class DamageEffect : IStackEffect, IEffect
	{
		private readonly float _damage;
		private readonly StackEffectType _stackEffect;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.None)
		{
			_damage = damage;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit owner)
		{
			target.TakeDamage(_damage, owner);
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			switch (_stackEffect)
			{
				case StackEffectType.Effect:
					Effect(targetComponent.Target, targetComponent.Owner);
					break;
				case StackEffectType.Add:
					//TODO This is a problem, having state in effects is bad for us. Because then we need to clone it.
					//_damage += value;
					break;
				case StackEffectType.AddStacksBased:
					//_damage += value * stacks;
					break;
				default:
					Debug.LogError($"StackEffectType {_stackEffect} not implemented");
					break;
			}
		}
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