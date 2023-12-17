namespace ModiBuff.Core.Units
{
	public sealed class DistanceTraveledMultiplierMetaEffect : IMetaEffect<float, float>
	{
		private readonly float _maxDistance;
		private readonly float _multiplier;
		private readonly Targeting _targeting;

		public DistanceTraveledMultiplierMetaEffect(float maxDistance, float multiplier,
			Targeting targeting = Targeting.TargetSource)
		{
			_maxDistance = maxDistance;
			_multiplier = multiplier;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);

			float distance = ((ITravelDistance<Vector2>)source).DistanceTraveled.Distance();

			float valueMultiplier = distance / _maxDistance * _multiplier + 1f;
			return value * valueMultiplier;
		}
	}
}