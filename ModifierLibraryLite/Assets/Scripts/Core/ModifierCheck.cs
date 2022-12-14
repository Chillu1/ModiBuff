using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierCheck
	{
		public int IntId { get; }
		public string Name { get; }

		private readonly CooldownCheck _cooldown;
		private readonly CostCheck _cost;
		private readonly ChanceCheck _chance;

		public ModifierCheck(int intId, string name, CostCheck cost = null, ChanceCheck chance = null)
		{
			IntId = intId;
			Name = name;
			//_cooldown = cooldown;
			_cost = cost;
			_chance = chance;
		}

		public bool Check(IUnit unit)
		{
			//Debug.Log($"Checking {Name}");
			//result = _cooldown != null && _cooldown.IsReady;
			if (_chance != null && !_chance.Roll())
				return false;

			if (_cost != null && !_cost.Check(unit))
				return false;

			return true;
		}
	}
}