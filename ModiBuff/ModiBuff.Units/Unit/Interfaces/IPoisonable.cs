namespace ModiBuff.Core.Units
{
	public interface IPoisonable
	{
		float TakeDamagePoison(float damage, int stacks, IUnit source);
	}
}