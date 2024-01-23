using System.Collections.Generic;

namespace ModiBuff.Core.Units
{
	public sealed class ModifierIdBasedOnUnitTypeMetaEffect : IMetaEffect<int, int>
	{
		private readonly IReadOnlyDictionary<UnitType, int> _unitTypeToModifierId;

		public ModifierIdBasedOnUnitTypeMetaEffect(IReadOnlyDictionary<UnitType, int> unitTypeToModifierId)
		{
			_unitTypeToModifierId = unitTypeToModifierId;
		}

		public int Effect(int modifierId, IUnit target, IUnit source)
		{
			if (target is IUnitEntity unit &&
			    _unitTypeToModifierId.TryGetValue(unit.UnitType, out int newModifierId)) return newModifierId;

			return modifierId;
		}
	}
}