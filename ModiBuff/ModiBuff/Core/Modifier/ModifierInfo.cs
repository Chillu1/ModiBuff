namespace ModiBuff.Core
{
	public sealed class ModifierInfo
	{
		public readonly int Id;
		public readonly string Name;

		public ModifierInfo(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}