using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public enum ValueType
	{
		Fed,
		Health,
		Mana,
	}

	public enum StatTypeCondition
	{
		Health,
		Mana,
	}

	//TODO Targeting
	public abstract record Condition;

	public sealed record ValueFull(StatTypeCondition StatType) : Condition;

	public sealed record ValueLow(StatTypeCondition StatType) : Condition
	{
		public const float Threshold = 0.3f; //Arbitrary, 30%
	}

	public sealed record ValueHigher(ValueType ValueType, float Value) : Condition;

	public sealed record ValueHigherPercent(StatTypeCondition ValueType, float Value) : Condition;

	public abstract class ConditionMetaEffect
	{
		protected Condition[] Conditions { get; private set; }

		public T Condition<T>(Condition condition)
		{
			Conditions = new[] { condition };
			return (T)(object)this;
		}

		public bool Check(float value, IUnit target, IUnit source)
		{
			for (int i = 0; i < Conditions.Length; i++)
			{
				if (!Check(value, target, source, Conditions[i]))
					return false;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Check(float value, IUnit target, IUnit source, Condition condition) => condition switch
		{
			ValueFull full => full.StatType switch
			{
				StatTypeCondition.Health => ((IDamagable<float, float>)target).FullHealth(),
				StatTypeCondition.Mana => ((IManaOwner<float, float>)target).FullMana(),
				_ => false
			},
			ValueLow low => low.StatType switch
			{
				StatTypeCondition.Health => ((IDamagable<float, float>)target).PercentageHealth() < ValueLow.Threshold,
				StatTypeCondition.Mana => ((IManaOwner<float, float>)target).PercentageMana() < ValueLow.Threshold,
				_ => false
			},
			ValueHigher higher => higher.ValueType switch
			{
				ValueType.Fed => value > higher.Value,
				ValueType.Health => ((IDamagable<float, float>)target).Health > higher.Value,
				ValueType.Mana => ((IManaOwner<float, float>)target).Mana > higher.Value,
				_ => false
			},
			ValueHigherPercent higherPercent => higherPercent.ValueType switch
			{
				StatTypeCondition.Health => ((IDamagable<float, float>)target).PercentageHealth() > higherPercent.Value,
				StatTypeCondition.Mana => ((IManaOwner<float, float>)target).PercentageMana() > higherPercent.Value,
				_ => false
			},
			_ => throw new ArgumentOutOfRangeException(nameof(condition))
		};
	}

	public sealed class ReverseValueMetaEffect : ConditionMetaEffect, IMetaEffect<float, float>, IMetaEffect<int, int>
	{
		public float Effect(float value, IUnit target, IUnit source) => -value;
		public int Effect(int value, IUnit target, IUnit source) => -value;
	}

	public static class Test
	{
		//public static ReverseValueMetaEffect Enable(this ReverseValueMetaEffect effect, On condition)
		//{
		//	return effect;
		//}
	}
}