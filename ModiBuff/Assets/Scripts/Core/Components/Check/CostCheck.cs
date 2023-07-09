using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class CostCheck
	{
		private readonly CostType _costType;
		private readonly float _cost;

		public CostCheck(CostType costType, float cost)
		{
			_costType = costType;
			_cost = cost;
		}

		//TODO Instead of feeding the owner, cache it?
		public bool Check(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Health:
					if (unit is not IDamagable damagable || unit is not IHealthCost)
						return false;
					return damagable.Health >= _cost;
				case CostType.Mana:
					if (unit is not IManaOwner manaOwner)
						return false;
					return manaOwner.Mana >= _cost;
				default:
					Debug.LogError($"Unknown cost type: {_costType}");
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
					Debug.LogError($"Unknown cost type: {_costType}");
					return;
			}
		}
	}
}