using System.Collections.Generic;
using ModiBuff.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace ModiBuff.Tests
{
	public sealed class BenchSwitchArray
	{
		private const int Iterations = 50_000;

		[Test, Performance]
		public void BenchSwitch()
		{
			List<IEffect> whenAttackedEffects = new List<IEffect>();
			List<IEffect> whenCastEffects = new List<IEffect>();
			List<IEffect> whenDeathEffects = new List<IEffect>();
			List<IEffect> whenHealedEffects = new List<IEffect>();
			List<IEffect> onAttackEffects = new List<IEffect>();
			List<IEffect> onCastEffects = new List<IEffect>();
			List<IEffect> onKillEffects = new List<IEffect>();
			List<IEffect> onHealEffects = new List<IEffect>();

			var @event = EffectOnEvent.OnKill;
			var effect = new DamageEffect(5);

			Measure.Method(() =>
				{
					switch (@event)
					{
						case EffectOnEvent.WhenAttacked:
							whenAttackedEffects.Add(effect);
							break;
						case EffectOnEvent.WhenCast:
							whenCastEffects.Add(effect);
							break;
						case EffectOnEvent.WhenKilled:
							whenDeathEffects.Add(effect);
							break;
						case EffectOnEvent.WhenHealed:
							whenHealedEffects.Add(effect);
							break;
						case EffectOnEvent.OnAttack:
							onAttackEffects.Add(effect);
							break;
						case EffectOnEvent.OnCast:
							onCastEffects.Add(effect);
							break;
						case EffectOnEvent.OnKill:
							onKillEffects.Add(effect);
							break;
						case EffectOnEvent.OnHeal:
							onHealEffects.Add(effect);
							break;
						default:
							return;
					}
				})
				.CleanUp(() => onKillEffects.Clear())
				.BenchGC(Iterations);
		}

		[Test, Performance]
		public void BenchArray()
		{
			List<IEffect>[] effects = new List<IEffect>[(int)(EffectOnEvent.OnHeal + 1)];
			for (int i = 0; i < effects.Length; i++)
				effects[i] = new List<IEffect>();

			var @event = EffectOnEvent.OnKill;
			var effect = new DamageEffect(5);

			int index = (int)@event;

			Measure.Method(() => { effects[index].Add(effect); })
				.CleanUp(() => effects[index].Clear())
				.BenchGC(Iterations);
		}
	}
}