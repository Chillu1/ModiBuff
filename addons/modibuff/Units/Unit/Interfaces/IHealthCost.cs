namespace ModiBuff.Core.Units
{
	public interface IHealthCost<in THealth>
	{
		void UseHealth(THealth value);
	}
}