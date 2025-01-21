using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum ModifierAction
	{
		Refresh = 1,
		ResetStacks = 2,

		/// <summary>
		///		Often used with Core.TagType.CustomStack, so the modifier won't be stacked when added to a unit.
		/// </summary>
		Stack = 4,
		//Enable
		//Disable
	}
}