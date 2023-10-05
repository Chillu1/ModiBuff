using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchNewModifier : ModifierBenches
	{
		private IModifierGenerator _initDamageRecipe;
		private IModifierGenerator _initDoTSeparateDamageRemoveRecipe;
		private int _initDoTSeparateDamageRemoveId;
		private IModifierGenerator _intervalDamageStackAddDamageRecipe;
		private IModifierGenerator _initDamageModifierGenerator;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_initDamageRecipe = Recipes.GetGenerator("InitDamage");
			_initDoTSeparateDamageRemoveRecipe = Recipes.GetGenerator("InitDoTSeparateDamageRemove");
			_initDoTSeparateDamageRemoveId = _initDoTSeparateDamageRemoveRecipe.Id;
			_intervalDamageStackAddDamageRecipe = Recipes.GetGenerator("IntervalDamage_StackAddDamage");

			_initDamageModifierGenerator = Recipes.GetGenerator("InitDamageManual");

			Pool.Clear();
			Pool.SetMaxPoolSize(1_000_000);
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

		[Benchmark]
		public void BenchPooledMediumModifierFromRecipeReturn()
		{
			var modifier = Pool.Rent(_initDoTSeparateDamageRemoveId);
			Pool.Return(modifier);
		}

		//[Benchmark]
		public void BenchPooledFullStateModifierFromRecipeReturn()
		{
			var modifier = Pool.Rent(_intervalDamageStackAddDamageRecipe.Id);
			Pool.Return(modifier);
		}

		[Benchmark]
		public void BenchNewModifierGeneratorManual()
		{
			var modifier = _initDamageModifierGenerator.Create();
		}
	}
}