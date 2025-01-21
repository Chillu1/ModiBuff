using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public sealed class ReverseValueMetaEffect : IConditionEffect, IMetaEffect<float, float>, IMetaEffect<int, int>,
		IMetaEffect<float, int, float>, ISaveableRecipeEffect
	{
		public Condition[] Conditions { get; set; }

		public ReverseValueMetaEffect() : this(Array.Empty<ICondition>())
		{
		}

		private ReverseValueMetaEffect(ICondition[] conditions) => Conditions = conditions.Cast<Condition>().ToArray();

		public float Effect(float value, IUnit target, IUnit source) => -value;
		public int Effect(int value, IUnit target, IUnit source) => -value;

		public float Effect(float value, int stacks, IUnit target, IUnit source) => -value;

		public object SaveRecipeState() => new RecipeSaveData(this.GetConditionSaveData(Conditions));

		public readonly struct RecipeSaveData
		{
			public readonly object[] Conditions;

			public RecipeSaveData(object[] conditions) => Conditions = conditions;
		}
	}
}