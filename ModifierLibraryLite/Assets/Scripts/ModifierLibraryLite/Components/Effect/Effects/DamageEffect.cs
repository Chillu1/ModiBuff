namespace ModifierLibraryLite
{
	public class DamageEffect : IEffect
	{
		private float _damage;

		public DamageEffect(float damage)
		{
			_damage = damage;
		}

		public void Effect(IUnit target)
		{
			target.TakeDamage(_damage);
		}
	}
}