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
					if (!(unit is IDamagable<Damage, float> damagable) || !(unit is IHealthCost))
						return false;
					return damagable.Health.Value >= _cost;
				default:
					Logger.LogError("Unknown cost type: " + _costType);
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
				default:
					Logger.LogError("Unknown cost type: " + _costType);
					return;
			}
		}
	}
}