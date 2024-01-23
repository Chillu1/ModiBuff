using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum ModifierAction
	{
		Refresh = 1,
		ResetStacks = 2,

		/// <summary>
		///		This modifier action might get removed
		/// </summary>
		Stack = 4,
		//Enable
		//Disable
	}
}