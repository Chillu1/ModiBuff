using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	public abstract class ModifierBenches
	{
		protected ModifierIdManager IdManager { get; private set; }
		protected ModifierRecipes Recipes { get; private set; }
		protected ModifierPool Pool { get; private set; }


		[GlobalSetup]
		public virtual void GlobalSetup()
		{
			Config.PoolSize = 1024;

			IdManager = new ModifierIdManager();
			Recipes = new TestModifierRecipes(IdManager);
			Pool = new ModifierPool(Recipes.GetGenerators());
		}

		[GlobalCleanup]
		public virtual void OneTimeTearDown()
		{
			Pool.Reset();
			IdManager.Reset();

			IdManager = null;
			Recipes = null;
			Pool = null;
		}
	}
}