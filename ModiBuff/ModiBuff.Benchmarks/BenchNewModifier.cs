using BenchmarkDotNet.Attributes;
using ModiBuff.Core;

namespace ModiBuff.Tests
{
	public sealed class BenchNewModifier : BaseModifierBenches
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

			//Pool.Clear();
			//var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			//Pool.SetMaxPoolSize(1_000_000);
			//Pool.Allocate(recipe.Id, 60 * Iterations);

			//Pool.Clear();
			//var recipe = Recipes.GetRecipe("InitDoTSeparateDamageRemove");
			//Pool.Allocate(recipe.Id, Iterations);

			//Pool.Clear();
			//var recipe = Recipes.GetRecipe("IntervalDamage_StackAddDamage");
			//Pool.Allocate(recipe.Id, Iterations);
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

		//[Benchmark]//TODO This one needs work manipulation, because we can overflow the limit
		public void BenchPooledMediumModifierFromRecipeReturn()
		{
			var modifier = Pool.Rent(_initDoTSeparateDamageRemoveRecipe.Id);
			Pool.Return(modifier);
		}

		//[Benchmark]//TODO This one needs work manipulation, because we can overflow the limit
		public void BenchPooledFullStateModifierFromRecipeReturn()
		{
			var modifier = Pool.Rent(_intervalDamageStackAddDamageRecipe.Id);
			Pool.Return(modifier);
		}
	}
}