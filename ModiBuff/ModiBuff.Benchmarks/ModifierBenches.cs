using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	public abstract class ModifierBenches
	{
		protected const int Iterations = 10_000;

		private CoreSystem _coreSystem;

		protected ModifierIdManager IdManager { get; private set; }
		protected ModifierRecipes Recipes { get; private set; }
		protected ModifierPool Pool { get; private set; }


		[GlobalSetup]
		public virtual void GlobalSetup()
		{
			_coreSystem = new CoreSystem(1024);

			IdManager = _coreSystem.IdManager;
			Recipes = _coreSystem.Recipes;
			Pool = _coreSystem.Pool;
		}

		[GlobalCleanup]
		public virtual void OneTimeTearDown()
		{
			_coreSystem.Dispose();
			IdManager = null;
			Recipes = null;
			Pool = null;
		}
	}
}