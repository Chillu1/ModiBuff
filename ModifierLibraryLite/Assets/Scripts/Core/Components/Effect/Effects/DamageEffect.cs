namespace ModifierLibraryLite.Core
{
	public class DamageEffect : IEffect
	{
		private float _damage;

		public DamageEffect(float damage)
		{
			_damage = damage;
		}

		public void Effect(IUnit target, IUnit owner)
		{
			target.TakeDamage(_damage, owner);
		}
	}
}