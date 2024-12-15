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

	public abstract record Condition(Targeting Targeting = Targeting.TargetSource)
	{
		//public abstract bool Check(float value, IUnit target, IUnit source);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(float value, IUnit target, IUnit source)
		{
			switch (this)
			{
				case ValueFull full:
					return full.Check(target);
				case ValueLow low:
					return low.Check(target);
				case ValueComparison comparison:
					return comparison.Check(value, target);
				case ValueComparisonPercent comparisonPercent:
					return comparisonPercent.Check(target);
				default:
					Logger.LogError("[ModiBuff] Invalid condition: " + this);
					return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(float value, int stacks, IUnit target, IUnit source)
		{
			switch (this)
			{
				case ValueFull full:
					return full.Check(target);
				case ValueLow low:
					return low.Check(target);
				case ValueComparison comparison:
					return comparison.Check(stacks, target);
				case ValueComparisonPercent comparisonPercent:
					return comparisonPercent.Check(target);
				default:
					Logger.LogError("[ModiBuff] Invalid condition: " + this);
					return false;
			}
		}
	}

	public sealed record ValueFull(StatTypeCondition StatType, bool Invert = false) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => StatType switch
		{
			StatTypeCondition.Health => ((IDamagable<float, float>)target).FullHealth() != Invert,
			StatTypeCondition.Mana => ((IManaOwner<float, float>)target).FullMana() != Invert,
			_ => false
		};
	}

	public sealed record ValueLow(StatTypeCondition StatType, bool Invert = false) : Condition
	{
		public const float Threshold = 0.3f; //Arbitrary, 30%

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => StatType switch
		{
			StatTypeCondition.Health => ((IDamagable<float, float>)target).PercentageHealth() < Threshold != Invert,
			StatTypeCondition.Mana => ((IManaOwner<float, float>)target).PercentageMana() < Threshold != Invert,
			_ => false
		};
	}

	public sealed record ValueComparison(ValueType ValueType, ComparisonType ComparisonType, float Value) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(float value, IUnit target) => ValueType switch
		{
			ValueType.Fed => ComparisonType.Check(value, Value),
			ValueType.Health => ComparisonType.Check(((IDamagable<float, float>)target).Health, Value),
			ValueType.Mana => ComparisonType.Check(((IManaOwner<float, float>)target).Mana, Value),
			_ => false
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(int stacks, IUnit target) => ValueType switch
		{
			ValueType.Fed => ComparisonType.Check(stacks, Value),
			ValueType.Health => ComparisonType.Check(((IDamagable<float, float>)target).Health, Value),
			ValueType.Mana => ComparisonType.Check(((IManaOwner<float, float>)target).Mana, Value),
			_ => false
		};
	}

	public sealed record ValueComparisonPercent(StatType StatType, ComparisonType ComparisonType, float Value)
		: Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => StatType switch
		{
			StatType.Health => ComparisonType.Check(((IDamagable<float, float>)target).PercentageHealth(), Value),
			StatType.Mana => ComparisonType.Check(((IManaOwner<float, float>)target).PercentageMana(), Value),
			_ => false
		};
	}
}