namespace ModiBuff.Core
{
	/// <summary>
	///		Reverse of <see cref="DamageEffect"/>
	/// </summary>
	public sealed class ActerDamageEffect : IEventTrigger, IEffect
	{
		private readonly float _damage;
		private bool _isEventBased;

		public ActerDamageEffect(float damage) => _damage = damage;

		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit acter)
		{
			acter.TakeDamage(_damage, target, !_isEventBased);
		}
	}
}