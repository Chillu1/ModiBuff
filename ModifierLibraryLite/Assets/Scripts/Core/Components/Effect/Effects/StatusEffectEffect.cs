namespace ModifierLibraryLite.Core
{
	public sealed class StatusEffectEffect : IEffect
	{
		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			target.ChangeStatusEffect(_statusEffectType, _duration);
		}
	}
}