namespace ModiBuff.Core
{
	/// <summary>
	///		Special interface for setting up a modifier id in modifier effects
	/// </summary>
	public interface IModifierIdOwner
	{
		void SetModifierId(int id);
	}
}