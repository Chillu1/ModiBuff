namespace ModiBuff.Core
{
	public interface IModifierGenerator //TODO Rename?
	{
		int Id { get; }
		string Name { get; }

		/// <summary>
		///		Do not call manually, use pool instead.
		/// </summary>
		Modifier Create(); //TODO Refactor so this can't be used outside of pool/by user
	}
}