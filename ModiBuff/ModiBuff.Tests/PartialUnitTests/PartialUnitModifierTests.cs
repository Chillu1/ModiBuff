using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using TagType = ModiBuff.Core.Units.TagType;

namespace ModiBuff.Tests
{
	public abstract class PartialUnitModifierTests<TUnit>
	{
		protected ModifierIdManager IdManager { get; private set; }
		protected EffectIdManager EffectIdManager { get; private set; }
		protected ModifierRecipes Recipes { get; private set; }
		protected ModifierPool Pool { get; private set; }
		protected ModifierLessEffects Effects { get; private set; }

		protected TUnit Unit { get; private set; }
		protected float UnitHealth { get; private set; }
		protected float UnitDamage { get; private set; }
		protected float UnitHeal { get; private set; }
		protected float UnitMana { get; private set; }

		protected TUnit Enemy { get; private set; }
		protected float EnemyHealth { get; private set; }
		protected float EnemyDamage { get; private set; }
		protected float EnemyHeal { get; private set; }

		protected TUnit Ally { get; private set; }
		protected float AllyHealth { get; private set; }
		protected float AllyDamage { get; private set; }
		protected float AllyHeal { get; private set; }

		protected Func<float, float, float, float, UnitType, UnitTag, TUnit> UnitFactory;

		protected abstract void SetupUnitFactory();

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			SetupUnitFactory();

			Logger.SetLogger<NUnitLogger>();
			Config.DefaultTag = (int)Core.Units.TagType.Default;
			Config.PoolSize = 1;

			UnitHealth = AllyHealth = 500;
			UnitDamage = AllyDamage = 10;
			UnitHeal = AllyHeal = 5;
			UnitMana = 1000;
			EnemyHealth = 1000;
			EnemyDamage = 20;
			EnemyHeal = 10;

			if (UnitFactory == null)
				Logger.LogError($"No unit factory before OneTimeSetup for type {typeof(TUnit)}");
		}

		[SetUp]
		public void IterationSetup()
		{
			IdManager = new ModifierIdManager();
			EffectIdManager = new EffectIdManager();
			Recipes = new ModifierRecipes(IdManager);
			Recipes.Add("InitDamage").Effect(new DamageEffect(5), EffectOn.Init);
			Effects = new ModifierLessEffects(EffectIdManager);
		}

		protected ModifierRecipe AddRecipe(string name) => Recipes.Add(name, "", "");
		protected ModifierRecipe AddRecipe(RecipeAddFunc addFunc) => addFunc(AddRecipe);

		protected void AddEffect(string name, params IEffect[] effects) => Effects.Add(name, effects);

		protected void AddGenerator(string name, in ModifierGeneratorFunc createFunc, TagType tag = TagType.Default)
		{
			Recipes.Add(name, name, "", in createFunc, tag.ToInternalTag());
		}

		/// <summary>
		///		Setup needs to be called manually so we create the units after all recipes have been added
		/// </summary>
		public void Setup()
		{
			Recipes.CreateGenerators();
			Effects.Finish();
			Pool = new ModifierPool(Recipes.GetGenerators());

			Unit = UnitFactory(UnitHealth, UnitDamage, UnitHeal, UnitMana, UnitType.Good, UnitTag.Default);
			Enemy = UnitFactory(EnemyHealth, EnemyDamage, EnemyHeal, 1000, UnitType.Bad, UnitTag.Default);
			Ally = UnitFactory(AllyHealth, AllyDamage, AllyHeal, 1000, UnitType.Good, UnitTag.Default);
		}

		[TearDown]
		public void TearDown()
		{
			Pool.Reset();
			Effects.Reset();
			IdManager.Reset();
			EffectIdManager.Reset();

			IdManager = null;
			EffectIdManager = null;
			Recipes = null;
			Pool = null;
			Effects = null;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Pool?.Reset();
			Effects?.Reset();
			IdManager?.Reset();
			EffectIdManager?.Reset();

			IdManager = null;
			EffectIdManager = null;
			Recipes = null;
			Pool = null;
			Effects = null;
		}
	}
}