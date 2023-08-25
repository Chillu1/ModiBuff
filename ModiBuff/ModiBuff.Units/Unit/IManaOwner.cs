namespace ModiBuff.Core.Units
{
	public interface IManaOwner
	{
		float Mana { get; }
		float MaxMana { get; }

		void UseMana(float value);
	}
}