using System;
using System.Linq;
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

	public sealed record ValueFull(StatTypeCondition StatType, bool Invert = false) : Condition;

	public sealed record ValueLow(StatTypeCondition StatType, bool Invert = false) : Condition
	{
		public const float Threshold = 0.3f; //Arbitrary, 30%
	}

	public sealed record ValueComparison(ValueType ValueType, ComparisonType ComparisonType, float Value) : Condition;

	public sealed record ValueComparisonPercent(StatType StatType, ComparisonType ComparisonType, float Value)
		: Condition;

	public abstract class ConditionMetaEffect
	{
		protected Condition[] Conditions { get; private set; } = Array.Empty<Condition>();

		public T Condition<T>(Condition condition) where T : ConditionMetaEffect
		{
			Conditions = Conditions.Append(condition).ToArray();
			return (T)this;
		}

		public T Condition<T>(params Condition[] conditions) where T : ConditionMetaEffect
		{
			Conditions = Conditions.Concat(conditions).ToArray();
			return (T)this;
		}

		public bool Check(float value, IUnit target, IUnit source)
		{
			for (int i = 0; i < Conditions.Length; i++)
				if (!Check(value, target, source, Conditions[i]))
					return false;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Check(float value, IUnit target, IUnit source, Condition condition) => condition switch
		{
			ValueFull full => full.StatType switch
			{
				StatTypeCondition.Health => ((IDamagable<float, float>)target).FullHealth() != full.Invert,
				StatTypeCondition.Mana => ((IManaOwner<float, float>)target).FullMana() != full.Invert,
				_ => false
			},
			ValueLow low => low.StatType switch
			{
				StatTypeCondition.Health =>
					((IDamagable<float, float>)target).PercentageHealth() < ValueLow.Threshold != low.Invert,
				StatTypeCondition.Mana =>
					((IManaOwner<float, float>)target).PercentageMana() < ValueLow.Threshold != low.Invert,
				_ => false
			},
			ValueComparison comparison => comparison.ValueType switch
			{
				ValueType.Fed => comparison.ComparisonType.Check(value, comparison.Value),
				ValueType.Health => comparison.ComparisonType.Check(((IDamagable<float, float>)target).Health,
					comparison.Value),
				ValueType.Mana => comparison.ComparisonType.Check(((IManaOwner<float, float>)target).Mana,
					comparison.Value),
				_ => false
			},
			ValueComparisonPercent comparisonPercent => comparisonPercent.StatType switch
			{
				StatType.Health => comparisonPercent.ComparisonType.Check(
					((IDamagable<float, float>)target).PercentageHealth(), comparisonPercent.Value),
				StatType.Mana => comparisonPercent.ComparisonType.Check(
					((IManaOwner<float, float>)target).PercentageMana(), comparisonPercent.Value),
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