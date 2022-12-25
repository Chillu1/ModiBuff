using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	public sealed class BenchEffectIteration
	{
		private const int Iterations = 50000;

		[Test, Performance]
		public void BenchInterfaceIEffectsArray()
		{
			IEffect[] effects =
			{
				new DamageEffect(5),
				new DamageEffect(5),
				new DamageEffect(5),
			};

			IUnit unit = new Unit();

			Measure.Method(() =>
				{
					for (int i = 0; i < effects.Length; i++)
						effects[i].Effect(unit, unit);
				})
				.BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchDelegateEffectsArray()
		{
			var actions = new Action<IUnit, IUnit>[]
			{
				(target, acter) => { target.TakeDamage(5, acter); },
				(target, acter) => { target.TakeDamage(5, acter); },
				(target, acter) => { target.TakeDamage(5, acter); },
			};

			IUnit unit = new Unit();

			Measure.Method(() =>
				{
					for (int i = 0; i < actions.Length; i++)
						actions[i](unit, unit);
				})
				.BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchEffectsArray()
		{
			var effects = new[]
			{
				new DamageEffect(5),
				new DamageEffect(5),
				new DamageEffect(5),
			};

			IUnit unit = new Unit();

			Measure.Method(() =>
				{
					for (int i = 0; i < effects.Length; i++)
						effects[i].Effect(unit, unit);
				})
				.BenchGC(Iterations);
		}
	}
}