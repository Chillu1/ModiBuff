using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public abstract class BaseModifierTests
	{
		private CoreSystem _coreSystem;

		protected ModifierRecipes Recipes { get; private set; }
		protected ModifierPool Pool { get; private set; }

		protected Unit Unit { get; private set; }
		protected float UnitHealth { get; private set; }
		protected float UnitDamage { get; private set; }
		protected float UnitHeal { get; private set; }
		protected float UnitMana { get; private set; }

		protected Unit Enemy { get; private set; }
		protected float EnemyHealth { get; private set; }
		protected float EnemyDamage { get; private set; }
		protected float EnemyHeal { get; private set; }

		protected Unit Ally { get; private set; }
		protected float AllyHealth { get; private set; }
		protected float AllyDamage { get; private set; }
		protected float AllyHeal { get; private set; }


		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_coreSystem = new CoreSystem(1);

			Recipes = _coreSystem.Recipes;
			Pool = _coreSystem.Pool;
		}

		[SetUp]
		public void Setup()
		{
			UnitHealth = AllyHealth = 500;
			UnitDamage = AllyDamage = 10;
			UnitHeal = AllyHeal = 5;
			UnitMana = 1000;
			EnemyHealth = 1000;
			EnemyDamage = 20;
			EnemyHeal = 10;

			Unit = new Unit(UnitHealth, UnitDamage, UnitHeal, UnitMana);
			Enemy = new Unit(EnemyHealth, EnemyDamage, EnemyHeal);
			Ally = new Unit(AllyHealth, AllyDamage, AllyHeal);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_coreSystem.Dispose();
			Recipes = null;
			Pool = null;
		}
	}
}