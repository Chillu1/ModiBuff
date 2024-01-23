namespace ModiBuff.Core.Units
{
	public sealed class AddValueBasedOnPoisonStacksMetaEffect : IMetaEffect<float, float>
	{
		private readonly float _multiplier;
		private readonly Targeting _targeting;

		public AddValueBasedOnPoisonStacksMetaEffect(float multiplier, Targeting targeting = Targeting.TargetSource)
		{
			_multiplier = multiplier;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			return value + ((IPoisonable)target).PoisonStacks * _multiplier;
		}
	}
}