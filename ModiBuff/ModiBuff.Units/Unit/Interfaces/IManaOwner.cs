using System;

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

		public static bool FullMana(this IManaOwner<float, float> manaOwner)
		{
			return Math.Abs(manaOwner.Mana - manaOwner.MaxMana) < 0.001f;
		}
	}
}