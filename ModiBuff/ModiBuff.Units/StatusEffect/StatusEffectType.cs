using System;

namespace ModiBuff.Core.Units
{
	/// <summary>
	///     Special status effects changing Unit acts/lack of acts
	/// </summary>
	[Flags]
	public enum StatusEffectType
	{
		None = 0,
		Stun = 1,
		Freeze = 2,
		Root = 4,
		Disarm = 8,
		Silence = 16,
		Taunt = 32,
		Confuse = 64,
		Sleep = 128,
		//Hex?
		//Blind?
		//Mute?
		//Fear?
		//Break?
		//Airborne/Suspension (ranged attacks work)?
		//Stasis?
		//Charm?

		Last = Sleep
	}

	public static class StatusEffectExtensions
	{
		public static bool HasStatusEffect(this StatusEffectType initial, StatusEffectType check) =>
			(initial & check) != 0;
	}
}