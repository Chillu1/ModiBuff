using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.Units.TagType;

namespace ModiBuff.Tests
{
	public abstract class ModifierTests
	{
		protected EffectTypeIdManager EffectTypeIdManager { get; private set; }
		protected ModifierIdManager IdManager { get; private set; }
		protected EffectIdManager EffectIdManager { get; private set; }
		protected ModifierRecipes Recipes { get; private set; }
		protected ModifierPool Pool { get; private set; }
		protected ModifierLessEffects Effects { get; private set; }
		protected ModifierControllerPool ModifierControllerPool { get; private set; }
		protected UnitHelper<int> UnitHelper { get; private set; }

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

		protected bool SkipInitDamageRecipe = false;

		[OneTimeSetUp]
		public virtual void OneTimeSetup()
		{
			Logger.SetLogger<NUnitLogger>();
			Config.DefaultTag = (ulong)Core.Units.TagType.Default;
			Config.PoolSize = 1;
			Config.ModifierControllerPoolSize = 3;
			Config.ModifierApplierControllerPoolSize = 3;
			EffectTypeIdManager = new EffectTypeIdManager();
			EffectTypeIdManager.RegisterAllEffectTypesInAssemblies();

			UnitHealth = AllyHealth = 500;
			UnitDamage = AllyDamage = 10;
			UnitHeal = AllyHeal = 5;
			UnitMana = 1000;
			EnemyHealth = 1000;
			EnemyDamage = 20;
			EnemyHeal = 10;
		}

		[SetUp]
		public virtual void IterationSetup()
		{
			IdManager = new ModifierIdManager();
			EffectIdManager = new EffectIdManager();
			Recipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			if (!SkipInitDamageRecipe)
				Recipes.Add("InitDamage").Effect(new DamageEffect(5), EffectOn.Init);
			Effects = new ModifierLessEffects(EffectIdManager);
			UnitHelper = new UnitHelper<int>();
		}

		protected ModifierRecipe AddRecipe(string name) => Recipes.Add(name, "", "");
		protected ModifierRecipe AddRecipe(RecipeAddFunc addFunc) => addFunc(AddRecipe);

		protected void AddEffect(string name, params IEffect[] effects) => Effects.Add(name, effects);

		protected void AddGenerator(string name, in ModifierGeneratorFunc createFunc, TagType tag = TagType.Default,
			int? auraId = null, object customModifierData = null)
		{
			Recipes.Add(name, name, "", in createFunc, tag.ToInternalTag(), auraId, customModifierData);
		}

		/// <summary>
		///		Setup needs to be called manually so we create the units after all recipes have been added
		/// </summary>
		public void Setup()
		{
			Recipes.CreateGenerators();
			Effects.Finish();
			Pool = new ModifierPool(Recipes.GetGenerators());
			ModifierControllerPool = new ModifierControllerPool();

			Unit = new Unit(UnitHealth, UnitDamage, UnitHeal, UnitMana, UnitType.Good);
			Enemy = new Unit(EnemyHealth, EnemyDamage, EnemyHeal, unitType: UnitType.Bad);
			Ally = new Unit(AllyHealth, AllyDamage, AllyHeal, unitType: UnitType.Good);
			UnitHelper.AddUnit(Unit, Unit.Id);
			UnitHelper.AddUnit(Enemy, Enemy.Id);
			UnitHelper.AddUnit(Ally, Ally.Id);
		}

		[TearDown]
		public void TearDown()
		{
			Pool.Reset();
			Effects.Reset();
			IdManager.Reset();
			EffectIdManager.Reset();
			ModifierControllerPool.Reset();
			UnitHelper.Reset();

			IdManager = null;
			EffectIdManager = null;
			Recipes = null;
			Pool = null;
			Effects = null;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			EffectTypeIdManager.Reset();
			Pool?.Reset();
			Effects?.Reset();
			IdManager?.Reset();
			EffectIdManager?.Reset();
			ModifierControllerPool?.Reset();

			IdManager = null;
			EffectTypeIdManager = null;
			EffectIdManager = null;
			Recipes = null;
			Pool = null;
			Effects = null;
		}
	}
}