namespace ModiBuff.Core
{
	public static class ModifierControllerExtensions
	{
		/// <summary>
		///		Implicitly converts double delta to double, this shouldn't be an issue with our timers. 
		/// </summary>
		/// <remarks>
		///		Timers are the only things that should be updated here, but if we're updating something like
		///		a position, that has a big value where precision is important, we'll run into trouble.
		///		If there are any other issues, message @Chillu1 on GitHub/make an issue about it.
		///		<para>
		///		Or if there's mostly only advantages to using double-point precision in our case.
		///		</para>
		/// </remarks>
		public static void Update(this ModifierController modifierController, double delta)
		{
			modifierController.Update((float)delta);
		}
	}
}