namespace ModiBuff.Core.Units
{
	public sealed class ReverseValueMetaEffect : ConditionMetaEffect, IMetaEffect<float, float>,
		IMetaEffect<float, int, float>
	{
		public float Effect(float value, IUnit target, IUnit source) => -value;
		public int Effect(int value, IUnit target, IUnit source) => -value;

		public float Effect(float value, int stacks, IUnit target, IUnit source) => -value;
	}
}