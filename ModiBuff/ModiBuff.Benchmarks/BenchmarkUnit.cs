using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	public sealed class BenchmarkUnit : IUnit, IModifierOwner, IDamagable, IUpdatable, IUnitEntity
	{
		public UnitTag UnitTag { get; }
		public UnitType UnitType { get; }

		public float Health { get; private set; }
		public float MaxHealth { get; }

		public ModifierController ModifierController { get; }

		private const int MaxRecursionEventCount = 1;

		public BenchmarkUnit(float health, UnitType unitType = UnitType.Good)
		{
			UnitType = unitType;
			UnitTag = UnitTag.Default;
			MaxHealth = Health = health;

			ModifierController = new ModifierController();
		}

		public void Update(float delta)
		{
			ModifierController.Update(delta);
		}

		public float TakeDamage(float damage, IUnit source)
		{
			Health -= damage;
			return damage;
		}
	}
}