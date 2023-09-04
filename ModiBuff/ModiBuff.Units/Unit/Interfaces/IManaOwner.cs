namespace ModiBuff.Core.Units
{
	public interface IManaOwner<TMana, out TMaxMana>
	{
		TMana Mana { get; }
		TMaxMana MaxMana { get; }

		void UseMana(TMana value);
	}

	public static class ManaOwnerExtensions
	{
		public static float PercentageMana(this IManaOwner<float, float> manaOwner)
		{
			return manaOwner.Mana / manaOwner.MaxMana;
		}
	}
}