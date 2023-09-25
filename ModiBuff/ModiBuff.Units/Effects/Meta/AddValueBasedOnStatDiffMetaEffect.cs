using System;

namespace ModiBuff.Core.Units
{
	public sealed class AddValueBasedOnStatDiffMetaEffect : IMetaEffect<float, float>
	{
		private readonly StatType _statType;
		private readonly Targeting _targeting;

		public AddValueBasedOnStatDiffMetaEffect(StatType statType, Targeting targeting = Targeting.TargetSource)
		{
			_statType = statType;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);

			float extraValue;

			switch (_statType)
			{
				case StatType.MaxHealth:
					var damagable = (IDamagable<float, float>)target;
					extraValue = damagable.MaxHealth - damagable.Health;
					break;
				default:
					throw new ArgumentException($"StatType {_statType} is not supported/implemented");
			}

			return value + extraValue;
		}
	}
}