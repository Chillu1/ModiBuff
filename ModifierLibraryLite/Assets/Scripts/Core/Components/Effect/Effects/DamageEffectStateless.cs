namespace ModifierLibraryLite.Core
{
	public sealed class DamageEffectStateless : IEffect
	{
		private readonly float _baseDamage;

		public DamageEffectStateless(float damage)
		{
			_baseDamage = damage;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			target.TakeDamage(_baseDamage, acter);
		}
	}
}