using UnityEngine;

namespace ModifierLibraryLite.Core
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
					return unit.Health >= _cost;
				case CostType.Mana:
					return unit.Mana >= _cost;
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
					unit.UseHealth(_cost);
					return;
				case CostType.Mana:
					unit.UseMana(_cost);
					return;
				default:
					Debug.LogError($"Unknown cost type: {_costType}");
					return;
			}
		}
	}
}