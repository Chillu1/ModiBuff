using System;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	/*public abstract class BaseSharedTests : BaseModifierTests, IBaseSharedTests
	{
		public new float UnitHealth => base.UnitHealth;
		public float CurrentUnitHealth => Unit.Health;

		/// <summary>
		///		Does nothing here in Lite version.
		/// </summary>
		public void SetSystemsState(SystemsState state)
		{
		}

		public void AddModifier(string modifierId, UnitType unitType)
		{
			switch (unitType)
			{
				case UnitType.Unit:
					Unit.TryAddModifierSelf(modifierId);
					break;
				case UnitType.Ally:
					Ally.TryAddModifierSelf(modifierId);
					break;
				case UnitType.Enemy:
					Enemy.TryAddModifierSelf(modifierId);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null);
			}
		}
	}*/
}