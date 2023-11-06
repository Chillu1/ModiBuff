namespace ModiBuff.Core.Units
{
	public interface IMultiInstanceStatusEffectController<in TLegalAction, in TStatusEffectType>
		: IStatusEffectController<TLegalAction, TStatusEffectType>
	{
		void ChangeStatusEffect(int id, int genId, TStatusEffectType statusEffectType, float duration, IUnit source);
		void DecreaseStatusEffect(int id, int genId, TStatusEffectType statusEffectType, float duration, IUnit source);
		void TriggerAddEvent(StatusEffectEvent statusEffectEvent);
	}
}