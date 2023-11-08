namespace ModiBuff.Core.Units
{
	public sealed class CostPercentCheck : IUsableCheck, IDataCheck<CostPercentCheck.Data>
	{
		private readonly CostType _costType;
		private readonly float _costPercent;

		public CostPercentCheck(CostType costType, float costPercent)
		{
			_costType = costType;
			_costPercent = costPercent;
		}

		public bool Check(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Health:
					if (!(unit is IDamagable<float, float> damagable) || !(unit is IHealthCost<float>))
						return false;
					return damagable.Health > damagable.MaxHealth * _costPercent;
				case CostType.Mana:
					if (!(unit is IManaOwner<float, float> manaOwner))
						return false;
					return manaOwner.Mana >= manaOwner.MaxMana * _costPercent;
				default:
					Logger.LogError("Unknown cost type: " + _costType);
					return false;
			}
		}

		public void Use(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Mana:
					var manaOwner = (IManaOwner<float, float>)unit;
					manaOwner.UseMana(manaOwner.MaxMana * _costPercent);
					break;
				case CostType.Health:
					var healthCost = (IHealthCost<float>)unit;
					healthCost.UseHealth(((IDamagable<float, float>)unit).MaxHealth * _costPercent);
					break;
				default:
					Logger.LogError("Unknown cost type: " + _costType);
					break;
			}
		}

		public Data GetData() => new Data(_costType, _costPercent);

		public readonly struct Data
		{
			public readonly CostType CostType;
			public readonly float CostPercent;

			public Data(CostType costType, float costPercent)
			{
				CostType = costType;
				CostPercent = costPercent;
			}
		}
	}
}