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
					if (!(unit is IDamagable<float, float> damagable) || !(unit is IHealthCost))
						return false;
					return damagable.Health >= _cost;
				case CostType.Mana:
					if (!(unit is IManaOwner manaOwner))
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
					((IHealthCost)unit).UseHealth(_cost);
					return;
				case CostType.Mana:
					((IManaOwner)unit).UseMana(_cost);
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