namespace ModiBuff.Core.Units.Interfaces.NonGeneric
{
	/// <summary>
	///		Don't use, it's a helper interface for testing
	/// </summary>
	public interface IAllNonGeneric : IUnit, IAddDamage, IAttacker, IDamagable, IHealable, IHealer, IManaOwner,
		IStatusEffectOwner
	{
	}
}