namespace ModiBuff.Core
{
	/// <summary>
	///		Modifier information that holds data only about immutable state of the modifier.
	/// </summary>
	public sealed class ModifierInfo
	{
		public readonly int Id;
		public readonly string InternalName;
		public readonly string DisplayName;
		public readonly string Description;

		public ModifierInfo(int id, string internalName, string displayName, string description)
		{
			Id = id;
			InternalName = internalName;
			DisplayName = displayName;
			Description = description;
		}
	}
}