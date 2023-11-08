namespace ModiBuff.Core.Units
{
	//Might be a few issues with saving/loading damage, or reverting it if wanted
	public sealed class AddDamageOnKillPostEffect : IPostEffect<float>
	{
		private readonly float _damage;
		private readonly Targeting _targeting;

		public AddDamageOnKillPostEffect(float damage, Targeting targeting = Targeting.TargetSource)
		{
			_damage = damage;
			_targeting = targeting;
		}

		public void Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);

			//Is damagable, health below 0, and health before attack was above 0
			if (source is IDamagable<float, float> damagable && damagable.Health <= 0 &&
			    damagable.Health + value > 0 && source is IKillable killable && killable.IsDead)
			{
				if (!(target is IAddDamage<float> addDamageTarget))
					return;

				addDamageTarget.AddDamage(_damage);
			}
		}
	}
}