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
					bool result = unit.Health >= _cost;
					if (result)
						unit.UseHealth(_cost);
					return result;
				case CostType.Mana:
					throw new System.NotImplementedException();
				default:
					Debug.LogError($"Unknown cost type: {_costType}");
					return false;
			}
		}
	}
}