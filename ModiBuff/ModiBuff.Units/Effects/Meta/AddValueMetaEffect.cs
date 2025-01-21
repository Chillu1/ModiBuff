using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public sealed class AddValueMetaEffect : IConditionEffect, IMetaEffect<float, float>, IMetaEffect<int, int>,
		IMetaEffect<float, int, float>, ISaveableRecipeEffect<AddValueMetaEffect.RecipeSaveData>
	{
		public Condition[] Conditions { get; set; }

		private readonly float _value;

		public AddValueMetaEffect(float value) : this(value, Array.Empty<ICondition>())
		{
		}

		private AddValueMetaEffect(float value, ICondition[] conditions)
		{
			_value = value;
			Conditions = conditions.Cast<Condition>().ToArray();
		}

		public float Effect(float value, IUnit target, IUnit source) => value + _value;

		public int Effect(int value, IUnit target, IUnit source) => value + (int)_value;

		public float Effect(float value, int value2, IUnit target, IUnit source) => value + _value;

		public object SaveRecipeState() => new RecipeSaveData(_value, this.GetConditionSaveData(Conditions));

		public readonly struct RecipeSaveData
		{
			public readonly float Value;
			public readonly object[] Conditions;

			public RecipeSaveData(float value, object[] conditions)
			{
				Value = value;
				Conditions = conditions;
			}
		}
	}
}