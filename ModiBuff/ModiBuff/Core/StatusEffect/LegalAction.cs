using System;
using System.Linq;

namespace ModiBuff.Core
{
	[Flags]
	public enum LegalAction
	{
		None = 0,

		/// <summary>
		///     Attack, heal, whatever the main action is
		/// </summary>
		Act = 1,
		Cast = 2,
		Move = 4,

		/// <summary>
		///     Prioritize enemies/allies attacks/spells
		/// </summary>
		Prioritize = 8,
		Think = 16, //?

		Stun = Act | Cast | Move | Prioritize | Think,
		Freeze = Act | Move,
		Root = Move | Prioritize,
		Disarm = Act,
		Silence = Cast | Think,
		Taunt = Cast | Prioritize,

		All = Act | Cast | Move | Prioritize | Think
	}

	public static class LegalActionHelper
	{
		public static int BaseCount => Enum.GetValues(typeof(LegalAction)).Cast<LegalAction>()
			.Where(x => Utilities.Utilities.IsPowerOfTwo((int)x)).Count();
	}
}