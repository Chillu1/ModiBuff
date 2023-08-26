namespace ModiBuff.Core.Units
{
	public interface IAttacker<TDamage, TDamageReturnInfo>
	{
		TDamage Damage { get; }

		TDamageReturnInfo Attack(IUnit target, bool triggersEvents = true);
	}
}