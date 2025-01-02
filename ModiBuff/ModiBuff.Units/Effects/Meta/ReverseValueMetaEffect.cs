using System;

namespace ModiBuff.Core.Units
{
	public sealed class ReverseValueMetaEffect : IConditionEffect, IMetaEffect<float, float>, IMetaEffect<int, int>,
		IMetaEffect<float, int, float>, ISaveableRecipeEffect
	{
		public Condition[] Conditions { get; set; } = Array.Empty<Condition>();

		public float Effect(float value, IUnit target, IUnit source) => -value;
		public int Effect(int value, IUnit target, IUnit source) => -value;

		public float Effect(float value, int stacks, IUnit target, IUnit source) => -value;

		public object SaveRecipeState() => new object(); //TODO
	}
}