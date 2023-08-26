namespace ModiBuff.Core.Units
{
	public interface IManaOwner<TMana, TMaxMana>
	{
		TMana Mana { get; }
		TMaxMana MaxMana { get; }

		void UseMana(TMana value);
	}
}