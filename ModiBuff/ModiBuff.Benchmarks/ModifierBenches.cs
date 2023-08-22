using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	public abstract class ModifierBenches
	{
		protected const int Iterations = 10_000;

		protected ModifierIdManager IdManager { get; private set; }
		protected ModifierRecipes Recipes { get; private set; }
		protected ModifierPool Pool { get; private set; }


		[GlobalSetup]
		public virtual void GlobalSetup()
		{
			IdManager = new ModifierIdManager();
			Recipes = new TestModifierRecipes(IdManager);
			Pool = new ModifierPool(Recipes.GetRecipes(), 1024);
		}

		[GlobalCleanup]
		public virtual void OneTimeTearDown()
		{
			Pool.Dispose();
			IdManager.Reset();

			IdManager = null;
			Recipes = null;
			Pool = null;
		}
	}
}