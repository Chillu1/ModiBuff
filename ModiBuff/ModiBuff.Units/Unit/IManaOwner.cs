namespace ModiBuff.Core.Units
{
	public interface IManaOwner<TMana>
	{
		TMana Mana { get; }
		TMana MaxMana { get; }

		void UseMana(TMana value);
	}
}