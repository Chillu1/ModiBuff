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

	public readonly struct ConditionRecipeSaveData
	{
		public readonly int Id;
		public readonly object SaveData;

		public ConditionRecipeSaveData(int id, object saveData)
		{
			Id = id;
			SaveData = saveData;
		}
	}

	public abstract record Condition(Targeting Targeting = Targeting.TargetSource) : ICondition
	{
		//We don't use an abstract method because then every condition would need to implement stack logic
		//Of which not all can make sense, instead we can pick and choose which conditions should support stacks, and how

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
				case LevelCond levelCond:
					return levelCond.Check(target);
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
				case LevelCond levelCond:
					return levelCond.Check(target);
				default:
					Logger.LogError("[ModiBuff.Units] Invalid/unknown condition: " + GetType());
					return false;
			}
		}

		public abstract object SaveRecipeState();
	}

	public sealed record AndCondition : Condition
	{
		public Condition[] Conditions { get; }

		public AndCondition(params ICondition[] conditions) => Conditions = conditions.Cast<Condition>().ToArray();

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

		public override object SaveRecipeState() =>
			new ConditionRecipeSaveData(ConditionEffectExtensions.GetConditionSaveData(Conditions));

		public readonly struct ConditionRecipeSaveData
		{
			public readonly object[] Conditions;

			public ConditionRecipeSaveData(object[] conditions) => Conditions = conditions;
		}
	}

	public sealed record OrCondition : Condition
	{
		public Condition[] Conditions { get; }

		public OrCondition(params ICondition[] conditions) => Conditions = conditions.Cast<Condition>().ToArray();

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

		public override object SaveRecipeState() =>
			new ConditionRecipeSaveData(ConditionEffectExtensions.GetConditionSaveData(Conditions));

		public readonly struct ConditionRecipeSaveData
		{
			public readonly object[] Conditions;

			public ConditionRecipeSaveData(object[] conditions) => Conditions = conditions;
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

		public override object SaveRecipeState() => new ConditionRecipeSaveData(StatType, Invert);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly StatTypeCondition StatTypeCondition;
			public readonly bool Invert;

			public ConditionRecipeSaveData(StatTypeCondition statTypeCondition, bool invert)
			{
				StatTypeCondition = statTypeCondition;
				Invert = invert;
			}
		}
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

		public override object SaveRecipeState() => new ConditionRecipeSaveData(StatType, Invert);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly StatTypeCondition StatTypeCondition;
			public readonly bool Invert;

			public ConditionRecipeSaveData(StatTypeCondition statTypeCondition, bool invert)
			{
				StatTypeCondition = statTypeCondition;
				Invert = invert;
			}
		}
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

		public override object SaveRecipeState() => new ConditionRecipeSaveData(ValueType, ComparisonType, Value);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly ValueType ValueType;
			public readonly ComparisonType ComparisonType;
			public readonly float Value;

			public ConditionRecipeSaveData(ValueType valueType, ComparisonType comparisonType, float value)
			{
				ValueType = valueType;
				ComparisonType = comparisonType;
				Value = value;
			}
		}
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

		public override object SaveRecipeState() => new ConditionRecipeSaveData(StatType, ComparisonType, Value);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly StatType StatType;
			public readonly ComparisonType ComparisonType;
			public readonly float Value;

			public ConditionRecipeSaveData(StatType statType, ComparisonType comparisonType, float value)
			{
				StatType = statType;
				ComparisonType = comparisonType;
				Value = value;
			}
		}
	}

	public sealed record StatusEffectCond(StatusEffectType StatusEffectType, bool Invert = false) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => ((IStatusEffectOwner<LegalAction, StatusEffectType>)target)
			.StatusEffectController.HasStatusEffect(StatusEffectType) != Invert;

		public override object SaveRecipeState() => new ConditionRecipeSaveData(StatusEffectType, Invert);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly StatusEffectType StatusEffectType;
			public readonly bool Invert;

			public ConditionRecipeSaveData(StatusEffectType statusEffectType, bool invert)
			{
				StatusEffectType = statusEffectType;
				Invert = invert;
			}
		}
	}

	public sealed record DebuffEffectCond(DebuffType DebuffType, bool Invert = false) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => ((IDebuffable)target).ContainsDebuff(DebuffType) != Invert;

		public override object SaveRecipeState() => new ConditionRecipeSaveData(DebuffType, Invert);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly DebuffType DebuffType;
			public readonly bool Invert;

			public ConditionRecipeSaveData(DebuffType debuffType, bool invert)
			{
				DebuffType = debuffType;
				Invert = invert;
			}
		}
	}

	public sealed record LevelCond(int ModifierId, int Value) : Condition
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(IUnit target) => ((ILevelOwner)target).IsLevel(ModifierId, Value);

		public override object SaveRecipeState() => new ConditionRecipeSaveData(ModifierId, Value);

		public readonly struct ConditionRecipeSaveData
		{
			public readonly int ModifierId;
			public readonly int Value;

			public ConditionRecipeSaveData(int modifierId, int value)
			{
				ModifierId = modifierId;
				Value = value;
			}
		}
	}
}