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
		//We don't use an abstract method because then every condition would need to implement stack logic, of which not all can make sense

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(float value, IUnit target, IUnit source)
		{
			switch (this)
			{
				case AndCondition and:
					return and.Check(value, target, source);
				case OrCondition or:
					return or.Check(value, target, source);

				case ValueFull full:
					return full.Check(target);
				case ValueLow low:
					return low.Check(target);
				case ValueComparison comparison:
					return comparison.Check(value, target);
				case ValueComparisonPercent comparisonPercent:
					return comparisonPercent.Check(target);
				case StatusEffectCond statusEffect:
					return statusEffect.Check(target);
				case DebuffEffectCond debuffEffectCond:
					return debuffEffectCond.Check(target);
				default:
					Logger.LogError("[ModiBuff.Units] Invalid/unknown condition: " + GetType());
					return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(float value, int stacks, IUnit target, IUnit source)
		{
			switch (this)
			{
				case AndCondition and:
					return and.Check(value, target, source);
				case OrCondition or:
					return or.Check(value, target, source);

				case ValueFull full:
					return full.Check(target);
				case ValueLow low:
					return low.Check(target);
				case ValueComparison comparison:
					return comparison.Check(stacks, target);
				case ValueComparisonPercent comparisonPercent:
					return comparisonPercent.Check(target);
				case StatusEffectCond statusEffect:
					return statusEffect.Check(target);
				case DebuffEffectCond debuffEffectCond:
					return debuffEffectCond.Check(target);
				default:
					Logger.LogError("[ModiBuff.Units] Invalid/unknown condition: " + GetType());
					return false;
			}
		}
	}

	public sealed record AndCondition(params Condition[] Conditions) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new bool Check(float value, IUnit target, IUnit source)
		{
			for (int i = 0; i < Conditions.Length; i++)
				if (!Conditions[i].Check(value, target, source))
					return false;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new bool Check(float value, int stacks, IUnit target, IUnit source)
		{
			for (int i = 0; i < Conditions.Length; i++)
				if (!Conditions[i].Check(value, stacks, target, source))
					return false;

			return true;
		}
	}

	public sealed record OrCondition(params Condition[] Conditions) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new bool Check(float value, IUnit target, IUnit source)
		{
			for (int i = 0; i < Conditions.Length; i++)
				if (Conditions[i].Check(value, target, source))
					return true;

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new bool Check(float value, int stacks, IUnit target, IUnit source)
		{
			for (int i = 0; i < Conditions.Length; i++)
				if (Conditions[i].Check(value, stacks, target, source))
					return true;

			return false;
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

	public sealed record StatusEffectCond(StatusEffectType StatusEffectType, bool Invert = false) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => ((IStatusEffectOwner<LegalAction, StatusEffectType>)target)
			.StatusEffectController.HasStatusEffect(StatusEffectType) != Invert;
	}

	public sealed record DebuffEffectCond(DebuffType DebuffType, bool Invert = false) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => ((IDebuffable)target).ContainsDebuff(DebuffType) != Invert;
	}
}