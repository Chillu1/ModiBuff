using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	public sealed class BenchState
	{
		private const int Iterations = 50000;

		[Test, Performance]
		public void BenchFeedingTargets()
		{
			IUnit target = new Unit();
			IUnit source = new Unit();
			TargetComponent targetComponent = new TargetComponent(source, source, target);

			IEffect[] effects = { new DamageEffect(5), new DamageEffect(5) };
			var init = new InitComponentFeedFake(effects);

			Measure.Method(() => init.Init(targetComponent.Target, targetComponent.Owner))
				.BenchGC(Iterations);
		}

		private sealed class InitComponentFeedFake
		{
			private readonly IEffect[] _effects;

			public InitComponentFeedFake(IEffect[] effects) => _effects = effects;

			public void Init(IUnit target, IUnit owner)
			{
				int length = _effects.Length;
				for (int i = 0; i < length; i++)
					_effects[i].Effect(target, owner);
			}
		}

		[Test, Performance]
		public void BenchCachingTargets()
		{
			IUnit target = new Unit();
			IUnit source = new Unit();

			IEffect[] effects = { new DamageEffect(5), new DamageEffect(5) };
			var init = new InitComponentCacheFake(effects);
			init.SetupTarget(new TargetComponent(source, source, target));

			Measure.Method(() => init.Init())
				.BenchGC(Iterations);
		}

		private sealed class InitComponentCacheFake
		{
			private readonly IEffect[] _effects;
			private TargetComponent _targetComponent;

			public InitComponentCacheFake(IEffect[] effects) => _effects = effects;

			public void SetupTarget(TargetComponent targetComponent) => _targetComponent = targetComponent;

			public void Init()
			{
				int length = _effects.Length;
				for (int i = 0; i < length; i++)
					_effects[i].Effect(_targetComponent.Target, _targetComponent.Owner);
			}
		}
	}
}