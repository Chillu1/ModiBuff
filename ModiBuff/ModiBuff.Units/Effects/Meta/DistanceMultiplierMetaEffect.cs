namespace ModiBuff.Core.Units
{
	public sealed class DistanceMultiplierMetaEffect : IMetaEffect<float, float>
	{
		private readonly float _maxDistance;
		private readonly float _multiplier;
		private readonly Targeting _targeting;

		public DistanceMultiplierMetaEffect(float maxDistance, float multiplier,
			Targeting targeting = Targeting.TargetSource)
		{
			_maxDistance = maxDistance;
			_multiplier = multiplier;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);

			var targetPosition = ((IPosition<Vector2>)target).Position;
			var sourceInitialPosition = ((IInitialPosition<Vector2>)source).InitialPosition;
			float distance = sourceInitialPosition.DistanceTo(targetPosition);

			float valueMultiplier = distance / _maxDistance * _multiplier + 1f;
			return value * valueMultiplier;
		}
	}
}