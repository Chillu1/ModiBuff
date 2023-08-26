namespace ModiBuff.Core.Units
{
	public interface IHealthCost<THealth>
	{
		void UseHealth(THealth value);
	}
}