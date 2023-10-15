namespace ModiBuff.Core.Units
{
	public interface IAttacker<out TDamage, out TDamageReturnInfo>
	{
		TDamage Damage { get; }

		TDamageReturnInfo Attack(IUnit target);
	}
}