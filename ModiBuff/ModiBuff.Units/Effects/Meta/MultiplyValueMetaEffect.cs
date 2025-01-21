using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public sealed class MultiplyValueMetaEffect : IConditionEffect, IMetaEffect<float, float>, IMetaEffect<int, int>,
		IMetaEffect<float, int, float>, ISaveableRecipeEffect<MultiplyValueMetaEffect.RecipeSaveData>
	{
		public Condition[] Conditions { get; set; }

		private readonly float _value;

		public MultiplyValueMetaEffect(float value) : this(value, null)
		{
		}

		private MultiplyValueMetaEffect(float value, ICondition[] conditions)
		{
			_value = value;
			Conditions = conditions?.Cast<Condition>().ToArray() ?? Array.Empty<Condition>();
		}

		public float Effect(float value, IUnit target, IUnit source) => value * _value;

		public int Effect(int value, IUnit target, IUnit source) => (int)(value * _value);

		public float Effect(float value, int value2, IUnit target, IUnit source) => value * _value;

		public object SaveRecipeState() => new RecipeSaveData(_value, Conditions);

		public readonly struct RecipeSaveData
		{
			public readonly float Value;
			public readonly Condition[] Conditions;

			public RecipeSaveData(float value, Condition[] conditions)
			{
				Value = value;
				Conditions = conditions;
			}
		}
	}
}