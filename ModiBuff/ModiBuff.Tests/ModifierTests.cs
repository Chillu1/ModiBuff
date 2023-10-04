using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public abstract class ModifierTests
	{
		protected ModifierIdManager IdManager { get; private set; }
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
			Logger.SetLogger<NUnitLogger>();
			Config.PoolSize = 1;

			UnitHealth = AllyHealth = 500;
			UnitDamage = AllyDamage = 10;
			UnitHeal = AllyHeal = 5;
			UnitMana = 1000;
			EnemyHealth = 1000;
			EnemyDamage = 20;
			EnemyHeal = 10;
		}

		[SetUp]
		public void IterationSetup()
		{
			IdManager = new ModifierIdManager();
			Recipes = new ModifierRecipes(IdManager,
				(effects, @event) => new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)@event));
			Recipes.Add("InitDamage").Effect(new DamageEffect(5), EffectOn.Init);
		}

		protected ModifierRecipe AddRecipe(string name) => Recipes.Add(name, "", "");

		protected ModifierEventRecipe AddEventRecipe(string name, EffectOnEvent @event) =>
			Recipes.AddEvent(name, @event);

		protected void AddGenerator(in ManualGeneratorData data) =>
			Recipes.Add(data.Name, in data.CreateFunc, in data.AddData);

		protected void AddGenerator(string name, in ModifierGeneratorFunc createFunc, in ModifierAddData addData) =>
			Recipes.Add(name, in createFunc, in addData);

		/// <summary>
		///		Setup needs to be called manually so we create the units after all recipes have been added
		/// </summary>
		public void Setup()
		{
			Recipes.CreateGenerators();
			Pool = new ModifierPool(Recipes.GetGenerators());

			Unit = new Unit(UnitHealth, UnitDamage, UnitHeal, UnitMana);
			Enemy = new Unit(EnemyHealth, EnemyDamage, EnemyHeal);
			Ally = new Unit(AllyHealth, AllyDamage, AllyHeal);
		}

		[TearDown]
		public void TearDown()
		{
			Pool.Reset();
			IdManager.Reset();

			IdManager = null;
			Recipes = null;
			Pool = null;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Pool?.Reset();
			IdManager?.Reset();

			IdManager = null;
			Recipes = null;
			Pool = null;
		}
	}
}