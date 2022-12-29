using ModiBuff.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	public sealed class BenchChecks : BaseModifierTests
	{
		private const int Iterations = 100_000;

		[Test, Performance]
		public void BenchInitDamage()
		{
			var modifier = Recipes.GetRecipe("InitDamage").Create();
			modifier.SetTargets(Unit, Unit);

			Measure.Method(() => { modifier.Init(); })
				.Bench(Iterations);
		}

		[Test, Performance]
		public void BenchCheckInitDamage()
		{
			var checks = new ModifierCheck[ModifierRecipes.RecipesCount];
			var check = new ModifierCheck(0, "Test");
			var modifier = Recipes.GetRecipe("InitDamage").Create();
			modifier.SetTargets(Unit, Unit);
			checks[modifier.Id] = check;

			Measure.Method(() =>
				{
					if (checks[modifier.Id].Check(Unit))
						modifier.Init();
				})
				.Bench(Iterations);
		}

		private sealed class TestCheckModifier : IModifier
		{
			private readonly bool _init;

			private readonly ModifierCheck _check;
			private readonly InitComponent _initComponent;

			private TargetComponent _targetComponent;

			public TestCheckModifier(InitComponent initComponent, IUnit target)
			{
				_check = new ModifierCheck(0, "Test");
				_targetComponent = new TargetComponent(target, target);

				if (initComponent != null)
				{
					_initComponent = initComponent;
					_init = true;
				}
			}

			public int Id { get; }
			public string Name { get; }

			public void SetTargets(IUnit target, IUnit acter)
			{
			}

			public void Init()
			{
				if (!_init)
					return;

				if (!_check.Check(_targetComponent.Target))
					return;

				_initComponent.Init(_targetComponent.Target, _targetComponent.Acter);
			}

			public void Update(float deltaTime)
			{
			}

			public void Refresh()
			{
			}

			public void Stack()
			{
			}

			public void ResetState()
			{
			}
		}

		[Test, Performance]
		public void BenchInterfaceCheckInitDamage()
		{
			IModifier modifier = new TestCheckModifier(new InitComponent(false, new DamageEffect(5), null), Unit);

			Measure.Method(() => { modifier.Init(); })
				.Bench(Iterations);
		}

		private sealed class TestModifier : IModifier
		{
			private readonly bool _init;

			private readonly InitComponent _initComponent;

			private TargetComponent _targetComponent;

			public TestModifier(InitComponent initComponent, IUnit target)
			{
				_targetComponent = new TargetComponent(target, target);

				if (initComponent != null)
				{
					_initComponent = initComponent;
					_init = true;
				}
			}

			public int Id { get; }
			public string Name { get; }

			public void SetTargets(IUnit target, IUnit acter)
			{
			}

			public void Init()
			{
				if (!_init)
					return;

				_initComponent.Init(_targetComponent.Target, _targetComponent.Acter);
			}

			public void Update(float deltaTime)
			{
			}

			public void Refresh()
			{
			}

			public void Stack()
			{
			}

			public void ResetState()
			{
			}
		}

		[Test, Performance]
		public void BenchInterfaceInitDamage()
		{
			IModifier modifier = new TestModifier(new InitComponent(false, new DamageEffect(5), null), Unit);

			Measure.Method(() => { modifier.Init(); })
				.Bench(Iterations);
		}

		private class BaseModifier : IModifier
		{
			private readonly bool _init;

			private readonly InitComponent _initComponent;

			protected TargetComponent TargetComponent;

			public BaseModifier(InitComponent initComponent, IUnit target)
			{
				TargetComponent = new TargetComponent(target, target);

				if (initComponent != null)
				{
					_initComponent = initComponent;
					_init = true;
				}
			}

			public int Id { get; }
			public string Name { get; }

			public void SetTargets(IUnit target, IUnit acter)
			{
			}

			public void Init()
			{
				if (!_init)
					return;

				_initComponent.Init(TargetComponent.Target, TargetComponent.Acter);
			}

			public void Update(float deltaTime)
			{
			}

			public void Refresh()
			{
			}

			public void Stack()
			{
			}

			public void ResetState()
			{
			}
		}

		private class DerivedCheckModifier : BaseModifier
		{
			private readonly ModifierCheck _check;

			public DerivedCheckModifier(InitComponent initComponent, IUnit target) : base(initComponent, target)
			{
				_check = new ModifierCheck(0, "Test");
			}

			public new void Init()
			{
				if (!_check.Check(TargetComponent.Target))
					return;

				base.Init();
			}
		}

		[Test, Performance]
		public void BenchBaseInitDamage()
		{
			IModifier modifier = new BaseModifier(new InitComponent(false, new DamageEffect(5), null), Unit);

			Measure.Method(() => { modifier.Init(); })
				.Bench(Iterations);
		}

		[Test, Performance]
		public void BenchDerivedCheckInitDamage()
		{
			IModifier modifier = new DerivedCheckModifier(new InitComponent(false, new DamageEffect(5), null), Unit);

			Measure.Method(() => { modifier.Init(); })
				.Bench(Iterations);
		}

		[Test, Performance]
		public void BenchMultipleCombination()
		{
			Unit.TryAddModifierSelf("InitFreeze");
			Unit.TryAddModifierSelf("Flag");

			Measure.Method(() => { Unit.TryAddModifierSelf("InitDamage_EffectCondition_Combination"); })
				.Bench();
		}
	}
}