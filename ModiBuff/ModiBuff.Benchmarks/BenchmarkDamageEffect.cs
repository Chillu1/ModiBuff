using ModiBuff.Core;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

namespace ModiBuff.Tests
{
	public sealed class BenchmarkDamageEffect : IEffect
	{
		private readonly float _damage;

		public BenchmarkDamageEffect(float damage) => _damage = damage;

		public void Effect(IUnit target, IUnit source)
		{
			if (!(target is IDamagable damagableTarget))
				return;

			damagableTarget.TakeDamage(_damage, source);
		}
	}
}