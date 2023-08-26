namespace ModiBuff.Core.Units
{
	//Might be a few issues with saving/loading damage, or reverting it if wanted
	public sealed class AddDamageOnKillMetaEffect : IMetaEffect<float>
	{
		private readonly float _damage;

		public Targeting Targeting { get; }

		public AddDamageOnKillMetaEffect(float damage, Targeting targeting = Targeting.TargetSource)
		{
			_damage = damage;
			Targeting = targeting;
		}

		public void Effect(float value, IUnit target, IUnit source, bool triggerEvents)
		{
			if (target is IDamagable<float, float> damagable && damagable.Health <= 0)
				((IAddDamage<float>)source).AddDamage(_damage);
		}
	}
}