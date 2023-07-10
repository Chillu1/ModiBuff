using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class UnitTests : BaseModifierTests
	{
		[Test]
		public void DamageDamagableUnit()
		{
			var unit = new Unit();
			float unitHealth = unit.Health;

			unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(unitHealth - 5, unit.Health);
		}

		private sealed class NonDamagableUnit : IUnit, IModifierOwner
		{
			public ModifierController ModifierController { get; }

			public NonDamagableUnit()
			{
				ModifierController = new ModifierController();
			}

			public bool TryAddModifier(int id, IUnit source)
			{
				return ModifierController.TryAdd(id, this, source);
			}
		}

		[Test]
		public void DamageNonDamagableUnit()
		{
			NonDamagableUnit unit = new NonDamagableUnit();

			Assert.Throws<ArgumentException>(() => unit.TryAddModifierSelf("InitDamage"));
		}
	}
}