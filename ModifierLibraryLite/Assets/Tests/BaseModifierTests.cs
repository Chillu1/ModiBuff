using System.Collections;
using System.Collections.Generic;
using ModifierLibraryLite.Core;
using ModifierLibraryLite.Core.Units;
using NUnit.Framework;
using UnityEngine;

namespace ModifierLibraryLite.Tests
{
	public abstract class BaseModifierTests
	{
		protected ModifierRecipes Recipes { get; private set; }

		protected IUnit Unit { get; private set; }
		protected float UnitHealth { get; private set; }
		protected float UnitDamage { get; private set; }
		protected float UnitHeal { get; private set; }

		protected IUnit Enemy { get; private set; }
		protected float EnemyHealth { get; private set; }
		protected float EnemyDamage { get; private set; }
		protected float EnemyHeal { get; private set; }

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			Recipes = new ModifierRecipes();
		}

		[SetUp]
		public void Setup()
		{
			UnitHealth = 500;
			UnitDamage = 10;
			UnitHeal = 5;
			EnemyHealth = 1000;
			EnemyDamage = 20;
			EnemyHeal = 10;

			Unit = new Unit(UnitHealth, UnitDamage, UnitHeal);
			Enemy = new Unit(EnemyHealth, EnemyDamage, EnemyHeal);
		}

		[TearDown]
		public void TearDown()
		{
			Unit = null;
		}
	}
}