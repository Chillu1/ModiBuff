namespace ModiBuff.Core
{
	public interface IModifierGenerator //TODO Rename?
	{
		int Id { get; }
		string Name { get; }

		Modifier Create();
	}
}