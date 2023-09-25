using System;
using System.Collections.Generic;
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

		private readonly RecipeAddFunc[] _defaultRecipeAddFuncs =
		{
			add => add("InitDamage").Effect(new DamageEffect(5), EffectOn.Init)
		};

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
		}

		protected void AddRecipes(params RecipeAddFunc[] recipeAddFunc) => SetupSystems(recipeAddFunc, new EventRecipeAddFunc[0]);

		protected void AddEventRecipes(params EventRecipeAddFunc[] recipeAddFunc) => SetupSystems(new RecipeAddFunc[0], recipeAddFunc);

		protected void AddMixedRecipes(RecipeAddFunc[] recipeAddFunc, EventRecipeAddFunc[] eventRecipeAddFunc)
		{
			SetupSystems(recipeAddFunc, eventRecipeAddFunc);
		}

		protected void AddGenerators(params ManualGeneratorData[] genData) => SetupSystems(genData);

		private void SetupSystems(RecipeAddFunc[] recipeAddFunc, EventRecipeAddFunc[] eventRecipeAddFunc)
		{
			var newRecipeAddFuncs = new RecipeAddFunc[_defaultRecipeAddFuncs.Length + recipeAddFunc.Length];
			_defaultRecipeAddFuncs.CopyTo(newRecipeAddFuncs, 0);
			recipeAddFunc.CopyTo(newRecipeAddFuncs, _defaultRecipeAddFuncs.Length);

			IdManager = new ModifierIdManager();
			var eventEffectFactory =
				new EventEffectFactory((effects, @event) => new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)@event));
			Recipes = new ModifierRecipes(newRecipeAddFuncs, eventRecipeAddFunc, IdManager, eventEffectFactory);
			Pool = new ModifierPool(Recipes.GetGenerators());

			Setup();
		}

		private void SetupSystems(ManualGeneratorData[] genData)
		{
			IdManager = new ModifierIdManager();
			var eventEffectFactory =
				new EventEffectFactory((effects, @event) => new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)@event));
			Recipes = new ModifierRecipes(_defaultRecipeAddFuncs, new EventRecipeAddFunc[0], IdManager, eventEffectFactory, genData);
			Pool = new ModifierPool(Recipes.GetGenerators());

			Setup();
		}

		public void SetupSystems()
		{
			IdManager = new ModifierIdManager();
			var eventEffectFactory =
				new EventEffectFactory((effects, @event) => new EventEffect<EffectOnEvent>(effects, (EffectOnEvent)@event));
			Recipes = new ModifierRecipes(_defaultRecipeAddFuncs, new EventRecipeAddFunc[0], IdManager, eventEffectFactory);
			Pool = new ModifierPool(Recipes.GetGenerators());

			Setup();
		}

		//Setup needs to be called manually so we create the units after all recipes have been added
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