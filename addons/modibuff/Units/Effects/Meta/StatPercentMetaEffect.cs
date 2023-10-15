using System;

namespace ModiBuff.Core.Units
{
	public sealed class StatPercentMetaEffect : IMetaEffect<float, float>
	{
		private readonly StatType _statType;
		private readonly Targeting _targeting;

		public StatPercentMetaEffect(StatType statType, Targeting targeting = Targeting.TargetSource)
		{
			_statType = statType;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);

			float percent;

			switch (_statType)
			{
				case StatType.Health:
					percent = ((IDamagable<float, float>)target).PercentageHealth();
					break;
				case StatType.Mana:
					percent = ((IManaOwner<float, float>)target).PercentageMana();
					break;
				default:
					throw new ArgumentException("StatType must be Health or Mana");
			}

			return value * percent;
		}
	}
}