namespace ModiBuff.Core.Units
{
	public interface IAddDamage<in TDamage>
	{
		void AddDamage(TDamage damage);
	}
}