using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchNewModifier : ModifierBenches
	{
		private IModifierRecipe _initDamageRecipe;
		private IModifierRecipe _initDoTSeparateDamageRemoveRecipe;
		private IModifierRecipe _intervalDamageStackAddDamageRecipe;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_initDamageRecipe = Recipes.GetRecipe("InitDamage");
			_initDoTSeparateDamageRemoveRecipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			_intervalDamageStackAddDamageRecipe = Recipes.GetRecipe("IntervalDamage_StackAddDamage");

			Pool.Clear();
			Pool.SetMaxPoolSize(1_000_000);
			//Pool.Allocate(Recipes.GetRecipe("InitDoTSeparateDamageRemove").Id, 60 * Iterations);

			Pool.Allocate(Recipes.GetRecipe("InitDoTSeparateDamageRemove").Id, Iterations);

			Pool.Allocate(Recipes.GetRecipe("IntervalDamage_StackAddDamage").Id, Iterations);
		}

		[Benchmark]
		public void BenchNewBasicModifierFromRecipe()
		{
			var modifier = _initDamageRecipe.Create();
		}

		[Benchmark]
		public void BenchNewMediumModifierFromRecipe()
		{
			var modifier = _initDoTSeparateDamageRemoveRecipe.Create();
		}

		//[Benchmark]//TODO This one needs work manipulation, because we can overflow the limit
		public void BenchPooledMediumModifierFromRecipe()
		{
			var modifier = Pool.Rent(_initDoTSeparateDamageRemoveRecipe.Id);
		}

		[Benchmark]
		public void BenchPooledMediumModifierFromRecipeReturn()
		{
			var modifier = Pool.Rent(_initDoTSeparateDamageRemoveRecipe.Id);
			Pool.Return(modifier);
		}

		[Benchmark]
		public void BenchPooledFullStateModifierFromRecipeReturn()
		{
			var modifier = Pool.Rent(_intervalDamageStackAddDamageRecipe.Id);
			Pool.Return(modifier);
		}
	}
}