namespace ModiBuff.Core.Units
{
	public interface IMultiInstanceStatusEffectController<in TLegalAction, in TStatusEffectType>
		: IUpdatableStatusEffectController<TLegalAction, TStatusEffectType>
	{
		void DispelStatusEffect(StatusEffectType statusEffectType, IUnit source);
		void DispelAll(IUnit source);
		void ChangeStatusEffect(int id, int genId, TStatusEffectType statusEffectType, float duration, IUnit source);
		void DecreaseStatusEffect(int id, int genId, TStatusEffectType statusEffectType, float duration, IUnit source);
	}
}