namespace ModiBuff.Core.Units
{
	public interface IPoisonable
	{
		int PoisonStacks { get; }

		float TakeDamagePoison(float damage, int stacks, int totalStacks, IUnit source);
	}
}