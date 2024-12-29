using System;

namespace ModiBuff.Core.Units
{
	public sealed class MultiplyValueMetaEffect : IConditionEffect, IMetaEffect<float, float>, IMetaEffect<int, int>,
		IMetaEffect<float, int, float>
	{
		public Condition[] Conditions { get; set; } = Array.Empty<Condition>();

		private readonly float _value;

		public MultiplyValueMetaEffect(float value) => _value = value;

		public float Effect(float value, IUnit target, IUnit source) => value * _value;

		public int Effect(int value, IUnit target, IUnit source) => (int)(value * _value);

		public float Effect(float value, int value2, IUnit target, IUnit source) => value * _value;
	}
}