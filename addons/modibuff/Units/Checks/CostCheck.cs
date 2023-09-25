namespace ModiBuff.Core.Units
{
	public sealed class CostCheck : IUsableCheck
	{
		private readonly CostType _costType;
		private readonly float _cost;

		public CostCheck(CostType costType, float cost)
		{
			_costType = costType;
			_cost = cost;
		}

		public bool Check(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Health:
					if (!(unit is IDamagable<float, float> damagable) || !(unit is IHealthCost<float>))
						return false;
					return damagable.Health >= _cost;
				case CostType.Mana:
					if (!(unit is IManaOwner<float, float> manaOwner))
						return false;
					return manaOwner.Mana >= _cost;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("Unknown cost type: " + _costType);
#endif
					return false;
			}
		}

		public void Use(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Health:
					((IHealthCost<float>)unit).UseHealth(_cost);
					return;
				case CostType.Mana:
					((IManaOwner<float, float>)unit).UseMana(_cost);
					return;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("Unknown cost type: " + _costType);
#endif
					return;
			}
		}
	}
}