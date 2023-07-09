namespace ModiBuff.Core
{
	public interface IManaOwner
	{
		float Mana { get; }
		float MaxMana { get; }

		void UseMana(float value);
	}
}